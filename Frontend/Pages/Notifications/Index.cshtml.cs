using Frontend.Contracts;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Notifications;

public class IndexModel(INotificationApiClient notificationApiClient, ILogger<IndexModel> logger) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid? UserId { get; set; }

    public IReadOnlyList<NotificationItem> Notifications { get; private set; } = [];

    public string? LoadError { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        if (UserId is null)
        {
            return;
        }

        try
        {
            Notifications = await notificationApiClient.GetNotificationsAsync(UserId.Value, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Unable to load notifications for {UserId}.", UserId);
            LoadError = "The notification service could not be reached.";
        }
    }
}
