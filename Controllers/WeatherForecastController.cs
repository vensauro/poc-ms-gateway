using Microsoft.AspNetCore.Mvc;

namespace PocMsGateway.Controllers;

[ApiController]
[Route("api/test")]
public class WeatherForecastController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { Message = "API está funcionando!" });
    }
}
