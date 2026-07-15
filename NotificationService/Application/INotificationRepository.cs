using NotificationService.Domain.Entities;

namespace NotificationService.Application;

public interface INotificationRepository {
    Task<List<Notification>> GetNotifications(Guid userId, CancellationToken cancellationToken);
}