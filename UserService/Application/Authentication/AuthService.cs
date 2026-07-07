using Microsoft.AspNetCore.Identity;
using UserService.Application.DTOs;
using UserService.Domain.Entities;

namespace UserService.Application.Authentication;

public class AuthService : IAuthService {

    readonly IUserRepository _userRepository;
    readonly IPasswordHasher<User> _passwordHasher;
    readonly JwtTokenService _jwtTokenService;
    
    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        JwtTokenService jwtTokenService
    ) {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }
    
    public async Task<UserResponseDto?> Register(RegisterUserRequestDto request) {
        string normalizedEmail = GetNormalizedEmail(request.Email);
                
        User? userWithThisEmailExists = await _userRepository.GetByEmail(normalizedEmail);
        
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

        await _userRepository.CreateUser(user);
        
        return ToResponseDto(user);
    }

    public async Task<UserResponseDto?> Login(LoginUserRequestDto request) {
        User? user = await _userRepository.GetByEmail(GetNormalizedEmail(request.Email));

        if (user is null) {
            return null;
        }

        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        return result == PasswordVerificationResult.Failed ? null : ToResponseDto(user);
    }

    static string GetNormalizedEmail(string email) {
        return email.Trim().ToLowerInvariant();
    }

    static UserResponseDto ToResponseDto(User user) {
        return new UserResponseDto {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };
    }
}
