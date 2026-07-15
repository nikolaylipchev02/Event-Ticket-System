using UserService.Application.DTOs;

namespace UserService.Application.Authentication;

public interface IAuthService {
    public Task<AuthResponseDto?> Register(RegisterUserRequestDto request, CancellationToken cancellationToken);
    public Task<AuthResponseDto?> Login(LoginUserRequestDto request, CancellationToken cancellationToken);
}
