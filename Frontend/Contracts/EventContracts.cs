namespace Frontend.Contracts;

public enum EventCity {
    Other,
    Sofia,
    Plovdiv,
    Varna,
    Burgas,
    Ruse,
    Pleven,
    Sliven,
    Dobrich,
    Shumen,
    Pernik,
    Haskovo,
    Blagoevgrad,
    Yambol,
    VelikoTarnovo,
    Pazardzhik,
    Vratsa,
    Gabrovo,
    Kazanlak
}

public enum EventCategory {
    Other,
    Music,
    Sports,
    Arts,
    Business,
    Education,
    Social
}

public sealed class EventItem {
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public EventCity City { get; set; }
    public EventCategory Category { get; set; }
    public decimal Price { get; set; }
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalTickets { get; set; }
}

public sealed class CreateEventRequest {
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required EventCity City { get; set; }
    public required EventCategory Category { get; set; }
    public required decimal Price { get; set; }
    public required DateTime Date { get; set; }
    public required int TotalTickets { get; set; }
}

public sealed class UpdateEventRequest {
    public string? Title { get; set; }
    public string? Description { get; set; }
}

public sealed class EventFilterRequest {
    public EventCity? City { get; set; }
    public EventCategory? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
