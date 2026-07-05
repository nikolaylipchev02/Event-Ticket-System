using EventService.Domain.Entities;

namespace EventService.Application;

public interface IEventsRepository {

    public Task<List<Event>> GetEvents();
    public Task<Event?> GetSpecificEvent(Guid id);
    
    public Task CreateEvent(Event e);
    public Task UpdateEventDetails(Guid id, string title, string description);
    public Task DeleteEvent(Guid id);
}