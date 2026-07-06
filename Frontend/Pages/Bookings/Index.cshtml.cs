using Frontend.Contracts;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Bookings;

public class IndexModel(IBookingApiClient bookingApiClient, ILogger<IndexModel> logger) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid? UserId { get; set; }

    public IReadOnlyList<BookingItem> Bookings { get; private set; } = [];

    public string? LoadError { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        if (UserId is null)
        {
            return;
        }

        try
        {
            Bookings = await bookingApiClient.GetBookingsAsync(UserId.Value, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Unable to load bookings for {UserId}.", UserId);
            LoadError = "The booking service could not be reached.";
        }
    }
}
