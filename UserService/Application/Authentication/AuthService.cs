using Microsoft.AspNetCore.Identity;
using UserService.Application.DTOs;
using UserService.Domain.Entities;

namespace UserService.Application.Authentication;

public class AuthService : IAuthService {
    readonly IUserRepository _userRepository;
    readonly IPasswordHasher<User> _passwordHasher;
    readonly IJwtTokenService _jwtTokenService;

    public AuthService(
            IUserRepository userRepository,
            IPasswordHasher<User> passwordHasher,
            IJwtTokenService jwtTokenService
    ) {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto?> Register(RegisterUserRequestDto request, CancellationToken cancellationToken) {
        string normalizedEmail = GetNormalizedEmail(request.Email);

        User? userWithThisEmailExists = await _userRepository.GetByEmail(normalizedEmail, cancellationToken);

        if (userWithThisEmailExists is not null) {
            return null;
        }

        User user = new() {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = normalizedEmail,
                PasswordHash = string.Empty,
                Role = request.Role,
                CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _userRepository.Add(user, cancellationToken);

        return ToAuthResponseDto(user);
    }

    public async Task<AuthResponseDto?> Login(LoginUserRequestDto request, CancellationToken cancellationToken) {
        User? user = await _userRepository.GetByEmail(GetNormalizedEmail(request.Email), cancellationToken);

        if (user is null) {
            return null;
        }

        PasswordVerificationResult result =
                _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        return result == PasswordVerificationResult.Failed ? null : ToAuthResponseDto(user);
    }

    static string GetNormalizedEmail(string email) {
        return email.Trim().ToLowerInvariant();
    }

    AuthResponseDto ToAuthResponseDto(User user) {
        return new AuthResponseDto {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                AccessToken = _jwtTokenService.CreateToken(user)
        };
    }
}
