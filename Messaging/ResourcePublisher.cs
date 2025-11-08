using System.IdentityModel.Tokens.Jwt;
using MassTransit;
using PocMsGateway.DTOs;
using Saunter.Attributes;

namespace PocMsGateway.Messaging;

public interface IMessagePublisher
{
    Task PublishEventAsync<T>(string queueName, BaseEvent<T> evt);
}

public class MessagePublisher(
    IPublishEndpoint publishEndpoint,
    ILogger<MessagePublisher> logger,
    IHttpContextAccessor httpContextAccessor
    ) : IMessagePublisher
{
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ILogger<MessagePublisher> _logger = logger;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task PublishEventAsync<T>(string queueName, BaseEvent<T> evt)
    {
        var context = _httpContextAccessor.HttpContext;
        string? bearerToken = context?.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");

        string? userId = null;
        string? deviceToken = null;

        if (!string.IsNullOrEmpty(bearerToken))
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(bearerToken);

                userId = token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                deviceToken = token.Claims.FirstOrDefault(c => c.Type == "device_token")?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao ler JWT.");
            }
        }

        evt = evt with
        {
            UserId = userId ?? evt.UserId,
            Data = evt.Data,
            OccurredAt = evt.OccurredAt
        };

        _logger.LogInformation(
            "📤 Publicando evento '{Type}' (UserId={UserId}) na fila '{QueueName}'",
            evt.Type, evt.UserId, queueName
        );

        await _publishEndpoint.Publish(evt, context =>
        {
            if (!string.IsNullOrEmpty(deviceToken))
                context.Headers.Set("X-DeviceToken", deviceToken);
        });
    }
}

[Channel("task_queue")]
[PublishOperation(typeof(BaseEvent<TaskCreatedData>), Summary = "Publica criação da tarefa")]
public class TaskCreatedPublisherDoc { }

[Channel("task_queue")]
[PublishOperation(typeof(BaseEvent<TaskDeletePayload>), Summary = "Publica remoção da tarefa")]
public class TaskDeletedPublisherDoc { }
