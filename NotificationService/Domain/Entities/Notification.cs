namespace NotificationService.Domain.Entities;

public class Notification {
    
    public Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required string Message { get; set; }
    public required NotificationType Type { get; set; }
    public DateTime CreatedAt { get; set; }

}