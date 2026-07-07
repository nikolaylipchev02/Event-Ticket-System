using Microsoft.AspNetCore.Mvc;
using UserService.Application;
using UserService.Application.DTOs;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase {

    readonly AuthService _authService;
    readonly IUserRepository _userRepository;
    
    public UserController(AuthService authService, IUserRepository userRepository) {
        _authService = authService;
        _userRepository = userRepository;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(CreateUserDto request) {
        return Ok();
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserDto request) {
        return Ok();
    }

}
