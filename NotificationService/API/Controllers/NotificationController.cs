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
    [HttpGet]
    public async Task<ActionResult<List<Notification>>> GetNotifications() {
        string? userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userIdString is null) {
            return Forbid();
        }

        // TODO: proper return types
        return Ok(await _notificationRepository.GetNotifications(Guid.Parse(userIdString)));
    }
}
