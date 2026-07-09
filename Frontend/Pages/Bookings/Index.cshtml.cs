using Frontend.Contracts;
using Frontend.Services;
using System.Net;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Bookings;

public class IndexModel(IBookingApiClient bookingApiClient, IEventApiClient eventApiClient, ILogger<IndexModel> logger) : PageModel {
    public IReadOnlyList<BookingItem> Bookings { get; private set; } = [];

    public IReadOnlyDictionary<Guid, EventItem> EventsById { get; private set; } = new Dictionary<Guid, EventItem>();

    public string? LoadError { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken) {
        try {
            Bookings = await bookingApiClient.GetBookingsAsync(cancellationToken);
            await LoadEventsAsync(cancellationToken);
        }
        catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.Unauthorized
                                                             or HttpStatusCode.Forbidden) {
            logger.LogWarning(exception, "Booking service rejected the JWT.");
            LoadError = "Your login token was rejected by the booking service. Please sign in again.";
        }
        catch (Exception exception) {
            logger.LogWarning(exception, "Unable to load bookings.");
            LoadError = "The booking service could not be reached.";
        }
    }

    async Task LoadEventsAsync(CancellationToken cancellationToken) {
        HashSet<Guid> eventIds = Bookings.Select(booking => booking.EventId).ToHashSet();

        if (eventIds.Count == 0) {
            EventsById = new Dictionary<Guid, EventItem>();
            return;
        }

        List<Task<(Guid EventId, EventItem? Event)>> lookups = eventIds
                .Select(async eventId => {
                    try {
                        EventItem? eventItem = await eventApiClient.GetEventAsync(eventId, cancellationToken);
                        return (eventId, eventItem);
                    }
                    catch (Exception exception) {
                        logger.LogWarning(exception, "Unable to load event {EventId} for booking display.", eventId);
                        return (eventId, null);
                    }
                })
                .ToList();

        (Guid EventId, EventItem? Event)[] results = await Task.WhenAll(lookups);

        EventsById = results
                .Where(result => result.Event is not null)
                .ToDictionary(result => result.EventId, result => result.Event!);
    }

    public string FormatEventTitle(Guid eventId) {
        return EventsById.TryGetValue(eventId, out EventItem? eventItem)
                ? eventItem.Title
                : eventId.ToString();
    }

    public string FormatCity(Guid eventId) {
        return EventsById.TryGetValue(eventId, out EventItem? eventItem)
                ? FormatEnumName(eventItem.City.ToString())
                : "-";
    }

    public string FormatCategory(Guid eventId) {
        return EventsById.TryGetValue(eventId, out EventItem? eventItem)
                ? FormatEnumName(eventItem.Category.ToString())
                : "-";
    }

    static string FormatEnumName(string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            return value;
        }

        List<char> formatted = [];

        for (int index = 0; index < value.Length; index++) {
            char current = value[index];
            if (index > 0 && char.IsUpper(current) && char.IsLower(value[index - 1])) {
                formatted.Add(' ');
            }

            formatted.Add(current);
        }

        return new string(formatted.ToArray());
    }
}
