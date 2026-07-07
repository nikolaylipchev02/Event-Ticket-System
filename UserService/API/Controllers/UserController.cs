using Microsoft.AspNetCore.Mvc;
using UserService.Application.AuthService;
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
    public async Task<ActionResult<UserResponseDto>> Register([FromBody] RegisterUserRequestDto request) {
        UserResponseDto? response = await _authService.Register(request);

        if (response is null) {
            return Conflict("A user with that email already exists.");
        }

        return Ok(response);
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<UserResponseDto>> Login([FromBody] LoginUserRequestDto request) {
        UserResponseDto? response = await _authService.Login(request);

        if (response is null) {
            return Unauthorized();
        }
        
        return Ok(response);
    }

}
