using Frontend.Contracts;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Notifications;

public class IndexModel(INotificationApiClient notificationApiClient, ILogger<IndexModel> logger) : PageModel
{
    public IReadOnlyList<NotificationItem> Notifications { get; private set; } = [];

    public string? LoadError { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            Notifications = await notificationApiClient.GetNotificationsAsync(cancellationToken);
        }
        catch (HttpRequestException exception) when (exception.StatusCode is System.Net.HttpStatusCode.Forbidden or System.Net.HttpStatusCode.Unauthorized)
        {
            logger.LogWarning(exception, "Notification service rejected the JWT.");
            LoadError = "Your login token was rejected by the notification service. Please sign in again.";
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Unable to load notifications.");
            LoadError = "The notification service could not be reached.";
        }
    }
}
