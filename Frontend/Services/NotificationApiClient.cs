using System.Net.Http.Json;
using Frontend.Contracts;

namespace Frontend.Services;

public sealed class NotificationApiClient(HttpClient httpClient) : INotificationApiClient
{
    public async Task<IReadOnlyList<NotificationItem>> GetNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        List<NotificationItem>? notifications = await httpClient.GetFromJsonAsync<List<NotificationItem>>($"api/notifications/{userId}", cancellationToken);
        return notifications ?? [];
    }
}
