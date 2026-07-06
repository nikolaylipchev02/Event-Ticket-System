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
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string UpdateTitle { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [StringLength(2000, MinimumLength = 2)]
    public string UpdateDescription { get; set; } = string.Empty;

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

        if (!ModelState.IsValid)
        {
            await LoadEventsAsync(cancellationToken);
            FormError = "Please fill in both fields before updating the event.";
            ShowEditModal = true;
            return Page();
        }

        try
        {
            await eventApiClient.UpdateEventAsync(UpdateId, new UpdateEventRequest
            {
                Title = UpdateTitle.Trim(),
                Description = UpdateDescription.Trim()
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
}
