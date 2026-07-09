using PreferenceService.Application;
// using PreferenceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PreferenceService.Domain.Entities;

namespace PreferenceService.Infrastructure;

public class PreferenceRepository : IPreferenceRepository {
    readonly PreferenceServiceDbContext _preferenceServiceDbContext;

    public PreferenceRepository(PreferenceServiceDbContext preferenceServiceDbContext) {
        _preferenceServiceDbContext = preferenceServiceDbContext;
    }

    public async Task CreatePreference(Preference preference) {
        _preferenceServiceDbContext.Preferences.Add(preference);
        await _preferenceServiceDbContext.SaveChangesAsync();
    }

    public async Task<Preference?> GetPreference(Guid userId) {
        return await _preferenceServiceDbContext.Preferences
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task UpdatePreference(Preference preference) {
        _preferenceServiceDbContext.Preferences.Update(preference);
        await _preferenceServiceDbContext.SaveChangesAsync();
    }

    public async Task DeletePreference(Guid userId) {
        Preference? preference = await _preferenceServiceDbContext.Preferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

        if (preference is null) {
            return;
        }

        _preferenceServiceDbContext.Preferences.Remove(preference);
        await _preferenceServiceDbContext.SaveChangesAsync();
    }
}
