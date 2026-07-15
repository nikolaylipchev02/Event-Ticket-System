using EventService.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EventService.Infrastructure.Diagnostics.HealthChecks;

public sealed class EventServiceCachingHealthCheck : IHealthCheck {
    readonly IDistributedCache _cache;

    public EventServiceCachingHealthCheck(IDistributedCache cache) {
        _cache = cache;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default) {
        try {
            await _cache.GetStringAsync(EventCacheKeys.CacheVersionKey, cancellationToken);
            return HealthCheckResult.Healthy("Redis is reachable");
        } catch (Exception ex) {
            return HealthCheckResult.Unhealthy("Redis health check failed", ex);
        }
    }
}