using NotificationService.Application;
using SharedContracts;

namespace NotificationService.Infrastructure;

public class PreferenceApiClient : IPreferenceApiClient {
    readonly HttpClient _httpClient;

    public PreferenceApiClient(HttpClient httpClient) {
        _httpClient = httpClient;
    }


    public async Task<List<Guid>> GetMatchingUserIds(EventCity city, EventCategory category,
            CancellationToken cancellationToken) {
        List<Guid>? result =
                await _httpClient.GetFromJsonAsync<List<Guid>>(
                        $"api/preferences/matching-users?city={city}&category={category}", cancellationToken);

        return result ?? [];
    }
}