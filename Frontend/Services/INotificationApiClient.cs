using Frontend.Contracts;

namespace Frontend.Services;

public interface INotificationApiClient
{
    Task<IReadOnlyList<NotificationItem>> GetNotificationsAsync(CancellationToken cancellationToken = default);
}
