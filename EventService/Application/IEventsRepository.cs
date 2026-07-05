using EventService.Application.DTOs;
using EventService.Domain.Entities;

namespace EventService.Application;

public interface IEventsRepository {

    public Task<List<Event>> GetEvents();
    public Task<Event?> GetSpecificEvent(Guid id);
    
    public Task CreateEvent(Event e);
    public Task UpdateEvent(Event e);
    public Task DeleteEvent(Guid id);
}