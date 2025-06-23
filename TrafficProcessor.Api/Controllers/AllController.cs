using Microsoft.AspNetCore.Mvc;
using TrafficProcessor.Data;

namespace TrafficProcessor.Api.Controllers;


[ApiController]
public class AllController : ControllerBase
{
    public IHttpTrafficProcessor TrafficProcessor { get; }
    public ILogger<AllController> Logger { get; }

    public AllController(IHttpTrafficProcessor trafficProcessor, ILogger<AllController> logger)
    {
        TrafficProcessor = trafficProcessor;
        Logger = logger;
    }

    [Route("{*url}")]
    public async Task<IActionResult> Index(CancellationToken token)
    {
        Logger.LogDebug("Request recieved {Method} {Path}", Request.Method, Request.Path);

        var request = new Request() { Method = Request.Method, Path = Request.Path };
        var response = await TrafficProcessor.ProcessRequest(request, token);
        Logger.LogDebug("Response sending {Method} {Path} {Status}", Request.Method, Request.Path, response.StatusCode);

        return StatusCode((int)response.StatusCode, response.Content);
    }
}
