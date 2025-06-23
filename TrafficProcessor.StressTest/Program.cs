// See https://aka.ms/new-console-template for more information


using TrafficProcessor.StressTest;

int clientsAmount = 10;
string address = "https://localhost:7110/test";

var clients = Enumerable.Repeat(0, clientsAmount).Select(_ => new ClientWrapper()).ToList();
var resultGenerator = new ResultGenerator();
resultGenerator.GenerateResponses();

while (true)
{
    await Task.Delay(1000);
    var tasks = clients.Select(cl => cl.Send(address, HttpMethod.Get, CancellationToken.None));

    await Task.WhenAll(tasks);
}




