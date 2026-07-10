using BookingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure;

public class BookingServiceDbContext : DbContext {
    public BookingServiceDbContext(DbContextOptions<BookingServiceDbContext> options) : base(options) {
    }

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<TicketInventory> TicketsInventory => Set<TicketInventory>();

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
    }
}
