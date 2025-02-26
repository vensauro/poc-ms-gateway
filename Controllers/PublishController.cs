using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DotnetApi.Messages;


[Route("publish")]
[ApiController]
public class PublishController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;

    public PublishController(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost]
    public async Task<IActionResult> PublishResource([FromBody] ResourceCreated resource)
    {
        await _publishEndpoint.Publish(resource);
        return Ok(new { Message = "Resource published!" });
    }
}
