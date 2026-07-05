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

    public async Task<List<Preference>> GetPreferences(Guid userId) {
        return await _preferenceServiceDbContext.Preferences
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .ToListAsync();
    }

    public async Task UpdatePreference(Preference preference) {
        _preferenceServiceDbContext.Preferences.Update(preference);

        await _preferenceServiceDbContext.SaveChangesAsync();
    }
    
    public async Task<Preference?> GetSpecificPreference(Guid userId) {
        return await _preferenceServiceDbContext.Preferences
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.UserId == userId);
    }
}