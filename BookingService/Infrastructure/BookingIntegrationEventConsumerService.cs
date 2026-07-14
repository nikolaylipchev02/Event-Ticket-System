using System.Text.Json;
using SharedContracts;
using Confluent.Kafka;
using MessagingContracts;
using Microsoft.EntityFrameworkCore;
using BookingService.Domain.Entities;

namespace BookingService.Infrastructure;

public class BookingIntegrationEventConsumerService : BackgroundService {
    readonly IServiceScopeFactory _scopeFactory;
    readonly IConfiguration _configuration;
    readonly ILogger<BookingIntegrationEventConsumerService> _logger;

    IConsumer<string, string>? _consumer;

    const string GROUP_ID = "booking-service";

    public BookingIntegrationEventConsumerService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<BookingIntegrationEventConsumerService> logger
    ) {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
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
                KafkaTopics.EventCreated
        ]);

        try {
            while (!stoppingToken.IsCancellationRequested) {
                ConsumeResult<string, string> result = _consumer.Consume(stoppingToken);

                try {
                    await HandleEventCreated(result, stoppingToken);
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

    async Task HandleEventCreated(ConsumeResult<string, string> result, CancellationToken stoppingToken) {
        EventCreatedIntegrationEvent integrationEvent =
                JsonSerializer.Deserialize<EventCreatedIntegrationEvent>(result.Message.Value, SharedJsonOptions.Web)
                ?? throw new InvalidOperationException($"Invalid {KafkaTopics.EventCreated} message");

        using IServiceScope scope = _scopeFactory.CreateScope();
        BookingServiceDbContext db = scope.ServiceProvider.GetRequiredService<BookingServiceDbContext>();

        if (await AlreadyProcessed(db, integrationEvent.MessageId, nameof(EventCreatedIntegrationEvent),
                    stoppingToken)) {
            return;
        }

        db.TicketsInventory.Add(new TicketInventory {
                Id = Guid.NewGuid(),
                EventId = integrationEvent.EventId,
                RemainingTickets = integrationEvent.TotalTickets
        });

        db.ProcessedIntegrationMessages.Add(new ProcessedIntegrationMessage {
                Id = Guid.NewGuid(),
                MessageId = integrationEvent.MessageId,
                MessageType = nameof(EventCreatedIntegrationEvent),
                ProcessedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(stoppingToken);
    }

    static async Task<bool> AlreadyProcessed(BookingServiceDbContext db, Guid messageId, string messageType,
            CancellationToken stoppingToken) {
        return await db.ProcessedIntegrationMessages
                .AnyAsync(m => m.MessageId == messageId && m.MessageType == messageType, stoppingToken);
    }
}