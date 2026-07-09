using Frontend.Contracts;

namespace Frontend.Services;

public interface IPreferenceApiClient {
    Task<PreferenceItem?> GetPreferenceAsync(CancellationToken cancellationToken = default);

    Task UpdatePreferenceAsync(UpdatePreferenceRequest request, CancellationToken cancellationToken = default);

    Task DeletePreferenceAsync(CancellationToken cancellationToken = default);
}
