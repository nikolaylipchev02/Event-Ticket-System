using EventService.Domain.Entities;

namespace EventService.Application.DTOs;

public class EventDto {
    
    public string? Title { get; set; }
    public string? Description { get; set; }

    public Event ToEntity() {
        return new Event() {
                Id = Guid.NewGuid(),
                Title = Title ?? string.Empty,
                Description = Description ?? string.Empty,
                CreatedAt = DateTime.UtcNow
        };
    }
}