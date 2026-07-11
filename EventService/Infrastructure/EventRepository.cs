using System.Text.Json;
using SharedContracts;
using MessagingContracts;
using EventService.Application;
using EventService.Application.DTOs;
using EventService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure;

public class EventRepository : IEventRepository {
    readonly EventServiceDbContext _eventServiceDbContext;

    public EventRepository(EventServiceDbContext eventServiceDbContext) {
        _eventServiceDbContext = eventServiceDbContext;
    }

    public async Task<List<Event>> GetEvents() {
        return await _eventServiceDbContext.Events
                .AsNoTracking()
                .ToListAsync();
    }

    public async Task<List<Event>> GetFilteredEvents(FilterEventDto filter) {
        IQueryable<Event> query = _eventServiceDbContext.Events.AsNoTracking();

        if (filter.City is not null) {
            query = query.Where(e => e.City == filter.City);
        }

        if (filter.Category is not null) {
            query = query.Where(e => e.Category == filter.Category);
        }

        if (filter.MinPrice is not null) {
            query = query.Where(e => e.Price >= filter.MinPrice);
        }

        if (filter.MaxPrice is not null) {
            query = query.Where(e => e.Price <= filter.MaxPrice);
        }

        if (filter.FromDate is not null) {
            query = query.Where(e => e.Date >= filter.FromDate);
        }

        if (filter.ToDate is not null) {
            query = query.Where(e => e.Date <= filter.ToDate);
        }

        return await query.ToListAsync();
    }

    public async Task<Event?> GetSpecificEvent(Guid id) {
        return await _eventServiceDbContext.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task CreateEvent(Event e) {
        _eventServiceDbContext.Events.Add(e);

        EventCreatedIntegrationEvent integrationEvent = new(
                Guid.NewGuid(),
                e.Id,
                e.Title,
                e.City,
                e.Category,
                e.TotalTickets,
                DateTime.UtcNow
        );

        _eventServiceDbContext.OutboxMessages.Add(new OutboxMessage {
                Id = integrationEvent.MessageId,
                Topic = KafkaTopics.EventCreated,
                MessageType = nameof(EventCreatedIntegrationEvent),
                Payload = JsonSerializer.Serialize(integrationEvent, JsonOptions),
                MessageKey = e.Id.ToString(),
                OccurredAt = integrationEvent.OccurredAt,
                RetryCount = 0
        });

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

    static readonly JsonSerializerOptions JsonOptions = new() {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
