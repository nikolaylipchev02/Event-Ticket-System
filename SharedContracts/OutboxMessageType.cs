namespace SharedContracts;

public enum OutboxMessageType {
    BookingCreated,
    BookingCancelled,
    EventCreated,
    EventUpdated
}
