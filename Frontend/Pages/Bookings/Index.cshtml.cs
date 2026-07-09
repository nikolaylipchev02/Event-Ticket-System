using Frontend.Contracts;
using Frontend.Services;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Bookings;

public class IndexModel(IBookingApiClient bookingApiClient, ILogger<IndexModel> logger) : PageModel {
    public IReadOnlyList<BookingItem> Bookings { get; private set; } = [];

    public string? LoadError { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken) {
        try {
            Bookings = await bookingApiClient.GetBookingsAsync(cancellationToken);
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
}