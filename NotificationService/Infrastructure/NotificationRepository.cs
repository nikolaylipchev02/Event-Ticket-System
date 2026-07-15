using NotificationService.Application;
using NotificationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace NotificationService.Infrastructure;

public class NotificationRepository : INotificationRepository {
    readonly NotificationServiceDbContext _notificationServiceDbContext;

    public NotificationRepository(NotificationServiceDbContext notificationServiceDbContext) {
        _notificationServiceDbContext = notificationServiceDbContext;
    }

    public async Task<List<Notification>> GetNotifications(Guid userId, CancellationToken cancellationToken) {
        return await _notificationServiceDbContext.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .ToListAsync(cancellationToken);
    }
}
