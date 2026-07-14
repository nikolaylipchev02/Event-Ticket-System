using Microsoft.Extensions.Caching.Distributed;

namespace EventService.Infrastructure.Caching;

public static class EventCacheOptions {
    public static readonly DistributedCacheEntryOptions List = new() {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    };

    public static readonly DistributedCacheEntryOptions SpecificEvent = new() {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20)
    };

    public static readonly DistributedCacheEntryOptions Version = new() {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
    };
}