using System.Net.Http.Json;
using Frontend.Contracts;

namespace Frontend.Services;

public sealed class BookingApiClient(HttpClient httpClient) : IBookingApiClient {
    public async Task<IReadOnlyList<BookingItem>> GetBookingsAsync(CancellationToken cancellationToken = default) {
        List<BookingItem>? bookings =
                await httpClient.GetFromJsonAsync<List<BookingItem>>("api/bookings", cancellationToken);
        return bookings ?? [];
    }

    public async Task BookAsync(CreateBookingRequest request, string idempotencyKey,
            CancellationToken cancellationToken = default) {
        if (string.IsNullOrWhiteSpace(idempotencyKey)) {
            throw new ArgumentException("An idempotency key is required.", nameof(idempotencyKey));
        }

        using HttpRequestMessage httpRequest = new(HttpMethod.Post, "api/bookings") {
                Content = JsonContent.Create(request)
        };
        httpRequest.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey);

        HttpResponseMessage response = await httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task CancelBookingAsync(Guid id, CancellationToken cancellationToken = default) {
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/bookings/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
