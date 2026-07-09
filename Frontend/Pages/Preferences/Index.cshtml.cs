using Frontend.Contracts;
using Frontend.Services;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Preferences;

public class IndexModel(IPreferenceApiClient preferenceApiClient, ILogger<IndexModel> logger) : PageModel {
    public PreferenceItem? Preference { get; private set; }

    public string? LoadError { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken) {
        try {
            Preference = await preferenceApiClient.GetPreferenceAsync(cancellationToken);
        }
        catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.Unauthorized
                                                             or HttpStatusCode.Forbidden) {
            logger.LogWarning(exception, "Preference service rejected the JWT.");
            LoadError = "Your login token was rejected by the preference service. Please sign in again.";
        }
        catch (Exception exception) {
            logger.LogWarning(exception, "Unable to load preference.");
            LoadError = "The preference service could not be reached.";
        }
    }
}