using MassTransit;
using Saunter.Attributes;
using PocMsGateway.DTOs;
using Microsoft.Extensions.Logging;

namespace PocMsGateway.Messaging;

public interface IMessagePublisher
{
    Task PublishEventAsync<T>(string queueName, BaseEvent<T> evt);
}

public class MessagePublisher : IMessagePublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MessagePublisher> _logger;

    public MessagePublisher(IPublishEndpoint publishEndpoint, ILogger<MessagePublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishEventAsync<T>(string queueName, BaseEvent<T> evt)
    {
        _logger.LogInformation(
            "🚀 Publicando evento na fila '{QueueName}': Type={Type}, CorrelationId={CorrelationId}, UserId={UserId}", 
            queueName, evt.Type, evt.CorrelationId, evt.UserId
        );
        await _publishEndpoint.Publish(evt);
        _logger.LogInformation("✅ Evento publicado com sucesso!");
    }
}

[Channel("task_queue")]
[PublishOperation(typeof(BaseEvent<TaskCreatedData>), Summary = "Publica criação de tarefa")]
public class TaskCreatedPublisherDoc { }

[Channel("task_queue")]
[PublishOperation(typeof(BaseEvent<TaskDeletePayload>), Summary = "Publica exclusão de tarefa")]
public class TaskDeletedPublisherDoc { }

[Channel("notification_queue")]
[PublishOperation(typeof(BaseEvent<NotificationData>), Summary = "Publica notificações")]
public class NotificationPublisherDoc { }