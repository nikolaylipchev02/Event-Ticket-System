namespace BookingService.Application;

public interface IBookingService {
    Task Book(Guid userId, Guid eventId);
}
