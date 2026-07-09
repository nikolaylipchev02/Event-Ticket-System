using EventService.Domain.Entities;

namespace PreferenceService.Domain.Entities;

public class Preference {
    public required Guid UserId { get; set; }
    public EventCity? City { get; set; }
    public EventCategory? Category { get; set; }
}
