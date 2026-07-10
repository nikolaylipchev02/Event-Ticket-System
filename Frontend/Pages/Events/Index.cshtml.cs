using Frontend.Contracts;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.ComponentModel.DataAnnotations;

namespace Frontend.Pages.Events;

public class IndexModel(IEventApiClient eventApiClient, IBookingApiClient bookingApiClient, ILogger<IndexModel> logger) : PageModel {
    [BindProperty]
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [StringLength(2000, MinimumLength = 2)]
    public string Description { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    public EventCity? City { get; set; }

    [BindProperty]
    [Required]
    public EventCategory? Category { get; set; }

    [BindProperty]
    [Required]
    public decimal? Price { get; set; }

    [BindProperty]
    [Required]
    public DateTime? Date { get; set; }

    [BindProperty]
    [Range(1, int.MaxValue, ErrorMessage = "Tickets available must be at least 1.")]
    public int TotalTickets { get; set; } = 1;

    [BindProperty] public Guid UpdateId { get; set; }

    [BindProperty] public string? UpdateTitle { get; set; }

    [BindProperty] public string? UpdateDescription { get; set; }

    [BindProperty] public Guid DeleteId { get; set; }

    [BindProperty] public Guid BookEventId { get; set; }

    public IReadOnlyList<EventItem> Events { get; private set; } = [];

    public string? LoadError { get; private set; }

    public string? FormError { get; private set; }

    public bool ShowCreateModal { get; private set; }

    public bool ShowEditModal { get; private set; }

    public IReadOnlyList<SelectListItem> CityOptions { get; } = BuildEnumOptions<EventCity>();

    public IReadOnlyList<SelectListItem> CategoryOptions { get; } = BuildEnumOptions<EventCategory>();

    [TempData] public string? StatusMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken) {
        await LoadEventsAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken) {
        if (!ModelState.IsValid) {
            await LoadEventsAsync(cancellationToken);
            FormError = "Please fill in all required fields before creating the event.";
            ShowCreateModal = true;
            return Page();
        }

        if (City is null || Category is null || Price is null || Date is null) {
            await LoadEventsAsync(cancellationToken);
            FormError = "Please choose a city, category, price, and date for the event.";
            ShowCreateModal = true;
            return Page();
        }

        if (Price.Value < 0.01m) {
            await LoadEventsAsync(cancellationToken);
            FormError = "Price must be at least 0.01.";
            ShowCreateModal = true;
            return Page();
        }

        try {
            await eventApiClient.CreateEventAsync(new CreateEventRequest {
                    Title = Title.Trim(),
                    Description = Description.Trim(),
                    City = City.Value,
                    Category = Category.Value,
                    Price = Price.Value,
                    Date = Date.Value,
                    TotalTickets = TotalTickets
            }, cancellationToken);

            StatusMessage = "Event created successfully.";
            return RedirectToPage();
        }
        catch (Exception exception) {
            logger.LogWarning(exception, "Unable to create an event.");
            await LoadEventsAsync(cancellationToken);
            FormError = "The event service could not be reached.";
            ShowCreateModal = true;
            return Page();
        }
    }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken cancellationToken) {
        if (UpdateId == Guid.Empty) {
            await LoadEventsAsync(cancellationToken);
            FormError = "Please select an event to update.";
            ShowEditModal = true;
            return Page();
        }

        string? normalizedTitle = NormalizeUpdateValue(UpdateTitle);
        string? normalizedDescription = NormalizeUpdateValue(UpdateDescription);

        if (normalizedTitle is null && normalizedDescription is null) {
            await LoadEventsAsync(cancellationToken);
            FormError = "Please update at least one field.";
            ShowEditModal = true;
            return Page();
        }

        if (normalizedTitle is not null && (normalizedTitle.Length < 2 || normalizedTitle.Length > 200)) {
            await LoadEventsAsync(cancellationToken);
            FormError = "Title must be between 2 and 200 characters.";
            ShowEditModal = true;
            return Page();
        }

        if (normalizedDescription is not null &&
            (normalizedDescription.Length < 2 || normalizedDescription.Length > 2000)) {
            await LoadEventsAsync(cancellationToken);
            FormError = "Description must be between 2 and 2000 characters.";
            ShowEditModal = true;
            return Page();
        }

        try {
            await eventApiClient.UpdateEventAsync(UpdateId, new UpdateEventRequest {
                    Title = normalizedTitle,
                    Description = normalizedDescription
            }, cancellationToken);

            StatusMessage = "Event updated successfully.";
            return RedirectToPage();
        }
        catch (Exception exception) {
            logger.LogWarning(exception, "Unable to update event {EventId}.", UpdateId);
            await LoadEventsAsync(cancellationToken);
            FormError = "The event service could not be reached.";
            ShowEditModal = true;
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(CancellationToken cancellationToken) {
        if (DeleteId == Guid.Empty) {
            await LoadEventsAsync(cancellationToken);
            LoadError = "Please choose an event to delete.";
            return Page();
        }

        try {
            await eventApiClient.DeleteEventAsync(DeleteId, cancellationToken);

            StatusMessage = "Event deleted successfully.";
            return RedirectToPage();
        }
        catch (Exception exception) {
            logger.LogWarning(exception, "Unable to delete event {EventId}.", DeleteId);
            await LoadEventsAsync(cancellationToken);
            LoadError = "The event service could not be reached.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostBookAsync(CancellationToken cancellationToken) {
        if (BookEventId == Guid.Empty) {
            await LoadEventsAsync(cancellationToken);
            LoadError = "Please choose an event to book.";
            return Page();
        }

        try {
            await bookingApiClient.BookAsync(new CreateBookingRequest {
                    EventId = BookEventId
            }, cancellationToken);

            StatusMessage = "Event booked successfully.";
            return RedirectToPage();
        }
        catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.Unauthorized
                                                           or HttpStatusCode.Forbidden) {
            logger.LogWarning(exception, "Booking service rejected the JWT for event {EventId}.", BookEventId);
            await LoadEventsAsync(cancellationToken);
            LoadError = "Please sign in again before booking an event.";
            return Page();
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.NotFound) {
            logger.LogWarning(exception, "Booking service could not find event {EventId}.", BookEventId);
            await LoadEventsAsync(cancellationToken);
            LoadError = "That event is no longer available to book.";
            return Page();
        }
        catch (Exception exception) {
            logger.LogWarning(exception, "Unable to book event {EventId}.", BookEventId);
            await LoadEventsAsync(cancellationToken);
            LoadError = "The booking service could not be reached.";
            return Page();
        }
    }

    async Task LoadEventsAsync(CancellationToken cancellationToken) {
        try {
            Events = await eventApiClient.GetEventsAsync(cancellationToken);
        }
        catch (Exception exception) {
            logger.LogWarning(exception, "Unable to load events.");
            LoadError = "The event service could not be reached.";
        }
    }

    static string? NormalizeUpdateValue(string? value) {
        if (string.IsNullOrWhiteSpace(value)) {
            return null;
        }

        return value.Trim();
    }

    public string FormatEnumValue(string value) {
        return FormatEnumName(value);
    }

    static IReadOnlyList<SelectListItem> BuildEnumOptions<TEnum>() where TEnum : struct, Enum {
        return Enum.GetValues<TEnum>()
                .Select(value => new SelectListItem {
                        Value = value.ToString(),
                        Text = FormatEnumName(value.ToString())
                })
                .ToList();
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
