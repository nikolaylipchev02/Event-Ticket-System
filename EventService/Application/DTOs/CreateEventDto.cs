using EventService.Domain.Entities;

namespace EventService.Application.DTOs;

public class CreateEventDto {
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required EventCity City { get; set; }
    public required EventCategory Category { get; set; }
    public required decimal Price { get; set; }
    public required DateTime Date { get; set; }
}