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

    [HttpPost("tasks/create")]
    public async Task<IActionResult> CreateTask([FromBody] TaskRequest request)
    {
        var evt = new BaseEvent<TaskCreatedData>
        {
            Type = "task.create",
            UserId = request.UserId,
            Data = new TaskCreatedData
            {
                Description = request.Description,
                ExpiredAt = request.ExpiredAt
            },
            OccurredAt = DateTime.UtcNow.ToString("o")
        };

        await _publisher.PublishEventAsync("task_queue", evt);
        return Ok(new { Message = "Task enviada para fila!" });
    }

    [HttpPost("tasks/delete")]
    public async Task<IActionResult> DeleteTask([FromBody] DeleteTaskRequest request)
    {
        var evt = new BaseEvent<TaskDeletePayload>
        {
            Type = "task.delete",
            UserId = request.UserId,
            Data = new TaskDeletePayload
            {
                TaskId = request.TaskId
            },
            OccurredAt = DateTime.UtcNow.ToString("o")
        };

        await _publisher.PublishEventAsync("task_queue", evt);
        return Ok(new { Message = "Task delete command published!" });
    }
}
