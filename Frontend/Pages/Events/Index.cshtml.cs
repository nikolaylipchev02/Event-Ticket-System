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

    public IReadOnlyList<EventItem> Events { get; private set; } = [];

    public string? LoadError { get; private set; }

    public string? FormError { get; private set; }

    public bool ShowCreateModal { get; private set; }

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
