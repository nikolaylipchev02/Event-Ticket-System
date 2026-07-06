using Frontend.Contracts;

namespace Frontend.Services;

public interface IPreferenceApiClient
{
    Task<PreferenceItem?> GetPreferenceAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdatePreferenceAsync(Guid userId, UpdatePreferenceRequest request, CancellationToken cancellationToken = default);
}
