namespace BookingService.Application;

public interface IEventApiClient {
    public Task<bool> EventExists(Guid id, CancellationToken cancellationToken);
}