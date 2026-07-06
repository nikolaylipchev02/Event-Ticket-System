using PreferenceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace PreferenceService.Infrastructure;

public class PreferenceServiceDbContext : DbContext {
    
    public PreferenceServiceDbContext(DbContextOptions<PreferenceServiceDbContext> options) : base(options) { }

    public DbSet<Preference> Preferences => Set<Preference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Preference>(entity => {
            entity.ToTable("preferences");

            entity.HasKey(preference => preference.UserId);
        });
    }
}