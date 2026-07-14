using System.Text.Json;
using SharedContracts;
using MessagingContracts;
using EventService.Application;
using EventService.Application.DTOs;
using EventService.Domain.Entities;
using EventService.Infrastructure.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace EventService.Infrastructure;

public class EventRepository : IEventRepository {
    readonly EventServiceDbContext _eventServiceDbContext;
    readonly IDistributedCache _cache;
    readonly ILogger<EventRepository> _logger;

    public EventRepository(EventServiceDbContext eventServiceDbContext, IDistributedCache cache,
            ILogger<EventRepository> logger) {
        _eventServiceDbContext = eventServiceDbContext;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<Event>> GetEvents() {
        string cacheKey = EventCacheKeys.AllEvents(await GetCacheVersion());

        try {
            string? cachedEvents = await _cache.GetStringAsync(cacheKey);

            if (cachedEvents is not null) {
                return JsonSerializer.Deserialize<List<Event>>(cachedEvents, JsonOptions) ?? [];
            }
        } catch (Exception e) when (IsRedisException(e)) {
            _logger.LogWarning(e, "Redis read failed for key {CacheKey}, falling back to database", cacheKey);
        }

        List<Event> events = await _eventServiceDbContext.Events
                .AsNoTracking()
                .OrderBy(e => e.Date)
                .ThenBy(e => e.Title)
                .ToListAsync();

        await TryCache(cacheKey, JsonSerializer.Serialize(events, JsonOptions), EventCacheOptions.List);
        return events;
    }

    public async Task<List<Event>> GetFilteredEvents(FilterEventDto filter) {
        string cacheKey = EventCacheKeys.Filtered(await GetCacheVersion(), filter);

        try {
            string? cachedEvents = await _cache.GetStringAsync(cacheKey);

            if (cachedEvents is not null) {
                return JsonSerializer.Deserialize<List<Event>>(cachedEvents, JsonOptions) ?? [];
            }
        } catch (Exception e) when (IsRedisException(e)) {
            _logger.LogWarning(e, "Redis read failed for key {CacheKey}, falling back to database", cacheKey);
        }

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

        List<Event> events = await query
                .OrderBy(e => e.Date)
                .ThenBy(e => e.Title)
                .ToListAsync();

        await TryCache(cacheKey, JsonSerializer.Serialize(events, JsonOptions), EventCacheOptions.List);

        return events;
    }

    public async Task<Event?> GetSpecificEvent(Guid id) {
        string cacheKey = EventCacheKeys.EventById(await GetCacheVersion(), id);

        try {
            string? cachedEvent = await _cache.GetStringAsync(cacheKey);

            if (cachedEvent is not null) {
                return JsonSerializer.Deserialize<Event>(cachedEvent, JsonOptions);
            }
        } catch (Exception e) when (IsRedisException(e)) {
            _logger.LogWarning(e, "Redis read failed for key {CacheKey}, falling back to database", cacheKey);
        }

        Event? existingEvent = await _eventServiceDbContext.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

        if (existingEvent is not null) {
            await TryCache(cacheKey, JsonSerializer.Serialize(existingEvent, JsonOptions),
                    EventCacheOptions.SpecificEvent);
        }

        return existingEvent;
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
        await InvalidateEventCache();
    }

    public async Task UpdateEvent(Event e) {
        _eventServiceDbContext.Events.Update(e);

        EventUpdatedIntegrationEvent integrationEvent = new(
                Guid.NewGuid(),
                e.Id,
                e.Title,
                e.City,
                e.Category,
                DateTime.UtcNow
        );

        _eventServiceDbContext.OutboxMessages.Add(new OutboxMessage {
                Id = integrationEvent.MessageId,
                Topic = KafkaTopics.EventUpdated,
                MessageType = nameof(EventUpdatedIntegrationEvent),
                Payload = JsonSerializer.Serialize(integrationEvent, JsonOptions),
                MessageKey = e.Id.ToString(),
                OccurredAt = integrationEvent.OccurredAt,
                RetryCount = 0
        });

        await _eventServiceDbContext.SaveChangesAsync();
        await InvalidateEventCache();
    }

    public async Task DeleteEvent(Guid id) {
        Event? e = await _eventServiceDbContext.Events.FirstOrDefaultAsync(e => e.Id == id);

        if (e is not null) {
            _eventServiceDbContext.Events.Remove(e);
            await _eventServiceDbContext.SaveChangesAsync();
            await InvalidateEventCache();
        }
    }

    async Task<string> GetCacheVersion() {
        try {
            string? version = await _cache.GetStringAsync(EventCacheKeys.CacheVersionKey);

            if (!string.IsNullOrWhiteSpace(version)) {
                return version;
            }

            string newVersion = Guid.NewGuid().ToString("N");
            await TryCache(EventCacheKeys.CacheVersionKey, newVersion, EventCacheOptions.Version);

            return newVersion;
        } catch (Exception e) when (IsRedisException(e)) {
            _logger.LogWarning(e, "Redis version lookup failed, use uncached request path instead");
            return Guid.NewGuid().ToString("N");
        }
    }

    async Task InvalidateEventCache() {
        try {
            string newVersion = Guid.NewGuid().ToString("N");
            await TryCache(EventCacheKeys.CacheVersionKey, newVersion, EventCacheOptions.Version);
        } catch (Exception e) when (IsRedisException(e)) {
            _logger.LogWarning(e, "Redis invalidation failed");
        }
    }

    async Task TryCache(string key, string value, DistributedCacheEntryOptions options) {
        try {
            await _cache.SetStringAsync(key, value, options);
        } catch (Exception e) when (IsRedisException(e)) {
            _logger.LogWarning(e, "Redis write failed for key {Cache Key}, continuing without cache", key);
        }
    }

    static readonly JsonSerializerOptions JsonOptions = new() {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    static bool IsRedisException(Exception e) {
        return e is RedisConnectionException
                or RedisTimeoutException
                or RedisServerException;
    }
}