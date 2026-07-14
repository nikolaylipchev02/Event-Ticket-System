using MessagingContracts;
using System.Text.Json;
using BookingService.Application;
using BookingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using SharedContracts;

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

    public async Task<BookingIdempotencyRecord?> GetSpecificBookingRecord(Guid userId, string idempotencyKey) {
        return await _bookingServiceDbContext.BookingIdempotencyRecords
                .AsNoTracking()
                .Where(record => record.UserId == userId && record.IdempotencyKey == idempotencyKey)
                .SingleOrDefaultAsync();
    }

    public async Task<Guid> Book(Booking booking, Guid userId, string idempotencyKey) {
        await using IDbContextTransaction transaction = await _bookingServiceDbContext.Database.BeginTransactionAsync();

        try {
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

            _bookingServiceDbContext.BookingIdempotencyRecords.Add(new BookingIdempotencyRecord {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    IdempotencyKey = idempotencyKey,
                    EventId = booking.EventId,
                    BookingId = booking.Id,
                    CreatedAt = DateTime.UtcNow
            });

            BookingCreatedIntegrationEvent integrationEvent = new(
                    Guid.NewGuid(),
                    booking.Id,
                    userId,
                    booking.EventId,
                    DateTime.UtcNow
            );

            _bookingServiceDbContext.OutboxMessages.Add(new OutboxMessage {
                    Id = integrationEvent.MessageId,
                    Topic = KafkaTopics.BookingCreated,
                    MessageType = nameof(BookingCreatedIntegrationEvent),
                    Payload = JsonSerializer.Serialize(integrationEvent, SharedJsonOptions.Web),
                    MessageKey = booking.Id.ToString(),
                    OccurredAt = integrationEvent.OccurredAt,
                    RetryCount = 0
            });

            await _bookingServiceDbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return booking.Id;
        } catch (DbUpdateException e) when (IsUniqueViolation(e)) {
            await transaction.RollbackAsync();

            BookingIdempotencyRecord? existingBookingRecord = await GetSpecificBookingRecord(userId, idempotencyKey);

            if (existingBookingRecord is null) {
                throw;
            }

            return existingBookingRecord.EventId != booking.EventId
                    ? throw new InvalidOperationException("Idempotency key was already used for a different event")
                    : existingBookingRecord.BookingId;
        }
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

        BookingCancelledIntegrationEvent integrationEvent = new(
                Guid.NewGuid(),
                booking.Id,
                userId,
                booking.EventId,
                DateTime.UtcNow
        );

        _bookingServiceDbContext.OutboxMessages.Add(new OutboxMessage {
                Id = integrationEvent.MessageId,
                Topic = KafkaTopics.BookingCancelled,
                MessageType = nameof(BookingCancelledIntegrationEvent),
                Payload = JsonSerializer.Serialize(integrationEvent, SharedJsonOptions.Web),
                MessageKey = booking.Id.ToString(),
                OccurredAt = integrationEvent.OccurredAt,
                RetryCount = 0
        });

        await _bookingServiceDbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    static bool IsUniqueViolation(DbUpdateException e) {
        return e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
    }
}