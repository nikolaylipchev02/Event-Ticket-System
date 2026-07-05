using EventService.Application;
using EventService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure;

public class EventsRepository : IEventsRepository {
    
    private readonly EventServiceDbContext eventServiceDbContext;

    public EventsRepository(EventServiceDbContext eventServiceDbContext) {
        this.eventServiceDbContext = eventServiceDbContext;
    }

    public async Task<List<Event>> GetEvents() {
        return await eventServiceDbContext.Events
                .AsNoTracking()
                .ToListAsync();
    }

    public async Task<Event?> GetSpecificEvent(Guid id) {
        return await eventServiceDbContext.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task CreateEvent(Event e) {
        eventServiceDbContext.Events.Add(e);
        await eventServiceDbContext.SaveChangesAsync();
    }

    public async Task UpdateEventDetails(Guid id, string title, string description) {
        Event? e = await eventServiceDbContext.Events.FirstOrDefaultAsync(e => e.Id == id);
        
        if (e is not null) {
            if (!string.IsNullOrEmpty(title)) {
                e.Title = title;
            }
            
            if (!string.IsNullOrEmpty(description)) {
                e.Description = description;
            }
            
            await eventServiceDbContext.SaveChangesAsync();
        }
    }

    public async Task DeleteEvent(Guid id) {
        Event? e = await eventServiceDbContext.Events.FirstOrDefaultAsync(e => e.Id == id);

        if (e is not null) {
            eventServiceDbContext.Events.Remove(e);
            await eventServiceDbContext.SaveChangesAsync();
        }
    }
}