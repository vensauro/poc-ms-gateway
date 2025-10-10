using Microsoft.AspNetCore.Mvc;
using PocMsGateway.Messaging;
using PocMsGateway.DTOs;

[Route("publish")]
[ApiController]
public class EventsController : ControllerBase
{
    private readonly IMessagePublisher _publisher;

    public EventsController(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    [HttpPost("tasks")]
    public async Task<IActionResult> CreateTask([FromBody] TaskRequest request)
    {
        var evt = new BaseEvent<TaskCreatedData>
        {
            Type = "task.create",
            UserId = request.UserId,
            Data = new TaskCreatedData
            {
                Description = request.Description,
                CreatedAt = request.CreatedAt
            },
            OccurredAt = DateTime.UtcNow.ToString("o")
        };

        await _publisher.PublishEvent(evt, "task_queue");
        return Ok(new { Message = "Task enviada para fila!" });
    }

    [HttpPost("notifications")]
    public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
    {
        var evt = new BaseEvent<NotificationData>
        {
            Type = "notification.send",
            UserId = request.UserId,
            Data = new NotificationData
            {
                Title = request.Title,
                Message = request.Message,
                CreatedAt = request.CreatedAt
            },
            OccurredAt = DateTime.UtcNow.ToString("o")
        };
        await _publisher.PublishEvent(evt, "notification_queue");
        return Ok(new { Message = "Notification message published!" });
    }
}
