using EventService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedContracts;

namespace EventService.Infrastructure;

public class EventServiceDbContext : DbContext {
    public EventServiceDbContext(DbContextOptions<EventServiceDbContext> options) : base(options) {
    }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

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
            entity.Property(e => e.TotalTickets).IsRequired();
        });

        modelBuilder.Entity<OutboxMessage>(entity => {
            entity.ToTable("outbox_messages");

            entity.HasKey(message => message.Id);

            entity.Property(message => message.Topic).IsRequired();
            entity.Property(message => message.MessageType).IsRequired();
            entity.Property(message => message.Payload).IsRequired();
            entity.Property(message => message.MessageKey).IsRequired();
            entity.Property(message => message.OccurredAt).IsRequired();
            entity.Property(message => message.RetryCount).IsRequired();
        });
    }
}
