using System.Net;
using BookingService.Application;

namespace BookingService.Infrastructure;

public class EventApiClient : IEventApiClient {
    readonly HttpClient _httpClient;

    public EventApiClient(HttpClient httpClient) {
        _httpClient = httpClient;
    }

    public async Task<bool> EventExists(Guid id) {
        using HttpResponseMessage response = await _httpClient.GetAsync($"api/events/{id}");

        return response.StatusCode == HttpStatusCode.OK;
    }
}
