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

    public async Task CreatePreference(Preference preference, CancellationToken cancellationToken) {
        _preferenceServiceDbContext.Preferences.Add(preference);
        await _preferenceServiceDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Preference?> GetPreference(Guid userId, CancellationToken cancellationToken) {
        return await _preferenceServiceDbContext.Preferences
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task<List<Guid>> GetMatchingUserIds(EventCity city, EventCategory category,
            CancellationToken cancellationToken) {
        return await _preferenceServiceDbContext.Preferences
                .AsNoTracking()
                .Where(p =>
                        (p.City == null || p.City == city) &&
                        (p.Category == null || p.Category == category))
                .Select(p => p.UserId)
                .ToListAsync(cancellationToken);
    }

    public async Task UpdatePreference(Preference preference, CancellationToken cancellationToken) {
        _preferenceServiceDbContext.Preferences.Update(preference);
        await _preferenceServiceDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePreference(Guid userId, CancellationToken cancellationToken) {
        Preference? preference = await _preferenceServiceDbContext.Preferences
                .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (preference is null) {
            return;
        }

        _preferenceServiceDbContext.Preferences.Remove(preference);
        await _preferenceServiceDbContext.SaveChangesAsync(cancellationToken);
    }
}