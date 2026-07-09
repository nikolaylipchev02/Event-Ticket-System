using BookingService.Application;
using BookingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure;

public class BookingRepository : IBookingRepository {
    readonly BookingServiceDbContext _bookingServiceDbContext;

    public BookingRepository(BookingServiceDbContext bookingServiceDbContext) {
        _bookingServiceDbContext = bookingServiceDbContext;
    }

    public async Task<List<Booking>> GetBookings(Guid userId) {
        return await _bookingServiceDbContext.Bookings
                .AsNoTracking()
                .Where(b => b.UserId == userId)
                .ToListAsync();
    }

    public async Task Book(Booking booking) {
        _bookingServiceDbContext.Bookings.Add(booking);

        await _bookingServiceDbContext.SaveChangesAsync();
    }

    public async Task CancelBooking(Guid userId, Guid bookingId) {
        Booking? booking =
                await _bookingServiceDbContext.Bookings.FirstOrDefaultAsync(b =>
                        b.Id == bookingId && b.UserId == userId);

        if (booking is null) {
            throw new KeyNotFoundException("Booking not found");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = DateTime.UtcNow;

        await _bookingServiceDbContext.SaveChangesAsync();
    }
}
