using MassTransit;
using Saunter.Attributes;
using DontForgetGateway.DTOs;
using System.Net.Mime;

namespace DontForgetGateway.Messaging;

public interface IMessagePublisher
{
    Task PublishEventAsync<T>(string queueName, BaseEvent<T> evt);
}

public class MessagePublisher : IMessagePublisher
{
    private readonly IBus _bus;
    private readonly ILogger<MessagePublisher> _logger;

    public MessagePublisher(IBus bus, ILogger<MessagePublisher> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task PublishEventAsync<T>(string queueName, BaseEvent<T> evt)
    {
        _logger.LogInformation("🚀 Publicando evento direto na fila {QueueName}", queueName);

        var sendEndpoint = await _bus.GetSendEndpoint(new Uri($"queue:{queueName}"));

        await sendEndpoint.Send(evt, ctx =>
        {
            ctx.ContentType = new ContentType("application/json");
        });

        _logger.LogInformation("✅ Evento publicado na fila {QueueName}", queueName);
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
