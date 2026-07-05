using PreferenceService.Domain.Entities;

namespace PreferenceService.Application;

public interface IPreferenceRepository {

    public Task<List<Preference>> GetPreferences(Guid userId);

    public Task UpdatePreference(Preference preference);
    
    public Task<Preference?> GetSpecificPreference(Guid userId);
}