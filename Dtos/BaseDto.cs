namespace PocMsGateway.DTOs
{
    public record BaseEvent<T>
    {
        public string Type { get; init; } = string.Empty;
        public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
        public T Data { get; init; } = default!;
        public string OccurredAt { get; init; } = DateTime.UtcNow.ToString("o");
    }
}
