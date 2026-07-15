using Confluent.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EventService.Infrastructure.Diagnostics.HealthChecks;

public class EventServiceMessagingHealthCheck : IHealthCheck {
    readonly IConfiguration _configuration;

    const int METADATA_DELAY = 5;

    public EventServiceMessagingHealthCheck(IConfiguration configuration) {
        _configuration = configuration;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new()) {
        string? bootstrapServers = _configuration["Kafka:BootstrapServers"];

        if (bootstrapServers is null) {
            return Task.FromResult(HealthCheckResult.Unhealthy("Kafka bootstrap servers are not configured."));
        }

        try {
            using IAdminClient adminClient = new AdminClientBuilder(new AdminClientConfig {
                    BootstrapServers = bootstrapServers
            }).Build();

            Metadata metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(METADATA_DELAY));

            if (metadata.Brokers.Count > 0) {
                return Task.FromResult(HealthCheckResult.Healthy("Kafka is reachable"));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("Kafka returned no broker metadata"));
        } catch (Exception e) {
            return Task.FromResult(HealthCheckResult.Unhealthy("Kafka health check failed", e));
        }
    }
}