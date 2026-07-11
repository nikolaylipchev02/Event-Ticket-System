using NotificationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace NotificationService.Infrastructure;

public class NotificationServiceDbContext : DbContext {
    public NotificationServiceDbContext(DbContextOptions<NotificationServiceDbContext> options) : base(options) {
    }

    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ProcessedIntegrationMessage> ProcessedIntegrationMessages => Set<ProcessedIntegrationMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Notification>(entity => {
            entity.ToTable("notifications");

            entity.HasKey(notification => notification.Id);

            entity.Property(notification => notification.UserId).IsRequired();
            entity.Property(notification => notification.Message).IsRequired();
            entity.Property(notification => notification.Type).IsRequired();
        });

        modelBuilder.Entity<ProcessedIntegrationMessage>(entity => {
            entity.ToTable("processed_integration_messages");

            entity.HasKey(message => message.Id);

            entity.Property(message => message.MessageId).IsRequired();
            entity.Property(message => message.MessageType).IsRequired();
            entity.Property(message => message.ProcessedAt).IsRequired();

            entity.HasIndex(message => message.MessageId).IsUnique();
        });
    }
}
