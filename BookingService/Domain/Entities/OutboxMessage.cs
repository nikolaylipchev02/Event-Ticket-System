using SharedContracts;

namespace BookingService.Domain.Entities;

public class OutboxMessage {
    public Guid Id { get; set; }

    public required string Topic { get; set; }
    public required OutboxMessageType MessageType { get; set; }
    public required string Payload { get; set; }
    public required string MessageKey { get; set; }
    public required DateTime OccurredAtUtc { get; set; }
    public required int RetryCount { get; set; }

    public DateTime? PublishedAtUtc { get; set; }
    public string? LastError { get; set; }
}
