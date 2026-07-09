namespace BookingService.Application.DTOs;

public class CreateBookingDto {
    public required Guid UserId { get; set; }
    public required Guid EventId { get; set; }
}