using EventService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure;

public class EventServiceDbContext : DbContext {
    public EventServiceDbContext(DbContextOptions<EventServiceDbContext> options) : base(options) { }

    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Event>(entity => {
            entity.ToTable("events");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.City).IsRequired().HasDefaultValue(EventCity.Other);
            entity.Property(e => e.Category).IsRequired().HasDefaultValue(EventCategory.Other);
            entity.Property(e => e.Price).IsRequired();
            entity.Property(e => e.Date).IsRequired();
        });
    }
    
}
