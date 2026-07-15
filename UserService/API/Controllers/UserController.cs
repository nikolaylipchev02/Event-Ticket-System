using Microsoft.AspNetCore.Mvc;
using UserService.Application.Authentication;
using UserService.Application.DTOs;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase {
    readonly IAuthService _authService;

    public UserController(IAuthService authService) {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterUserRequestDto request,
            CancellationToken cancellationToken) {
        AuthResponseDto? response = await _authService.Register(request, cancellationToken);

        if (response is null) {
            return Conflict("A user with that email already exists.");
        }

        return Created($"/api/users/{response.Id}", response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginUserRequestDto request,
            CancellationToken cancellationToken) {
        AuthResponseDto? response = await _authService.Login(request, cancellationToken);

        if (response is null) {
            return Unauthorized();
        }

        return Ok(response);
    }
}