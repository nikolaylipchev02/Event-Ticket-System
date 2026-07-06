using Frontend.Contracts;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Events;

public class IndexModel(IEventApiClient eventApiClient, ILogger<IndexModel> logger) : PageModel
{
    public IReadOnlyList<EventItem> Events { get; private set; } = [];

    public string? LoadError { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            Events = await eventApiClient.GetEventsAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Unable to load events.");
            LoadError = "The event service could not be reached.";
        }
    }
}
