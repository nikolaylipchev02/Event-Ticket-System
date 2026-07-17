using System.Diagnostics;
using EventService.Domain.Entities;
using Confluent.Kafka;
using EventService.Infrastructure.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure;

public class EventOutboxPublisherService : BackgroundService {
    readonly IServiceScopeFactory _scopeFactory;
    readonly IProducer<string, string> _producer;
    readonly ILogger<EventOutboxPublisherService> _logger;

    const int BATCH_SIZE = 20;
    const int POLLING_INTERVAL_SECONDS = 2;

    public EventOutboxPublisherService(
            IServiceScopeFactory scopeFactory,
            IProducer<string, string> producer,
            ILogger<EventOutboxPublisherService> logger
    ) {
        _scopeFactory = scopeFactory;
        _producer = producer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _logger.LogInformation("Event outbox publisher started");

        while (!stoppingToken.IsCancellationRequested) {
            try {
                using IServiceScope scope = _scopeFactory.CreateScope();
                EventServiceDbContext db = scope.ServiceProvider.GetRequiredService<EventServiceDbContext>();

                List<OutboxMessage> messages = await db.OutboxMessages
                        .Where(message => message.PublishedAt == null)
                        .OrderBy(message => message.OccurredAt)
                        .Take(BATCH_SIZE)
                        .ToListAsync(stoppingToken);

                if (messages.Count == 0) {
                    await Task.Delay(TimeSpan.FromSeconds(POLLING_INTERVAL_SECONDS), stoppingToken);
                    continue;
                }

                foreach (OutboxMessage message in messages) {
                    Stopwatch stopwatch = Stopwatch.StartNew();

                    try {
                        await _producer.ProduceAsync(
                                message.Topic,
                                new Message<string, string> {
                                        Key = message.MessageKey,
                                        Value = message.Payload
                                },
                                stoppingToken
                        );

                        message.PublishedAt = DateTime.UtcNow;
                        message.LastError = null;

                        EventServiceMetrics.RecordOutboxPublish("success", stopwatch.Elapsed);
                    } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
                        return;
                    } catch (Exception e) {
                        message.RetryCount++;
                        message.LastError = e.Message;

                        _logger.LogError(
                                e,
                                "Failed to publish outbox message {MessageId} to topic {Topic} - retry count: {RetryCount}",
                                message.Id,
                                message.Topic,
                                message.RetryCount
                        );
                        EventServiceMetrics.RecordOutboxPublish("failed", stopwatch.Elapsed);
                    } finally {
                        stopwatch.Stop();
                    }
                }

                await db.SaveChangesAsync(stoppingToken);
            } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
                return;
            } catch (Exception e) {
                _logger.LogError(e, "Outbox publisher loop failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(POLLING_INTERVAL_SECONDS), stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("Stopping event outbox publisher");
        _producer.Flush(cancellationToken);

        await base.StopAsync(cancellationToken);
    }
}