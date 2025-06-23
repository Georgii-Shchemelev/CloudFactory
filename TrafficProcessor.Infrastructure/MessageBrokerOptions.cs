namespace TrafficProcessor.Infrastructure;

public class MessageBrokerOptions
{
    private string _directory = "";

    public string Directory { get => _directory; set => _directory = Environment.ExpandEnvironmentVariables(value); }
    public string ResponseFilter { get; set; } = "*.resp";
    public string RequestExtension { get; set; } = ".req";
    public int DirectoryCheckPeriod { get; set; } = 3000;
}
