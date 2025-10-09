using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PocMsGateway.Messaging;
using PocMsGateway.DTOs;

[Route("publish")]
[ApiController]
public class PublishController : ControllerBase
{
    private readonly IMessagePublisher _publishMessage;

    public PublishController(IMessagePublisher publishMessage)
    {
        _publishMessage = publishMessage;
    }

    [HttpPost("tasks")]
    public async Task<IActionResult> CreateTask([FromBody] TaskRequest request)
    {
        await _publishMessage.PublishMessage(
            new ResourcePublisher(request.Name, request.Description)
        );
        return Ok(new { Message = "Task message published!" });
    }

    [HttpPost("notifications")]
    public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
    {
        await _publishMessage.PublishMessage(
            new ResourcePublisher(request.Title, request.Message)
        );
        return Ok(new { Message = "Notification message published!" });
    }
}
