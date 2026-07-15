using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EventService.Infrastructure.Diagnostics.HealthChecks;

public class EventServiceDatabaseHealthCheck : IHealthCheck {
    readonly EventServiceDbContext _dbContext;

    public EventServiceDatabaseHealthCheck(EventServiceDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new()) {
        try {
            bool canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

            return canConnect
                    ? HealthCheckResult.Healthy("Database is reachable")
                    : HealthCheckResult.Unhealthy("Database is not reachable");
        } catch (Exception e) {
            return HealthCheckResult.Unhealthy("Database health check failed", e);
        }
    }
}