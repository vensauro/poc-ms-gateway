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
            "🚀 Publicando evento na fila '{QueueName}': Type={Type}, CorrelationId={CorrelationId}", 
            queueName, evt.Type, evt.CorrelationId
        );
        await _publishEndpoint.Publish(evt);
        _logger.LogInformation("✅ Evento publicado com sucesso!");
    }
}

[Channel("task_queue")]
[PublishOperation(typeof(BaseEvent<TaskCreatedData>), Summary = "Publica criação da tarefa")]
public class TaskCreatedPublisherDoc { }

[Channel("task_queue")]
[PublishOperation(typeof(BaseEvent<TaskDeleteData>), Summary = "Publica remoção da tarefa")]
public class TaskDeletedPublisherDoc { }


[Channel("category_queue")]
[PublishOperation(typeof(BaseEvent<CategoryCreatedData>), Summary = "Publica criação da tarefa")]
public class CategoryCreatedPublisherDoc { }

[Channel("category_queue")]
[PublishOperation(typeof(BaseEvent<CategoryDeleteData>), Summary = "Publica remoção da tarefa")]
public class CategoryDeletedPublisherDoc { }
