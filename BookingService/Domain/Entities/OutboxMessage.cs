namespace BookingService.Domain.Entities;

public class OutboxMessage {
    public Guid Id { get; set; }

    public required string Topic { get; set; }
    public required string MessageType { get; set; }
    public required string Payload { get; set; }
    public required string MessageKey { get; set; }
    public required DateTime OccurredAt { get; set; }
    public required int RetryCount { get; set; }

    public DateTime? PublishedAt { get; set; }
    public string? LastError { get; set; }
}