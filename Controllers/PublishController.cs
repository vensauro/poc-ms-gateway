using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PocMsGateway.Messaging;

[Route("publish")]
[ApiController]
public class PublishController : ControllerBase
{
    private readonly IMessagePublisher _publishMessage;

    public PublishController(IMessagePublisher publishMessage)
    {
        _publishMessage = publishMessage;
    }

    [HttpPost]
    public async Task<IActionResult> PublishResource([FromBody] ResourceCreated resource)
    {
        await _publishMessage.PublishMessage(resource);
        return Ok(new { Message = "Resource published!" });
    }
}
