using System.Globalization;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using Frontend.Contracts;

namespace Frontend.Services;

public sealed class EventApiClient(HttpClient httpClient) : IEventApiClient {
    public async Task<IReadOnlyList<EventItem>> GetEventsAsync(CancellationToken cancellationToken = default) {
        List<EventItem>? events = await httpClient.GetFromJsonAsync<List<EventItem>>("api/events", cancellationToken);
        return events ?? [];
    }

    public async Task<IReadOnlyList<EventItem>> GetFilteredEventsAsync(EventFilterRequest request,
            CancellationToken cancellationToken = default) {
        List<KeyValuePair<string, string?>> query = [];

        if (request.City is not null) {
            query.Add(new KeyValuePair<string, string?>("city", request.City.Value.ToString()));
        }

        if (request.Category is not null) {
            query.Add(new KeyValuePair<string, string?>("category", request.Category.Value.ToString()));
        }

        if (request.MinPrice is not null) {
            query.Add(new KeyValuePair<string, string?>("minPrice",
                    request.MinPrice.Value.ToString(CultureInfo.InvariantCulture)));
        }

        if (request.MaxPrice is not null) {
            query.Add(new KeyValuePair<string, string?>("maxPrice",
                    request.MaxPrice.Value.ToString(CultureInfo.InvariantCulture)));
        }

        if (request.FromDate is not null) {
            query.Add(new KeyValuePair<string, string?>("fromDate",
                    request.FromDate.Value.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture)));
        }

        if (request.ToDate is not null) {
            query.Add(new KeyValuePair<string, string?>("toDate",
                    request.ToDate.Value.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture)));
        }

        string requestUri = QueryHelpers.AddQueryString("api/events/filtered", query);
        List<EventItem>? events = await httpClient.GetFromJsonAsync<List<EventItem>>(requestUri, cancellationToken);
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
