using System.Net.Http.Json;
using Frontend.Contracts;

namespace Frontend.Services;

public sealed class PreferenceApiClient(HttpClient httpClient) : IPreferenceApiClient
{
    public async Task<PreferenceItem?> GetPreferenceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<PreferenceItem>($"api/preferences/{userId}", cancellationToken);
    }

    public async Task UpdatePreferenceAsync(Guid userId, UpdatePreferenceRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PatchAsJsonAsync($"api/preferences/{userId}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
