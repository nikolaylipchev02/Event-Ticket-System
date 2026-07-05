namespace BookingService.Domain.Entities;

public class Booking {

    public Guid Id { get; set;  }
    public required Guid UserId { get; set;  } 
    public required Guid EventId { get; set;  } 
    public BookingStatus Status { get; set;  } 
    public DateTime BookedAt { get; set;  } 
    public DateTime? CancelledAt { get; set;  } 
}