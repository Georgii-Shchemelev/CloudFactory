using Microsoft.Extensions.Logging;
using TrafficProcessor.Data;
using TrafficProcessor.Infrastructure;

namespace TrafficProcessor.Processors;

public class AdvancedHttpTrafficProcessor : HttpTrafficProcessor
{
    public AdvancedHttpTrafficProcessor(IMessageManager<Response> messageManager, IMessageBroker messageBroker, ILogger<AdvancedHttpTrafficProcessor> logger)
        : base(messageManager, messageBroker, logger)
    {
    }

    protected override string GetCorrelationId(Request request)
    {
        return GetMessageKey(request);
    }
}
