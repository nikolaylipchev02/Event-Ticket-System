namespace BookingService.Domain.Entities;

public class TicketInventory {
    public required Guid Id { get; set; }
    public required Guid EventId { get; set; }
    public required int RemainingTickets { get; set; }
}
