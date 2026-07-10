namespace BookingService.Domain.Entities;

public class BookingIdempotencyRecord {
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required string IdempotencyKey { get; set; }
    public required Guid EventId { get; set; }
    public required Guid BookingId { get; set; }
    public required DateTime CreatedAt { get; set; }
}
