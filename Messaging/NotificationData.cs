public record NotificationData
{
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string CreatedAt { get; init; } = string.Empty;
}
