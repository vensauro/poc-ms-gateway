using MassTransit;
namespace PocMsGateway.Messaging;

public record ResourceCreated
{
    public string? Name { get; set; }
    public string? Description { get; set; }

    public ResourceCreated(string name, string description)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }
}

public interface IMessagePublisher
{
    Task PublishMessage(ResourceCreated resource);
}

public class MessagePublisher : IMessagePublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MessagePublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishMessage(ResourceCreated resource)
    {
        await _publishEndpoint.Publish(resource);
    }
}
