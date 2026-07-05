using BookingService.Domain.Entities;

namespace BookingService.Application;

public interface IBookingRepository {

    public Task<List<Booking>> GetBookings(Guid userId);
            
    public Task Book(Booking booking);
    public Task CancelBooking(Guid id);
}