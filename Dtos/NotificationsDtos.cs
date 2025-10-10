namespace PocMsGateway.DTOs
{
    public record NotificationRequest
    {
        public string UserId { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string CreatedAt { get; init; } = string.Empty;
    }
}
