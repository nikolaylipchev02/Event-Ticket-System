namespace Frontend.Contracts;

public sealed class EventItem {
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class CreateEventRequest {
    public required string Title { get; set; }
    public required string Description { get; set; }
}

public sealed class UpdateEventRequest {
    public string? Title { get; set; }
    public string? Description { get; set; }
}