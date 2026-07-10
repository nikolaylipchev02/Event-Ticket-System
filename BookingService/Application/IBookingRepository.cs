using BookingService.Domain.Entities;

namespace BookingService.Application;

public interface IBookingRepository {
    public Task<List<Booking>> GetBookings(Guid userId);
    public Task<BookingIdempotencyRecord?> GetSpecificBookingRecord(Guid userId, string idempotencyKey);
    public Task<Guid> Book(Booking booking, Guid userId, string idempotencyKey);
    public Task CancelBooking(Guid userId, Guid bookingId);
}
