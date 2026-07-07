using UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace UserService.Infrastructure;

public class UserServiceDbContext : DbContext {
    
    public UserServiceDbContext(DbContextOptions<UserServiceDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<User>(entity => {
            entity.ToTable("users");

            entity.HasKey(user => user.Id);

            entity.Property(user => user.Name).IsRequired();
            
            entity.HasIndex(user => user.Email).IsUnique();
            
            entity.Property(user => user.PasswordHash).IsRequired();
            entity.Property(user => user.Role).HasDefaultValue(UserRole.User);
        });
    }
}