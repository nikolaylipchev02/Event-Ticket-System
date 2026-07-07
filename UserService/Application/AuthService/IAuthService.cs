using UserService.Application.DTOs;

namespace UserService.Application.AuthService;

public interface IAuthService {
    
    public Task<UserResponseDto?> Register(RegisterUserRequestDto request);
    public Task<UserResponseDto?> Login(LoginUserRequestDto request);
    
}
