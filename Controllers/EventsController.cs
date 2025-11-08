using Microsoft.AspNetCore.Mvc;
using PocMsGateway.Messaging;
using PocMsGateway.DTOs;
using Microsoft.AspNetCore.Authorization;

[Route("publish")]
[ApiController]
public class EventsController : ControllerBase
{
    private readonly IMessagePublisher _publisher;
    private readonly IJwtContext _jwt;

    public EventsController(IMessagePublisher publisher, IJwtContext jwt)
    {
        _publisher = publisher;
        _jwt = jwt;
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("tasks/create")]
    public async Task<IActionResult> CreateTask([FromBody] TaskRequest request)
    {
        var userId = _jwt.UserId;

        var evt = new BaseEvent<TaskCreatedData>
        {
            Type = "task.create",
            UserId = userId ?? string.Empty,
            Data = new TaskCreatedData
            {
                Description = request.Description,
                ExpiredAt = request.ExpiredAt,
                Category = request.Category
            },
            OccurredAt = DateTime.UtcNow.ToString("o")
        };

        await _publisher.PublishEventAsync("task_queue", evt);
        return Ok(new { Message = "Task enviada para fila!" });
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("tasks/delete")]
    public async Task<IActionResult> DeleteTask([FromBody] DeleteTaskRequest request)
    {
        var userId = _jwt.UserId;

        var evt = new BaseEvent<TaskDeletePayload>
        {
            Type = "task.delete",
            UserId = userId ?? string.Empty,
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
