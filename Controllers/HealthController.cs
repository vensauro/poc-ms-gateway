using Microsoft.AspNetCore.Mvc;

namespace PocMsGateway.Controllers;

[Route("health-ctrl")]
[ApiController]
public class WeatherForecastController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { Message = "API está funcionando!" });
    }
}
