using MassTransit;
using Microsoft.Extensions.Logging;
using PocMsGateway.DTOs;

public class EventPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(IBus bus, ILogger<EventPublisher> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task PublishAsync<T>(BaseEvent<T> message)
    {
        var queueName = message.Type.Replace('.', '_');
        _logger.LogInformation("📤 Publicando evento {Type} na fila {Queue}", message.Type, queueName);

        await _bus.Publish(message);
    }
}
