namespace TrafficProcessor.StressTest;

class ClientWrapper
{
    HttpClient Client { get; } = new();

    public async Task Send(string address, HttpMethod method, CancellationToken token)
    {
        var request = new HttpRequestMessage(method, address);
        await Client.SendAsync(request, token);
    }
}
