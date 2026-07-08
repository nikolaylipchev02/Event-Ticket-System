using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<List<Notification>>> GetNotifications(Guid id) {
        string? userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdString is null) {
            return Forbid();
        }
        
        Guid userIdGuid = Guid.Parse(userIdString);
        
        if (id != userIdGuid) {
            return Forbid();
        }
        
        // TODO: proper return types
        return Ok(await _notificationRepository.GetNotifications(userIdGuid));
    }
}
