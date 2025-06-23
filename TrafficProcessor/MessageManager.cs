using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace TrafficProcessor;

public interface IMessageManager<T>
{
    public ValueTask<T> Read(string correlationId, out bool isNew, CancellationToken token);
    Task Write(string correlationId, T data, CancellationToken token);
}

public class MessageManager<T> : IMessageManager<T>
{
    ConcurrentDictionary<string, List<Channel<T>>> Channels = new();
    public ILogger<MessageManager<T>> Logger { get; }

    public MessageManager(ILogger<MessageManager<T>> logger)
    {
        Logger = logger;
    }

    public ValueTask<T> Read(string correlationId, out bool isNew, CancellationToken token)
    {
        Logger.LogDebug("Start reading response {CorrelationId}", correlationId);
        bool isAdded = false;

        var channel = Channel.CreateUnbounded<T>();
        Channels.AddOrUpdate(correlationId,
            _ =>
            {
                isAdded = true;
                return new List<Channel<T>>() { channel };
            },
            (_, list) =>
            {
                list.Add(channel);
                return list;
            });

        isNew = isAdded;

        return channel.Reader.ReadAsync(token);
    }

    public async Task Write(string correlationId, T data, CancellationToken token)
    {
        Logger.LogDebug("Writing response {CorrelationId}", correlationId);

        Channels.Remove(correlationId, out var keyChannels);
        if (keyChannels == null)
        {
            Logger.LogWarning("Channel {CorrelationId} not found", correlationId);
            return;
        }

        foreach (var channel in keyChannels)
        {
            await channel.Writer.WriteAsync(data, token);
            channel.Writer.Complete();
        }
    }
}
