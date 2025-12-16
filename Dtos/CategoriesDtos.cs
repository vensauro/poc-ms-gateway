namespace PocMsGateway.DTOs
{
    public record CategoryRequest
    {
        public string Name { get; init; } = string.Empty;
    }

    public record CategoryCreatedData
    {
        public int? Id { get; init; } = null;
        public string Name { get; init; } = string.Empty;
        public string UserId { get; init; } = string.Empty;
    }

    public record UpdateCategoryRequest
    {
        public string? Name { get; init; }
    }

    public record CategoryUpdateData
    {
        public int CategoryId { get; init; }
        public string? Name { get; init; }
        public string UserId { get; init; } = string.Empty;
    }

    public record DeleteCategoryRequest
    {
        public int CategoryId { get; set; }
    }

    public record CategoryDeleteData
    {
        public int CategoryId { get; set; }
        public string UserId { get; init; } = string.Empty;
    }
}
