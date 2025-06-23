using Microsoft.Extensions.Logging;
using TrafficProcessor.Data;
using TrafficProcessor.Infrastructure;

namespace TrafficProcessor.Processors;


public class PrimitiveHttpTrafficProcessor : HttpTrafficProcessor
{
    public PrimitiveHttpTrafficProcessor(IMessageManager<Response> messageManager, IMessageBroker messageBroker, ILogger<PrimitiveHttpTrafficProcessor> logger)
        : base(messageManager, messageBroker, logger)
    {
    }

    protected override string GetCorrelationId(Request request)
    {
        return Guid.NewGuid().ToString();
    }
}
