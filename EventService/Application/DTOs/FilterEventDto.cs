using EventService.Domain.Entities;

namespace EventService.Application.DTOs;

public class FilterEventDto {
    public EventCity? City { get; set; }
    public EventCategory? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
