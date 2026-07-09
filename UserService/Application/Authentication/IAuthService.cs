using UserService.Application.DTOs;

namespace UserService.Application.Authentication;

public interface IAuthService {
    
    public Task<AuthResponseDto?> Register(RegisterUserRequestDto request);
    public Task<AuthResponseDto?> Login(LoginUserRequestDto request);
    
}
