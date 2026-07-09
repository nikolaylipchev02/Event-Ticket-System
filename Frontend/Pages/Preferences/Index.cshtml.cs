using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Frontend.Contracts;
using Frontend.Services;
using System.Net;

namespace Frontend.Pages.Preferences;

public class IndexModel(IPreferenceApiClient preferenceApiClient, ILogger<IndexModel> logger) : PageModel {
    [BindProperty]
    public EventCity? City { get; set; }

    [BindProperty]
    public EventCategory? Category { get; set; }

    public PreferenceItem? Preference { get; private set; }

    public string? LoadError { get; private set; }

    public string? FormError { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public IReadOnlyList<SelectListItem> CityOptions { get; } = BuildEnumOptions<EventCity>();

    public IReadOnlyList<SelectListItem> CategoryOptions { get; } = BuildEnumOptions<EventCategory>();

    public async Task OnGetAsync(CancellationToken cancellationToken) {
        await LoadPreferenceAsync(cancellationToken);
        City = Preference?.City;
        Category = Preference?.Category;
    }

    public async Task<IActionResult> OnPostSaveCityAsync(CancellationToken cancellationToken) {
        if (City is null) {
            await LoadPreferenceAsync(cancellationToken);
            FormError = "Please choose a city before saving.";
            return Page();
        }

        return await SavePreferenceAsync(new UpdatePreferenceRequest {
                City = City
        }, "City preference updated.", "Unable to update the city preference.", cancellationToken);
    }

    public async Task<IActionResult> OnPostClearCityAsync(CancellationToken cancellationToken) {
        return await SavePreferenceAsync(new UpdatePreferenceRequest {
                RemoveCity = true
        }, "City preference removed.", "Unable to clear the city preference.", cancellationToken);
    }

    public async Task<IActionResult> OnPostSaveCategoryAsync(CancellationToken cancellationToken) {
        if (Category is null) {
            await LoadPreferenceAsync(cancellationToken);
            FormError = "Please choose a category before saving.";
            return Page();
        }

        return await SavePreferenceAsync(new UpdatePreferenceRequest {
                Category = Category
        }, "Category preference updated.", "Unable to update the category preference.", cancellationToken);
    }

    public async Task<IActionResult> OnPostClearCategoryAsync(CancellationToken cancellationToken) {
        return await SavePreferenceAsync(new UpdatePreferenceRequest {
                RemoveCategory = true
        }, "Category preference removed.", "Unable to clear the category preference.", cancellationToken);
    }

    public string FormatPreferenceValue<TEnum>(TEnum? value) where TEnum : struct, Enum {
        return value is null ? "Not set" : FormatEnumValue(value.Value.ToString());
    }

    public string FormatEnumValue(string value) {
        return FormatEnumName(value);
    }

    async Task<IActionResult> SavePreferenceAsync(UpdatePreferenceRequest request, string successMessage,
            string failureLogMessage, CancellationToken cancellationToken) {
        try {
            await preferenceApiClient.UpdatePreferenceAsync(request, cancellationToken);

            StatusMessage = successMessage;
            return RedirectToPage();
        }
        catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.Unauthorized
                                                             or HttpStatusCode.Forbidden) {
            logger.LogWarning(exception, "Preference service rejected the JWT.");
            await LoadPreferenceAsync(cancellationToken);
            FormError = "Your login token was rejected by the preference service. Please sign in again.";
            return Page();
        }
        catch (Exception exception) {
            logger.LogWarning(exception, failureLogMessage);
            await LoadPreferenceAsync(cancellationToken);
            FormError = "The preference service could not be reached.";
            return Page();
        }
    }

    async Task LoadPreferenceAsync(CancellationToken cancellationToken) {
        Preference = null;
        LoadError = null;

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
