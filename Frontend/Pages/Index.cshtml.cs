using Frontend.Contracts;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Frontend.Pages;

public class IndexModel(
        IEventApiClient eventApiClient,
        IOptions<ServiceEndpointsOptions> serviceEndpoints,
        ILogger<IndexModel> logger) : PageModel {
    public IReadOnlyList<EventItem> RecentEvents { get; private set; } = [];

    public int EventCount { get; private set; }

    public string? LoadError { get; private set; }

    public ServiceEndpointsOptions Endpoints { get; } = serviceEndpoints.Value;

    public async Task OnGetAsync(CancellationToken cancellationToken) {
        try {
            IReadOnlyList<EventItem> events = await eventApiClient.GetEventsAsync(cancellationToken);

            EventCount = events.Count;
            RecentEvents = events
                    .OrderByDescending(eventItem => eventItem.CreatedAt)
                    .Take(3)
                    .ToArray();
        }
        catch (Exception exception) {
            logger.LogWarning(exception, "Unable to load events for the dashboard.");
            LoadError = "The dashboard loaded, but the event service was not reachable yet.";
        }
    }
}