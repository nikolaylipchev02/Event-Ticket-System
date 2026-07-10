using BookingService.Domain.Entities;

namespace BookingService.Application;

public class BookingService : IBookingService {
    readonly IBookingRepository _bookingRepository;
    readonly IEventApiClient _eventApiClient;

    public BookingService(IBookingRepository bookingRepository, IEventApiClient eventApiClient) {
        _bookingRepository = bookingRepository;
        _eventApiClient = eventApiClient;
    }

    public async Task<Guid> Book(Guid userId, Guid eventId, string idempotencyKey) {
        BookingIdempotencyRecord? existingBookingRecord =
                await _bookingRepository.GetSpecificBookingRecord(userId, idempotencyKey);

        if (existingBookingRecord is not null) {
            return existingBookingRecord.EventId != eventId
                    ? throw new InvalidOperationException("Idempotency key was already used for a different event")
                    : existingBookingRecord.BookingId;
        }

        bool eventExists = await _eventApiClient.EventExists(eventId);

        if (!eventExists) {
            throw new KeyNotFoundException("Event not found");
        }

        Booking booking = new() {
                Id = Guid.NewGuid(),
                UserId = userId,
                EventId = eventId,
                Status = BookingStatus.Booked,
                BookedAt = DateTime.UtcNow
        };

        return await _bookingRepository.Book(booking, userId, idempotencyKey);
    }
}
