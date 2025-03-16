using MassTransit;
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
    Task PublishMessage(ResourcePublisher resource);
}

public class MessagePublisher : IMessagePublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MessagePublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishMessage(ResourcePublisher resource)
    {
        await _publishEndpoint.Publish(resource);
    }
}
