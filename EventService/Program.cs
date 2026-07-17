using Confluent.Kafka;
using EventService.Application;
using EventService.Infrastructure;
using EventService.Infrastructure.Diagnostics;
using EventService.Infrastructure.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

const string EVENT_SERVICE_DB_CONNECTION_STRING = "EventServiceDbConnection";
const string REDIS_INSTANCE_NAME = "EventService:";

const int EXPORT_METRICS_INTERVAL_IN_MS = 1000;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

AddMessaging();
AddCaching();
AddPersistence();
AddDependencies();
AddHealthChecks();
AddMetrics();

builder.Services.AddOpenApi();
builder.Services.AddControllers();

WebApplication app = builder.Build();

app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions {
        Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions {
        Predicate = healthCheck => healthCheck.Tags.Contains("ready")
});

if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
return;

void AddMessaging() {
    builder.Services.AddSingleton<IProducer<string, string>>(sp => {
        IConfiguration configuration = sp.GetRequiredService<IConfiguration>();

        string bootstrapServers = configuration["Kafka:BootstrapServers"]
                                  ?? throw new InvalidOperationException("Kafka bootstrap servers not configured");

        ProducerConfig producerConfig = new() {
                BootstrapServers = bootstrapServers,
                Acks = Acks.All,
                EnableIdempotence = true,
                LingerMs = 5
        };

        return new ProducerBuilder<string, string>(producerConfig).Build();
    });

    builder.Services.AddHostedService<EventOutboxPublisherService>();
}

void AddCaching() {
    builder.Services.AddStackExchangeRedisCache(options => {
        options.Configuration = builder.Configuration.GetConnectionString("Redis")
                                ?? throw new InvalidOperationException("Redis connection string was not found");

        options.InstanceName = REDIS_INSTANCE_NAME;
    });
}

void AddPersistence() {
    string connectionString = builder.Configuration.GetConnectionString($"{EVENT_SERVICE_DB_CONNECTION_STRING}")
                              ?? throw new InvalidOperationException(
                                      $"Connection string '{EVENT_SERVICE_DB_CONNECTION_STRING}' was not found");

    builder.Services.AddDbContext<EventServiceDbContext>(options => { options.UseNpgsql(connectionString); });
}

void AddDependencies() {
    builder.Services.AddScoped<IEventRepository, EventRepository>();
}

void AddHealthChecks() {
    builder.Services.AddHealthChecks()
            .AddCheck<EventServiceDatabaseHealthCheck>(
                    "event-service-db"
                    , HealthStatus.Unhealthy,
                    ["ready"])
            .AddCheck<EventServiceMessagingHealthCheck>(
                    "event-service-messaging",
                    HealthStatus.Unhealthy,
                    ["ready"])
            .AddCheck<EventServiceCachingHealthCheck>(
                    "event-service-caching",
                    HealthStatus.Unhealthy,
                    ["ready"]);
}

void AddMetrics() {
    builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("EventService"))
            .WithMetrics(metrics => {
                metrics.AddMeter(EventServiceMetrics.METER_NAME);
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddOtlpExporter((exporterOptions, metricReaderOptions) => {
                    exporterOptions.Endpoint = new Uri(
                            builder.Configuration["Otlp:MetricsEndpoint"]
                            ?? "http://localhost:9090/api/v1/otlp/v1/metrics");
                    exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                    metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds =
                            EXPORT_METRICS_INTERVAL_IN_MS;
                });
            });
}