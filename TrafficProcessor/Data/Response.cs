using System.Net;

namespace TrafficProcessor.Data;

public class Response
{
    public HttpStatusCode StatusCode { get; set; }
    public string? Content { get; set; }
}
