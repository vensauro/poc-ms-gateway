using MassTransit;
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
        // Log antes de publicar
        _logger.LogInformation(
            "🚀 Publicando evento na fila '{QueueName}': Type={Type}, CorrelationId={CorrelationId}, UserId={UserId}", 
            queueName, evt.Type, evt.CorrelationId, evt.UserId
        );

        await _publishEndpoint.Publish(evt);

        // Log depois de publicar
        _logger.LogInformation("✅ Evento publicado com sucesso!");
    }
}
