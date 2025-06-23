namespace TrafficProcessor.Infrastructure;

public interface IMessageHandler
{
    Task Handle(string key, string value, CancellationToken token);
}
