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

    public EventsController(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("tasks/create")]
    public async Task<IActionResult> CreateTask([FromBody] TaskRequest request)
    {
        var userId = User.FindFirst("user_id")?.Value;
        var deviceToken = User.FindFirst("device_token")?.Value;

        Console.WriteLine($"Categoria criada pelo usuário: {userId}");

        var evt = new BaseEvent<TaskCreatedData>
        {
            Type = "task.create",
            Data = new TaskCreatedData
            {
                Description = request.Description,
                ExpiredAt = request.ExpiredAt,
                CategoryId = request.CategoryId,
                UserId = userId ?? string.Empty,
                DeviceToken = deviceToken ?? string.Empty,
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
        var userId = User.FindFirst("user_id")?.Value;

        Console.WriteLine($"Categoria criada pelo usuário: {userId}");

        var evt = new BaseEvent<TaskDeleteData>
        {
            Type = "task.delete",
            Data = new TaskDeleteData
            {
                TaskId = request.TaskId,
                UserId = userId ?? string.Empty,
            },
            OccurredAt = DateTime.UtcNow.ToString("o")
        };

        await _publisher.PublishEventAsync("task_queue", evt);
        return Ok(new { Message = "Task delete command published!" });
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("categories/create")]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryRequest request)
    {
        var userId = User.FindFirst("user_id")?.Value;

        Console.WriteLine("Categoria criada pelo usuário: {0}", userId);

        var evt = new BaseEvent<CategoryCreatedData>
        {
            Type = "category.create",
            Data = new CategoryCreatedData
            {
                Name = request.Name,
                UserId = userId ?? string.Empty,
            },
            OccurredAt = DateTime.UtcNow.ToString("o")
        };

        await _publisher.PublishEventAsync("category_queue", evt);
        return Ok(new { Message = "Categoria enviada para fila!" });
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("categories/delete")]
    public async Task<IActionResult> DeleteCategory([FromBody] DeleteCategoryRequest request)
    {
        var userId = User.FindFirst("user_id")?.Value;

        Console.WriteLine($"Categoria criada pelo usuário: {userId}");

        var evt = new BaseEvent<CategoryDeleteData>
        {
            Type = "category.delete",
            Data = new CategoryDeleteData
            {
                CategoryId = request.CategoryId,
                UserId = userId ?? string.Empty,
            },
            OccurredAt = DateTime.UtcNow.ToString("o")
        };

        await _publisher.PublishEventAsync("category_queue", evt);
        return Ok(new { Message = "Category delete command published!" });
    }
}
