using PreferenceService.Application;
using Microsoft.EntityFrameworkCore;
using PreferenceService.Domain.Entities;
using SharedContracts;

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

    public async Task<List<Guid>> GetMatchingUserIds(EventCity city, EventCategory category) {
        return await _preferenceServiceDbContext.Preferences
                .AsNoTracking()
                .Where(p =>
                        (p.City == null || p.City == city) &&
                        (p.Category == null || p.Category == category))
                .Select(p => p.UserId)
                .ToListAsync();
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