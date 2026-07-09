using System.Net.Http.Json;
using Frontend.Contracts;

namespace Frontend.Services;

public sealed class PreferenceApiClient(HttpClient httpClient) : IPreferenceApiClient
{
    public async Task<PreferenceItem?> GetPreferenceAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync("api/preferences", cancellationToken);

        if (response.StatusCode is System.Net.HttpStatusCode.NotFound or System.Net.HttpStatusCode.NoContent)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PreferenceItem>(cancellationToken);
    }

    public async Task UpdatePreferenceAsync(Guid userId, UpdatePreferenceRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PatchAsJsonAsync($"api/preferences/{userId}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
