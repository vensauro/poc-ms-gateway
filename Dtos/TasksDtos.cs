namespace PocMsGateway.DTOs
{
    public record TaskRequest
    {
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
    }

    public record NotificationRequest
    {
        public string Title { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
    }
}
