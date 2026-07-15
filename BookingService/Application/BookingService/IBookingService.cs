namespace BookingService.Application;

public interface IBookingService {
    public Task<Guid> Book(Guid userId, Guid eventId, string idempotencyKey, CancellationToken cancellationToken);
}