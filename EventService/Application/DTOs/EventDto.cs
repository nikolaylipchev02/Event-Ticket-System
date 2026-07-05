using EventService.Domain.Entities;

namespace EventService.Application.DTOs;

public class EventDto {
    
    public required string Title { get; set; }
    public required string Description { get; set; }

    public Event ToEntity() {
        return new Event() {
                Id = Guid.NewGuid(),
                Title = Title,
                Description = Description,
                CreatedAt = DateTime.UtcNow
        };
    }
}