using BookingService.Application;
using BookingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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
        await using IDbContextTransaction transaction = await _bookingServiceDbContext.Database.BeginTransactionAsync();

        TicketInventory? ticketInventory = await _bookingServiceDbContext.TicketsInventory
                .FromSql($"""SELECT * FROM "tickets_inventory" WHERE "EventId" = {booking.EventId} FOR UPDATE""")
                .SingleOrDefaultAsync();

        if (ticketInventory is null) {
            throw new KeyNotFoundException("Event inventory not found");
        }

        if (ticketInventory.RemainingTickets <= 0) {
            throw new InvalidOperationException("No tickets available");
        }

        ticketInventory.RemainingTickets--;

        _bookingServiceDbContext.Bookings.Add(booking);

        await _bookingServiceDbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task CancelBooking(Guid userId, Guid bookingId) {
        await using IDbContextTransaction transaction = await _bookingServiceDbContext.Database.BeginTransactionAsync();

        Booking? booking = await _bookingServiceDbContext.Bookings
                .FromSql($"""SELECT * FROM "bookings" WHERE "Id" = {bookingId} AND "UserId" = {userId} FOR UPDATE """)
                .SingleOrDefaultAsync();

        if (booking is null) {
            throw new KeyNotFoundException("Booking not found");
        }

        if (booking.Status == BookingStatus.Cancelled) {
            throw new InvalidOperationException("Booking is already cancelled");
        }

        TicketInventory? ticketInventory = await _bookingServiceDbContext.TicketsInventory
                .FromSql($"""SELECT * FROM "tickets_inventory" WHERE "EventId" = {booking.EventId} FOR UPDATE""")
                .SingleOrDefaultAsync();

        if (ticketInventory is null) {
            throw new KeyNotFoundException("Event inventory not found");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = DateTime.UtcNow;

        ticketInventory.RemainingTickets++;

        await _bookingServiceDbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
