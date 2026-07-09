using System.Net.Http.Json;
using Frontend.Contracts;

namespace Frontend.Services;

public sealed class BookingApiClient(HttpClient httpClient) : IBookingApiClient
{
    public async Task<IReadOnlyList<BookingItem>> GetBookingsAsync(CancellationToken cancellationToken = default)
    {
        List<BookingItem>? bookings = await httpClient.GetFromJsonAsync<List<BookingItem>>("api/bookings", cancellationToken);
        return bookings ?? [];
    }

    public async Task BookAsync(CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/bookings", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task CancelBookingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/bookings/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
