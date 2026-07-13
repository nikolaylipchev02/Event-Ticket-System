using Frontend.Contracts;

namespace Frontend.Services;

public interface IEventApiClient {
    Task<IReadOnlyList<EventItem>> GetEventsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventItem>> GetFilteredEventsAsync(EventFilterRequest request,
            CancellationToken cancellationToken = default);
    Task<EventItem?> GetEventAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateEventAsync(CreateEventRequest request, CancellationToken cancellationToken = default);
    Task UpdateEventAsync(Guid id, UpdateEventRequest request, CancellationToken cancellationToken = default);
    Task DeleteEventAsync(Guid id, CancellationToken cancellationToken = default);
}
