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

    public async Task CancelBooking(Guid id) {
        Booking? booking = await _bookingServiceDbContext.Bookings.FirstOrDefaultAsync(b => b.Id == id);

        if (booking is not null) {
            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;

            await _bookingServiceDbContext.SaveChangesAsync();
        }
    }
}