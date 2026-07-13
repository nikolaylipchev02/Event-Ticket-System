using PreferenceService.Domain.Entities;
using SharedContracts;

namespace PreferenceService.Application;

public interface IPreferenceRepository {
    public Task CreatePreference(Preference preference);
    public Task<Preference?> GetPreference(Guid userId);
    public Task<List<Guid>> GetMatchingUserIds(EventCity city, EventCategory category);
    public Task UpdatePreference(Preference preference);
    public Task DeletePreference(Guid userId);
}