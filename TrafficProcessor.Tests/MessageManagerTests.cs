using TrafficProcessor.Data;
using Moq;
using Microsoft.Extensions.Logging;
using System.Net;

namespace TrafficProcessor.Tests;

class MessageManagerTests
{
    [Test]
    public async Task CheckMultipleRead()
    {
        var manager = new MessageManager<Response>(Mock.Of<ILogger<MessageManager<Response>>>());

        string id = "123";
        var content = "999";
        var read1 = manager.Read(id, out var isAdded1, CancellationToken.None);
        var read2 = manager.Read(id, out var isAdded2, CancellationToken.None);

        Task.Run(async () =>
        {
            await Task.Delay(1000);
            await manager.Write(id, new Response() { StatusCode = HttpStatusCode.OK, Content = content }, CancellationToken.None);
        });

        var readResults = await Task.WhenAll(read1.AsTask(), read2.AsTask());
        Assert.True(isAdded1 ^ isAdded2);
        Assert.True(readResults.All(res => res.StatusCode == HttpStatusCode.OK && res.Content == content));
    }

    [Test]
    public async Task CheckMultipleWrite()
    {
        var manager = new MessageManager<Response>(Mock.Of<ILogger<MessageManager<Response>>>());

        string id1 = "123";
        string id2 = "456";
        var content1 = "666";
        var content2 = "999";

        var read1 = manager.Read(id1, out var _, CancellationToken.None);
        var read2 = manager.Read(id2, out var _, CancellationToken.None);

        Task.Run(async () =>
        {
            await Task.Delay(1000);
            await manager.Write(id1, new Response() { StatusCode = HttpStatusCode.OK, Content = content1 }, CancellationToken.None);
        });

        Task.Run(async () =>
        {
            await Task.Delay(1000);
            await manager.Write(id2, new Response() { StatusCode = HttpStatusCode.BadRequest, Content = content2 }, CancellationToken.None);
        });

        var result1 = await read1;
        var result2 = await read2;

        Assert.That(result1.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result1.Content, Is.EqualTo(content1));
        Assert.That(result2.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(result2.Content, Is.EqualTo(content2));
    }
}
