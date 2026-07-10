using Frontend.Contracts;

namespace Frontend.Services;

public interface IBookingApiClient {
    Task<IReadOnlyList<BookingItem>> GetBookingsAsync(CancellationToken cancellationToken = default);
    Task BookAsync(CreateBookingRequest request, string idempotencyKey,
            CancellationToken cancellationToken = default);
    Task CancelBookingAsync(Guid id, CancellationToken cancellationToken = default);
}
