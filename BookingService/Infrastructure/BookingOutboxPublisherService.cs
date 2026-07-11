using BookingService.Domain.Entities;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure;

public class BookingOutboxPublisherService : BackgroundService {
    readonly IServiceScopeFactory _scopeFactory;
    readonly IProducer<string, string> _producer;
    readonly ILogger<BookingOutboxPublisherService> _logger;

    const int BATCH_SIZE = 20;
    const int POLLING_INTERVAL_SECONDS = 2;

    public BookingOutboxPublisherService(
            IServiceScopeFactory scopeFactory,
            IProducer<string, string> producer,
            ILogger<BookingOutboxPublisherService> logger
    ) {
        _scopeFactory = scopeFactory;
        _producer = producer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            try {
                using IServiceScope scope = _scopeFactory.CreateScope();
                BookingServiceDbContext db = scope.ServiceProvider.GetRequiredService<BookingServiceDbContext>();

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
                    } catch (Exception e) {
                        message.RetryCount++;
                        message.LastError = e.Message;

                        _logger.LogError(
                                e,
                                "Failed to publish outbox message {MessageId} to topic {Topic}",
                                message.Id,
                                message.Topic
                        );
                    }
                }

                await db.SaveChangesAsync(stoppingToken);
            } catch (Exception e) {
                _logger.LogError(e, "Outbox publisher loop failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(POLLING_INTERVAL_SECONDS), stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken) {
        _producer.Flush(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
