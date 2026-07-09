using EventService.Domain.Entities;

namespace PreferenceService.Application.DTOs;

public class UpdatePreferenceDto {
    public EventCity? City { get; set; }
    public EventCategory? Category { get; set; }
}
