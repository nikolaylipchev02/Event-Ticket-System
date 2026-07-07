using Frontend.Contracts;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IUserApiClient userApiClient, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthLoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            UserResponse? user = await userApiClient.LoginAsync(
                new LoginUserRequest
                {
                    Email = request.Email,
                    Password = request.Password
                },
                cancellationToken);

            if (user is null)
            {
                return Unauthorized(new { message = "Email or password is incorrect." });
            }

            return Ok(user);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to log in user with email {Email}.", request.Email);
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "The user service is not reachable right now." });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            UserResponse? user = await userApiClient.RegisterAsync(
                new RegisterUserRequest
                {
                    Name = request.Name,
                    Email = request.Email,
                    Password = request.Password,
                    Role = request.IsAdmin ? UserRole.Admin : UserRole.User
                },
                cancellationToken);

            if (user is null)
            {
                return Conflict(new { message = "A user with that email already exists." });
            }

            return Created($"/api/users/{user.Id}", user);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to register user with email {Email}.", request.Email);
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "The user service is not reachable right now." });
        }
    }
}
