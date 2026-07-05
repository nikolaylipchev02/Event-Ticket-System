using EventService.Application;
using EventService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure;

public class EventsRepository : IEventsRepository {
    
    readonly EventServiceDbContext _eventServiceDbContext;

    public EventsRepository(EventServiceDbContext eventServiceDbContext) {
        _eventServiceDbContext = eventServiceDbContext;
    }

    public async Task<List<Event>> GetEvents() {
        return await _eventServiceDbContext.Events
                .AsNoTracking()
                .ToListAsync();
    }

    public async Task<Event?> GetSpecificEvent(Guid id) {
        return await _eventServiceDbContext.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task CreateEvent(Event e) {
        _eventServiceDbContext.Events.Add(e);
        await _eventServiceDbContext.SaveChangesAsync();
    }

    public async Task UpdateEvent(Event e) {
        _eventServiceDbContext.Events.Update(e);
        await _eventServiceDbContext.SaveChangesAsync();
    }

    public async Task DeleteEvent(Guid id) {
        Event? e = await _eventServiceDbContext.Events.FirstOrDefaultAsync(e => e.Id == id);

        if (e is not null) {
            _eventServiceDbContext.Events.Remove(e);
            await _eventServiceDbContext.SaveChangesAsync();
        }
    }
}