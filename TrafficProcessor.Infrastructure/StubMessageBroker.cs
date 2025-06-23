using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TrafficProcessor.Infrastructure;

public interface IMessageBroker
{
    Task Send(string key, string request, CancellationToken token);
}

public class StubMessageBroker : BackgroundService, IMessageBroker
{

    private readonly IEnumerable<Func<IMessageHandler>> _handlers;
    private readonly MessageBrokerOptions _options;

    public ILogger<StubMessageBroker> Logger { get; }

    public StubMessageBroker(IOptions<MessageBrokerOptions> options, IEnumerable<Func<IMessageHandler>> handlers, ILogger<StubMessageBroker> logger)
    {
        _options = options.Value; ;
        _handlers = handlers.ToList();
        Logger = logger;
        CreateDirectory();        
    }

    public async Task Send(string key, string request, CancellationToken token)
    {
        Logger.LogDebug("Sending requst {Key}", key);
        var file = Path.Combine(_options.Directory, key + _options.RequestExtension);
        if (File.Exists(file))
        {
            Logger.LogWarning("File with the sam key {MessageKey} already exists", key);
            return;
        }
        await File.WriteAllTextAsync(file, request, token);        
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(_options.DirectoryCheckPeriod, token);

            try
            {
                await CheckForResponses(token);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while processing responses");
            }
        }
    }

    private async Task CheckForResponses(CancellationToken token)
    {
        foreach (var file in Directory.GetFiles(_options.Directory, _options.ResponseFilter))
        {
            await ProcessFile(file, token);
        }
    }

    private async Task ProcessFile(string file, CancellationToken token)
    {
        var key = Path.GetFileNameWithoutExtension(file);
        Logger.LogDebug("Recieved rsponse {Key}", key);

        var content = await File.ReadAllTextAsync(file);

        DeleteFiles(file);

        foreach (var handler in _handlers)
        {
            await handler().Handle(key, content, token);
        }
    }

    private void DeleteFiles(string responseFile)
    {
        File.Delete(responseFile);
        var requestFile = Path.ChangeExtension(responseFile, _options.RequestExtension);
        if (File.Exists(requestFile))
            File.Delete(requestFile);
    }

    private void CreateDirectory()
    {
        if (!Directory.Exists(_options.Directory))
            Directory.CreateDirectory(_options.Directory);
    }
}
