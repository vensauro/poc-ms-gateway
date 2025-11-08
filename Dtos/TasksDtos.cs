namespace PocMsGateway.DTOs
{
    public record TaskRequest
    {
        public string UserId { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string ExpiredAt { get; init; } = string.Empty;
    }

    public record BaseEvent<T>
    {
        public string Type { get; init; } = string.Empty;
        public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
        public string UserId { get; init; } = string.Empty;
        public T Data { get; init; } = default!;
        public string OccurredAt { get; init; } = DateTime.UtcNow.ToString("o");
    }

    public record TaskCreatedData
    {
        public int? Id { get; init; } = null;
        public string Description { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string ExpiredAt { get; init; } = string.Empty;
    }

    public record ListTaskRequest
    {
        public string UserId { get; set; } = null!;
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    public record ListTaskData
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    public record GetTaskRequest
    {
        public string UserId { get; set; } = null!;
        public int TaskId { get; set; }
        public string Category { get; set; } = null!;
    }

    public record DeleteTaskRequest
    {
        public string UserId { get; set; } = null!;
        public int TaskId { get; set; }
    }

    public record TaskGetPayload
    {
        public int TaskId { get; set; }
    }

    public record TaskDeletePayload
    {
        public int TaskId { get; set; }
    }
}
