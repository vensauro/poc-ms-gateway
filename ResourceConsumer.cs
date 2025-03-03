using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using PocMsGateway.Messages;

public class ResourceConsumer : IConsumer<ResourceCreated>
{
    private readonly ILogger<ResourceConsumer> _logger;

    public ResourceConsumer(ILogger<ResourceConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ResourceCreated> context)
    {
        _logger.LogInformation("Resource received: {Name} - {Description}", 
            context.Message.Name, context.Message.Description);
        return Task.CompletedTask;
    }
}
