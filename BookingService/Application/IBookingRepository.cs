using BookingService.Domain.Entities;

namespace BookingService.Application;

public interface IBookingRepository {
    public Task<List<Booking>> GetBookings(Guid userId, CancellationToken cancellationToken);

    public Task<BookingIdempotencyRecord?> GetSpecificBookingRecord(Guid userId, string idempotencyKey,
            CancellationToken cancellationToken);

    public Task<Guid> Book(Booking booking, Guid userId, string idempotencyKey, CancellationToken cancellationToken);
    public Task CancelBooking(Guid userId, Guid bookingId, CancellationToken cancellationToken);
}