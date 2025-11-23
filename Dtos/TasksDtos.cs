namespace PocMsGateway.DTOs
{
    public record TaskRequest
    {
        public string Description { get; init; } = string.Empty;
        public int CategoryId { get; set; }
        public string ExpiredAt { get; init; } = string.Empty;
    }

    public record TaskCreatedData
    {
        public int? Id { get; init; } = null;
        public string Description { get; init; } = string.Empty;
        public int CategoryId { get; set; }
        public string ExpiredAt { get; init; } = string.Empty;
        public string DeviceToken { get; init; } = string.Empty;
        public string UserId { get; init; } = string.Empty;
    }

    public record DeleteTaskRequest
    {
        public int TaskId { get; set; }
    }

    public record TaskDeleteData
    {
        public int TaskId { get; set; }
        public string UserId { get; init; } = string.Empty;
    }
}
