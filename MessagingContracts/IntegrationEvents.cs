using SharedContracts;

namespace MessagingContracts;

public record BookingCreatedIntegrationEvent(
        Guid MessageId,
        Guid BookingId,
        Guid UserId,
        Guid EventId,
        DateTime OccurredAt
);

public record BookingCancelledIntegrationEvent(
        Guid MessageId,
        Guid BookingId,
        Guid UserId,
        Guid EventId,
        DateTime OccurredAt
);

public record EventCreatedIntegrationEvent(
        Guid MessageId,
        Guid EventId,
        string Title,
        EventCity City,
        EventCategory Category,
        int TotalTickets,
        DateTime OccurredAt
);

public record EventUpdatedIntegrationEvent(
        Guid MessageId,
        Guid EventId,
        string Title,
        EventCity City,
        EventCategory Category,
        DateTime OccurredAt
);
