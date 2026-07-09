using BookingService.Domain.Entities;

namespace BookingService.Application;

public class BookingService : IBookingService {
    readonly IBookingRepository _bookingRepository;
    readonly IEventApiClient _eventApiClient;

    public BookingService(IBookingRepository bookingRepository, IEventApiClient eventApiClient) {
        _bookingRepository = bookingRepository;
        _eventApiClient = eventApiClient;
    }

    public async Task Book(Guid userId, Guid eventId) {
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

        await _bookingRepository.Book(booking);
    }
}
