namespace BookingService.Domain.Entities;

public class ProcessedIntegrationMessage {
    public Guid Id { get; set; }
    public required Guid MessageId { get; set; }
    public required string MessageType { get; set; }
    public required DateTime ProcessedAt { get; set; }
}
