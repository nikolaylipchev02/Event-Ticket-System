using UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace UserService.Infrastructure;

public class UserServiceDbContext : DbContext {
    
    public UserServiceDbContext(DbContextOptions<UserServiceDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<User>(entity => {
            entity.ToTable("users");

            entity.HasKey(preference => preference.Id);
        });
    }
}