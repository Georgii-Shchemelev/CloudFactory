using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using TrafficProcessor.Data;
using TrafficProcessor.Infrastructure;


namespace TrafficProcessor;

public interface IHttpTrafficProcessor : IMessageHandler
{
    Task<Response> ProcessRequest(Request request, CancellationToken token);
}

public abstract class HttpTrafficProcessor : IHttpTrafficProcessor
{
    public IMessageManager<Response> MessageManager { get; }
    public IMessageBroker MessageBroker { get; }
    public ILogger<HttpTrafficProcessor> Logger { get; }
    internal static Dictionary<string, string> KeyToCorrelationId { get; } = new();

    public HttpTrafficProcessor(IMessageManager<Response> messageManager, IMessageBroker messageBroker, ILogger<HttpTrafficProcessor> logger)
    {
        MessageManager = messageManager;
        MessageBroker = messageBroker;
        Logger = logger;
    }

    public async Task<Response> ProcessRequest(Request request, CancellationToken token)
    {
        var messageKey = GetMessageKey(request);
        var content = GenerateRequestContent(request);
        var correlationId = GetCorrelationId(request);

        Logger.LogDebug("Reading result for {Key} {CorrelationId}", messageKey, correlationId);

        var readTask = MessageManager.Read(correlationId, out bool isAdded, token);
        if (isAdded)
        {
            KeyToCorrelationId[messageKey] = correlationId;
            await MessageBroker.Send(messageKey, content, token);
        }

        return await readTask;
    }

    public async Task Handle(string key, string value, CancellationToken token)
    {
        var correlationId = KeyToCorrelationId!.GetValueOrDefault(key, null);
        KeyToCorrelationId.Remove(key);
        if (key == null)
        {
            Logger.LogError("No key {MessageKey} was found", key);
            return;
        }

        Logger.LogDebug("Writing result for {Key} {CorrelationId}", key, correlationId);
        await MessageManager.Write(correlationId, ParseResponse(value), token);
    }

    private Response ParseResponse(string content)
    {
        try
        {
            using var sr = new StringReader(content);
            return new Response()
            {
                StatusCode = Enum.Parse<HttpStatusCode>(sr.ReadLine()),
                Content = sr.ReadToEnd()
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error parsing response content {ResponseContent}", content);
            return new Response()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = content
            };
        }
    }

    protected abstract string GetCorrelationId(Request request);

    protected string GetMessageKey(Request request)
    {
        var key = request.Method + request.Path;
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(key));
        return BitConverter.ToString(MD5.Create().ComputeHash(stream)).Replace("-", "").ToLower();
    }

    protected string GenerateRequestContent(Request request)
    {
        using var sw = new StringWriter();
        sw.WriteLine(request.Method);
        sw.Write(request.Path);
        return sw.ToString();
    }
}
