using MassTransit;
using Saunter.Attributes;
using Microsoft.Extensions.Logging;
using PocMsGateway.DTOs;

namespace PocMsGateway.Messaging;

public class ResourceConsumer<T> : IConsumer<BaseEvent<T>>
{
    private readonly ILogger<ResourceConsumer<T>> _logger;

    public ResourceConsumer(ILogger<ResourceConsumer<T>> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<BaseEvent<T>> context)
    {
        var message = context.Message;
        _logger.LogInformation("📥 Evento recebido: {Type} | CorrelationId: {CorrelationId}", message.Type, message.CorrelationId);
        _logger.LogInformation("Payload: {@Data}", message.Data);
        return Task.CompletedTask;
    }
}

[Channel("task_queue")]
[SubscribeOperation(typeof(BaseEvent<TaskCreatedData>), Summary = "Consome criação de tarefa")]
public class TaskCreatedConsumerDoc { }

[Channel("notification_queue")]
[SubscribeOperation(typeof(BaseEvent<NotificationData>), Summary = "Consome notificações")]
public class NotificationConsumerDoc { }