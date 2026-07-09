using System.Net.Http.Json;
using Frontend.Contracts;

namespace Frontend.Services;

public sealed class EventApiClient(HttpClient httpClient) : IEventApiClient {
    public async Task<IReadOnlyList<EventItem>> GetEventsAsync(CancellationToken cancellationToken = default) {
        List<EventItem>? events = await httpClient.GetFromJsonAsync<List<EventItem>>("api/events", cancellationToken);
        return events ?? [];
    }

    public async Task<EventItem?> GetEventAsync(Guid id, CancellationToken cancellationToken = default) {
        return await httpClient.GetFromJsonAsync<EventItem>($"api/events/{id}", cancellationToken);
    }

    public async Task CreateEventAsync(CreateEventRequest request, CancellationToken cancellationToken = default) {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/events", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateEventAsync(Guid id, UpdateEventRequest request,
            CancellationToken cancellationToken = default) {
        HttpResponseMessage response =
                await httpClient.PatchAsJsonAsync($"api/events/{id}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteEventAsync(Guid id, CancellationToken cancellationToken = default) {
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/events/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}