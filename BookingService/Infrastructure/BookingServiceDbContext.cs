using BookingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure;

public class BookingServiceDbContext : DbContext {
    public BookingServiceDbContext(DbContextOptions<BookingServiceDbContext> options) : base(options) {
    }

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<TicketInventory> TicketsInventory => Set<TicketInventory>();
    public DbSet<BookingIdempotencyRecord> BookingIdempotencyRecords => Set<BookingIdempotencyRecord>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ProcessedIntegrationMessage> ProcessedIntegrationMessages => Set<ProcessedIntegrationMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Booking>(entity => {
            entity.ToTable("bookings");

            entity.HasKey(booking => booking.Id);

            entity.Property(booking => booking.UserId).IsRequired();
            entity.Property(booking => booking.EventId).IsRequired();
            entity.Property(booking => booking.Status).HasDefaultValue(BookingStatus.Booked);
        });

        modelBuilder.Entity<TicketInventory>(entity => {
            entity.ToTable("tickets_inventory");

            entity.HasKey(ticket => ticket.Id);

            entity.Property(ticket => ticket.EventId);
            entity.HasIndex(ticket => ticket.EventId).IsUnique();

            entity.Property(ticket => ticket.RemainingTickets).IsRequired();
        });

        modelBuilder.Entity<BookingIdempotencyRecord>(entity => {
            entity.ToTable("booking_idempotency_records");

            entity.HasKey(record => record.Id);

            entity.Property(record => record.UserId).IsRequired();
            entity.Property(record => record.IdempotencyKey).IsRequired();
            entity.Property(record => record.EventId).IsRequired();
            entity.Property(record => record.BookingId).IsRequired();
            entity.Property(record => record.CreatedAt).IsRequired();

            entity.HasIndex(record => new { record.UserId, record.IdempotencyKey }).IsUnique();
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