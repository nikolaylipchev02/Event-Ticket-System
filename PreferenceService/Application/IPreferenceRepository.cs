using PreferenceService.Domain.Entities;
using SharedContracts;

namespace PreferenceService.Application;

public interface IPreferenceRepository {
    public Task CreatePreference(Preference preference, CancellationToken cancellationToken);
    public Task<Preference?> GetPreference(Guid userId, CancellationToken cancellationToken);

    public Task<List<Guid>> GetMatchingUserIds(EventCity city, EventCategory category,
            CancellationToken cancellationToken);

    public Task UpdatePreference(Preference preference, CancellationToken cancellationToken);
    public Task DeletePreference(Guid userId, CancellationToken cancellationToken);
}