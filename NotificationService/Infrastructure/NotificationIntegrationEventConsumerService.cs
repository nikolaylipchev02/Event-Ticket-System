using System.Text.Json;
using SharedContracts;
using Confluent.Kafka;
using MessagingContracts;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure;

public class NotificationIntegrationEventConsumerService : BackgroundService {
    readonly IServiceScopeFactory _scopeFactory;
    readonly IConfiguration _configuration;
    readonly ILogger<NotificationIntegrationEventConsumerService> _logger;
    readonly IPreferenceApiClient _preferenceApiClient;

    IConsumer<string, string>? _consumer;

    const string GROUP_ID = "notification-service";

    public NotificationIntegrationEventConsumerService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<NotificationIntegrationEventConsumerService> logger,
            IPreferenceApiClient preferenceApiClient
    ) {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
        _preferenceApiClient = preferenceApiClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        string bootstrapServers = _configuration["Kafka:BootstrapServers"]
                                  ?? throw new InvalidOperationException("Kafka bootstrap servers not configured");

        ConsumerConfig consumerConfig = new() {
                BootstrapServers = bootstrapServers,
                GroupId = GROUP_ID,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        _consumer.Subscribe([
                KafkaTopics.BookingCreated,
                KafkaTopics.BookingCancelled,
                KafkaTopics.EventCreated,
                KafkaTopics.EventUpdated
        ]);

        try {
            while (!stoppingToken.IsCancellationRequested) {
                ConsumeResult<string, string> result = _consumer.Consume(stoppingToken);

                try {
                    await HandleMessage(result, stoppingToken);
                    _consumer.Commit(result);
                } catch (Exception e) {
                    _logger.LogError(
                            e,
                            "Failed to process message from topic {Topic} with key {Key}",
                            result.Topic,
                            result.Message.Key
                    );
                }
            }
        } catch (OperationCanceledException) {
            // Expected when the host is shutting down.
            // Gracefully exit instead of treating cancellation as an error.
        } finally {
            _consumer.Close();
            _consumer.Dispose();
        }
    }

    async Task HandleMessage(ConsumeResult<string, string> result, CancellationToken stoppingToken) {
        using IServiceScope scope = _scopeFactory.CreateScope();
        NotificationServiceDbContext db = scope.ServiceProvider.GetRequiredService<NotificationServiceDbContext>();

        switch (result.Topic) {
            case KafkaTopics.BookingCreated:
                await HandleBookingCreated(db, result.Message.Value, stoppingToken);
                break;
            case KafkaTopics.BookingCancelled:
                await HandleBookingCancelled(db, result.Message.Value, stoppingToken);
                break;
            case KafkaTopics.EventCreated:
                await HandleEventCreated(db, result.Message.Value, stoppingToken);
                break;
            case KafkaTopics.EventUpdated:
                await HandleEventUpdated(db, result.Message.Value, stoppingToken);
                break;
        }
    }

    async Task HandleBookingCreated(NotificationServiceDbContext db, string payload,
            CancellationToken stoppingToken) {
        BookingCreatedIntegrationEvent integrationEvent =
                JsonSerializer.Deserialize<BookingCreatedIntegrationEvent>(payload, SharedJsonOptions.Web)
                ?? throw new InvalidOperationException($"Invalid {KafkaTopics.BookingCreated} message");

        if (await AlreadyProcessed(db, integrationEvent.MessageId, nameof(BookingCreatedIntegrationEvent),
                    stoppingToken)) {
            return;
        }

        db.Notifications.Add(new Notification {
                Id = Guid.NewGuid(),
                UserId = integrationEvent.UserId,
                Message =
                        $"Your booking {integrationEvent.BookingId} for {integrationEvent.EventId} event was created successfully.",
                Type = NotificationType.BookingConfirmed
        });

        db.ProcessedIntegrationMessages.Add(new ProcessedIntegrationMessage {
                Id = Guid.NewGuid(),
                MessageId = integrationEvent.MessageId,
                MessageType = nameof(BookingCreatedIntegrationEvent),
                ProcessedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(stoppingToken);
    }

    async Task HandleBookingCancelled(NotificationServiceDbContext db, string payload,
            CancellationToken stoppingToken) {
        BookingCancelledIntegrationEvent integrationEvent =
                JsonSerializer.Deserialize<BookingCancelledIntegrationEvent>(payload, SharedJsonOptions.Web)
                ?? throw new InvalidOperationException($"Invalid {KafkaTopics.BookingCancelled} message");

        if (await AlreadyProcessed(db, integrationEvent.MessageId, nameof(BookingCancelledIntegrationEvent),
                    stoppingToken)) {
            return;
        }

        db.Notifications.Add(new Notification {
                Id = Guid.NewGuid(),
                UserId = integrationEvent.UserId,
                Message =
                        $"Your booking {integrationEvent.BookingId} for {integrationEvent.EventId} event was cancelled.",
                Type = NotificationType.BookingCancelled
        });

        db.ProcessedIntegrationMessages.Add(new ProcessedIntegrationMessage {
                Id = Guid.NewGuid(),
                MessageId = integrationEvent.MessageId,
                MessageType = nameof(BookingCancelledIntegrationEvent),
                ProcessedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(stoppingToken);
    }

    async Task HandleEventCreated(NotificationServiceDbContext db, string payload,
            CancellationToken stoppingToken) {
        EventCreatedIntegrationEvent integrationEvent =
                JsonSerializer.Deserialize<EventCreatedIntegrationEvent>(payload, SharedJsonOptions.Web)
                ?? throw new InvalidOperationException($"Invalid {KafkaTopics.EventCreated} message");

        if (await AlreadyProcessed(db, integrationEvent.MessageId, nameof(EventCreatedIntegrationEvent),
                    stoppingToken)) {
            return;
        }

        List<Guid> matchingUserIds =
                await _preferenceApiClient.GetMatchingUserIds(integrationEvent.City, integrationEvent.Category);

        foreach (Guid userId in matchingUserIds.Distinct()) {
            db.Notifications.Add(new Notification {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Message =
                            $"A new {integrationEvent.Category} event was created in {integrationEvent.City}: {integrationEvent.Title}",
                    Type = NotificationType.EventMatchedPreference,
                    CreatedAt = DateTime.UtcNow
            });
        }

        db.ProcessedIntegrationMessages.Add(new ProcessedIntegrationMessage {
                Id = Guid.NewGuid(),
                MessageId = integrationEvent.MessageId,
                MessageType = nameof(EventCreatedIntegrationEvent),
                ProcessedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(stoppingToken);
    }

    async Task HandleEventUpdated(NotificationServiceDbContext db, string payload,
            CancellationToken stoppingToken) {
        EventUpdatedIntegrationEvent integrationEvent =
                JsonSerializer.Deserialize<EventUpdatedIntegrationEvent>(payload, SharedJsonOptions.Web)
                ?? throw new InvalidOperationException($"Invalid {KafkaTopics.EventUpdated} message");

        if (await AlreadyProcessed(db, integrationEvent.MessageId, nameof(EventUpdatedIntegrationEvent),
                    stoppingToken)) {
            return;
        }

        db.ProcessedIntegrationMessages.Add(new ProcessedIntegrationMessage {
                Id = Guid.NewGuid(),
                MessageId = integrationEvent.MessageId,
                MessageType = nameof(EventUpdatedIntegrationEvent),
                ProcessedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(stoppingToken);
    }

    static async Task<bool> AlreadyProcessed(NotificationServiceDbContext db, Guid messageId, string messageType,
            CancellationToken stoppingToken) {
        return await db.ProcessedIntegrationMessages
                .AnyAsync(m => m.MessageId == messageId && m.MessageType == messageType, stoppingToken);
    }
}