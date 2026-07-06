using Frontend.Contracts;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Preferences;

public class IndexModel(IPreferenceApiClient preferenceApiClient, ILogger<IndexModel> logger) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid? UserId { get; set; }

    public PreferenceItem? Preference { get; private set; }

    public string? LoadError { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        if (UserId is null)
        {
            return;
        }

        try
        {
            Preference = await preferenceApiClient.GetPreferenceAsync(UserId.Value, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Unable to load preference for {UserId}.", UserId);
            LoadError = "The preference service could not be reached.";
        }
    }
}
