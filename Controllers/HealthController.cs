using Microsoft.AspNetCore.Mvc;

namespace PocMsGateway.Controllers;

[Route("test")]
[ApiController]
public class WeatherForecastController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { Message = "API está funcionando!" });
    }
}
