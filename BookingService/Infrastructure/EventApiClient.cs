using System.Net;
using BookingService.Application;

namespace BookingService.Infrastructure;

public class EventApiClient : IEventApiClient {
    readonly HttpClient _httpClient;

    public EventApiClient(HttpClient httpClient) {
        _httpClient = httpClient;
    }

    public async Task<bool> EventExists(Guid id, CancellationToken cancellationToken) {
        using HttpResponseMessage response = await _httpClient.GetAsync($"api/events/{id}", cancellationToken);

        return response.StatusCode == HttpStatusCode.OK;
    }
}
