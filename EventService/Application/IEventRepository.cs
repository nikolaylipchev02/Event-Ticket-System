using EventService.Application.DTOs;
using EventService.Domain.Entities;

namespace EventService.Application;

public interface IEventRepository {
    public Task<List<Event>> GetEvents(CancellationToken cancellationToken);
    public Task<List<Event>> GetFilteredEvents(FilterEventDto filter, CancellationToken cancellationToken);
    public Task<Event?> GetSpecificEvent(Guid id, CancellationToken cancellationToken);

    public Task CreateEvent(Event e, CancellationToken cancellationToken);
    public Task UpdateEvent(Event e, CancellationToken cancellationToken);
    public Task DeleteEvent(Guid id, CancellationToken cancellationToken);
}