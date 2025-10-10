using MassTransit;
using PocMsGateway.DTOs;
namespace PocMsGateway.Messaging;

public record ResourcePublisher
{
    public string? Name { get; set; }
    public string? Description { get; set; }

    public ResourcePublisher(string name, string description)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }
}

public interface IMessagePublisher
{
    Task PublishEvent<T>(BaseEvent<T> evt, string queueName);
}

public class MessagePublisher : IMessagePublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MessagePublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishEvent<T>(BaseEvent<T> evt, string queueName)
    {
        await _publishEndpoint.Publish(evt);
    }
}
