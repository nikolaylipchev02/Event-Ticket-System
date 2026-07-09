namespace Frontend.Contracts;

public enum BookingStatus {
    Booked,
    Cancelled
}

public sealed class BookingItem {
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid EventId { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime BookedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}

public sealed class CreateBookingRequest {
    public required Guid EventId { get; set; }
}