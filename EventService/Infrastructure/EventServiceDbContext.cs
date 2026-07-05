using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure;

public class EventServiceDbContext : DbContext {
    public EventServiceDbContext(DbContextOptions<EventServiceDbContext> options) : base(options) { }

}