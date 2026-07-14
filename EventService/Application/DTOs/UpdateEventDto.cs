using SharedContracts;

namespace EventService.Application.DTOs;

public class UpdateEventDto {
    public string? Title { get; set; }
    public string? Description { get; set; }
    public EventCity? City { get; set; }
    public EventCategory? Category { get; set; }
    public decimal? Price { get; set; }
    public DateTime? Date { get; set; }
}