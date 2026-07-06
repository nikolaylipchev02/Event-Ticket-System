using Frontend.Contracts;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Frontend.Pages.Events;

public class IndexModel(IEventApiClient eventApiClient, ILogger<IndexModel> logger) : PageModel
{
    [BindProperty]
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [StringLength(2000, MinimumLength = 2)]
    public string Description { get; set; } = string.Empty;

    [BindProperty]
    public Guid UpdateId { get; set; }

    [BindProperty]
    public string? UpdateTitle { get; set; }

    [BindProperty]
    public string? UpdateDescription { get; set; }

    [BindProperty]
    public Guid DeleteId { get; set; }

    public IReadOnlyList<EventItem> Events { get; private set; } = [];

    public string? LoadError { get; private set; }

    public string? FormError { get; private set; }

    public bool ShowCreateModal { get; private set; }

    public bool ShowEditModal { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadEventsAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadEventsAsync(cancellationToken);
            FormError = "Please fill in both fields before creating the event.";
            ShowCreateModal = true;
            return Page();
        }

        try
        {
            await eventApiClient.CreateEventAsync(new CreateEventRequest
            {
                Title = Title.Trim(),
                Description = Description.Trim()
            }, cancellationToken);

            StatusMessage = "Event created successfully.";
            return RedirectToPage();
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Unable to create an event.");
            await LoadEventsAsync(cancellationToken);
            FormError = "The event service could not be reached.";
            ShowCreateModal = true;
            return Page();
        }
    }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken cancellationToken)
    {
        if (UpdateId == Guid.Empty)
        {
            await LoadEventsAsync(cancellationToken);
            FormError = "Please select an event to update.";
            ShowEditModal = true;
            return Page();
        }

        string? normalizedTitle = NormalizeUpdateValue(UpdateTitle);
        string? normalizedDescription = NormalizeUpdateValue(UpdateDescription);

        if (normalizedTitle is null && normalizedDescription is null)
        {
            await LoadEventsAsync(cancellationToken);
            FormError = "Please update at least one field.";
            ShowEditModal = true;
            return Page();
        }

        if (normalizedTitle is not null && (normalizedTitle.Length < 2 || normalizedTitle.Length > 200))
        {
            await LoadEventsAsync(cancellationToken);
            FormError = "Title must be between 2 and 200 characters.";
            ShowEditModal = true;
            return Page();
        }

        if (normalizedDescription is not null && (normalizedDescription.Length < 2 || normalizedDescription.Length > 2000))
        {
            await LoadEventsAsync(cancellationToken);
            FormError = "Description must be between 2 and 2000 characters.";
            ShowEditModal = true;
            return Page();
        }

        try
        {
            await eventApiClient.UpdateEventAsync(UpdateId, new UpdateEventRequest
            {
                Title = normalizedTitle,
                Description = normalizedDescription
            }, cancellationToken);

            StatusMessage = "Event updated successfully.";
            return RedirectToPage();
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Unable to update event {EventId}.", UpdateId);
            await LoadEventsAsync(cancellationToken);
            FormError = "The event service could not be reached.";
            ShowEditModal = true;
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(CancellationToken cancellationToken)
    {
        if (DeleteId == Guid.Empty)
        {
            await LoadEventsAsync(cancellationToken);
            LoadError = "Please choose an event to delete.";
            return Page();
        }

        try
        {
            await eventApiClient.DeleteEventAsync(DeleteId, cancellationToken);

            StatusMessage = "Event deleted successfully.";
            return RedirectToPage();
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Unable to delete event {EventId}.", DeleteId);
            await LoadEventsAsync(cancellationToken);
            LoadError = "The event service could not be reached.";
            return Page();
        }
    }

    private async Task LoadEventsAsync(CancellationToken cancellationToken)
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

    private static string? NormalizeUpdateValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
