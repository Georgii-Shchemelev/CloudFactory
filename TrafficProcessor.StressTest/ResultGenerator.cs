namespace TrafficProcessor.StressTest;

class ResultGenerator
{
    string _directory { get; } = Environment.ExpandEnvironmentVariables("%programdata%\\MessageBroker");

    public async Task GenerateResponses()
    {
        while (true)
        {
            await Task.Delay(10000);

            foreach (var file in Directory.GetFiles(_directory, "*.req"))
            {
                var responseFile = Path.ChangeExtension(file, ".resp");
                var content = "OK" + Environment.NewLine + "Success";
                await File.WriteAllTextAsync(responseFile, content);
            }
        }
    }
}
