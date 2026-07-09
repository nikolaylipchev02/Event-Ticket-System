using System.Net.Http.Json;
using Frontend.Contracts;

namespace Frontend.Services;

public sealed class NotificationApiClient(HttpClient httpClient) : INotificationApiClient
{
    public async Task<IReadOnlyList<NotificationItem>> GetNotificationsAsync(CancellationToken cancellationToken = default)
    {
        List<NotificationItem>? notifications = await httpClient.GetFromJsonAsync<List<NotificationItem>>("api/notifications", cancellationToken);
        return notifications ?? [];
    }
}
