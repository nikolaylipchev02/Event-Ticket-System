namespace BookingService.Application;

public interface IEventApiClient {
    Task<bool> EventExists(Guid id);
}
