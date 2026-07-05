using NotificationService.Application;
using NotificationService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationController : ControllerBase {
    
    readonly INotificationRepository _notificationRepository;

    public NotificationController(INotificationRepository notificationRepository) {
        _notificationRepository = notificationRepository;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetNotifications(Guid id) {
        await _notificationRepository.GetNotifications(id);
        
        // TODO: proper return types
        return Ok();
    }
}
