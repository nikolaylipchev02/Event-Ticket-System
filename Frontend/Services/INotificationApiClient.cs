using Frontend.Contracts;

namespace Frontend.Services;

public interface INotificationApiClient
{
    Task<IReadOnlyList<NotificationItem>> GetNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);
}
