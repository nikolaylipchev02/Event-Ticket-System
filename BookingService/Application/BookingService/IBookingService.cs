namespace BookingService.Application;

public interface IBookingService {
    Task<Guid> Book(Guid userId, Guid eventId, string idempotencyKey, CancellationToken cancellationToken);
}
