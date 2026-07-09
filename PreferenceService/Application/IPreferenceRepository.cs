using PreferenceService.Domain.Entities;

namespace PreferenceService.Application;

public interface IPreferenceRepository {
    public Task<Preference?> GetPreference(Guid userId);

    public Task UpdatePreference(Preference preference);

    public Task<Preference?> GetSpecificPreference(Guid userId);
}