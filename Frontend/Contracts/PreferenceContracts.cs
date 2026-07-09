namespace Frontend.Contracts;

public sealed class PreferenceItem {
    public Guid UserId { get; set; }
    public EventCity? City { get; set; }
    public EventCategory? Category { get; set; }
}

public sealed class UpdatePreferenceRequest {
    public EventCity? City { get; set; }
    public EventCategory? Category { get; set; }
    public bool RemoveCity { get; set; }
    public bool RemoveCategory { get; set; }
}
