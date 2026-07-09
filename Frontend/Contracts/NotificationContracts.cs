namespace Frontend.Contracts;

public enum NotificationType {
    BookingConfirmed,
    EventMatchedPreference
}

public sealed class NotificationItem {
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public DateTime CreatedAt { get; set; }
}