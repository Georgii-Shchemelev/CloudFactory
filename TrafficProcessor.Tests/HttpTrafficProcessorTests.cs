using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using TrafficProcessor.Data;
using TrafficProcessor.Infrastructure;
using TrafficProcessor.Processors;

namespace TrafficProcessor.Tests;

class HttpTrafficProcessorTests
{
    [Test]
    public async Task ProcessRequestTest()
    {
        var messageBrokerMock = new Mock<IMessageBroker>();
        var messageManagerMock = new Mock<IMessageManager<Response>>();

        var trafficProcessor = new PrimitiveHttpTrafficProcessor(messageManagerMock.Object, messageBrokerMock.Object, Mock.Of<ILogger<PrimitiveHttpTrafficProcessor>>());
        bool isNew = true;
        var response = new Response();
        messageManagerMock.Setup(mock => mock.Read(It.IsAny<string>(), out isNew, CancellationToken.None))
            .Returns(ValueTask.FromResult(response));

        var request = new Request() { Method = "GET", Path = "/test" };
        var result = await trafficProcessor.ProcessRequest(request, CancellationToken.None);

        var expectedContent = request.Method + Environment.NewLine + request.Path;
        messageBrokerMock.Verify(mock => mock.Send(It.IsAny<string>(), It.Is<string>(s => s == expectedContent), CancellationToken.None), Times.Once);
        Assert.That(response, Is.EqualTo(result));
    }

    [Test]
    public async Task ProcessRequestCheckForDoubleTest()
    {
        var messageBrokerMock = new Mock<IMessageBroker>();
        var messageManagerMock = new Mock<IMessageManager<Response>>();

        var trafficProcessor = new AdvancedHttpTrafficProcessor(messageManagerMock.Object, messageBrokerMock.Object, Mock.Of<ILogger<AdvancedHttpTrafficProcessor>>());
        bool isNew = false;
        var response = new Response();
        messageManagerMock.Setup(mock => mock.Read(It.IsAny<string>(), out isNew, CancellationToken.None))
            .Returns(ValueTask.FromResult(response));

        var request = new Request() { Method = "GET", Path = "/test" };
        var result = await trafficProcessor.ProcessRequest(request, CancellationToken.None);

        var expectedContent = request.Method + Environment.NewLine + request.Path;
        messageBrokerMock.Verify(mock => mock.Send(It.IsAny<string>(), It.Is<string>(s => s == expectedContent), CancellationToken.None), Times.Never);
        Assert.That(response, Is.EqualTo(result));
    }

    [Test]
    public async Task ProcessResponseTest()
    {
        var messageBrokerMock = new Mock<IMessageBroker>();
        var messageManagerMock = new Mock<IMessageManager<Response>>();

        var trafficProcessor = new PrimitiveHttpTrafficProcessor(messageManagerMock.Object, messageBrokerMock.Object, Mock.Of<ILogger<PrimitiveHttpTrafficProcessor>>());
        string key = "123";
        string correlationId = "456";
        HttpTrafficProcessor.KeyToCorrelationId.Add(key, correlationId);
        var responseContent = "888";
        var fileContent = "200" + Environment.NewLine + responseContent;
       
        await trafficProcessor.Handle(key, fileContent, CancellationToken.None);


        messageManagerMock.Verify(mock => mock.Write(It.Is<string>(s => s == correlationId), It.Is<Response>(r => r.StatusCode == HttpStatusCode.OK && r.Content == responseContent), CancellationToken.None), Times.Once);
        
        Assert.False(HttpTrafficProcessor.KeyToCorrelationId.ContainsKey(key));
    }

}
