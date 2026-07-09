using Frontend.Contracts;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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

            SetAuthCookie(user.AccessToken);
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

            SetAuthCookie(user.AccessToken);
            return Created($"/api/users/{user.Id}", user);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to register user with email {Email}.", request.Email);
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "The user service is not reachable right now." });
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(AuthTokenConstants.AccessTokenCookieName, new CookieOptions
        {
            Path = "/"
        });

        return NoContent();
    }

    private void SetAuthCookie(string accessToken)
    {
        Response.Cookies.Append(AuthTokenConstants.AccessTokenCookieName, accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = ResolveTokenExpiration(accessToken)
        });
    }

    private static DateTimeOffset ResolveTokenExpiration(string accessToken)
    {
        string[] tokenParts = accessToken.Split('.');

        if (tokenParts.Length < 2)
        {
            return DateTimeOffset.UtcNow.AddHours(1);
        }

        try
        {
            byte[] payloadBytes = Base64UrlDecode(tokenParts[1]);
            using JsonDocument payload = JsonDocument.Parse(payloadBytes);

            if (payload.RootElement.TryGetProperty("exp", out JsonElement expirationElement) && expirationElement.TryGetInt64(out long expirationSeconds))
            {
                return DateTimeOffset.FromUnixTimeSeconds(expirationSeconds);
            }
        }
        catch
        {
            // Fall back to a short-lived cookie when the token cannot be decoded.
        }

        return DateTimeOffset.UtcNow.AddHours(1);
    }

    private static byte[] Base64UrlDecode(string value)
    {
        string base64 = value.Replace('-', '+').Replace('_', '/');

        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
        }

        return Convert.FromBase64String(base64);
    }
}
