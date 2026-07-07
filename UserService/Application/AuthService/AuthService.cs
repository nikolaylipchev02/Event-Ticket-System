using Microsoft.AspNetCore.Identity;
using UserService.Application.DTOs;
using UserService.Domain.Entities;

namespace UserService.Application.AuthService;

public class AuthService : IAuthService {

    readonly IUserRepository _userRepository;
    readonly IPasswordHasher<User> _passwordHasher;
    
    public AuthService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher) {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }
    
    public async Task<UserResponseDto?> Register(RegisterUserRequestDto request) {
        User? userWithThisEmailExists = await _userRepository.GetByEmail(request.Email);
        
        if (userWithThisEmailExists is not null) {
            return null;
        }
        
        User user = new() {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PasswordHash = request.Password,
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        return new UserResponseDto() {
            Id = user.Id,
            Name = request.Name,
            Email = request.Email,
            Role = user.Role
        };
    }

    public async Task<UserResponseDto?> Login(LoginUserRequestDto request) {
        User? user = await _userRepository.GetByEmail(request.Email);

        if (user is null) {
            return null;
        }

        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed) {
            return null;
        }

        return new UserResponseDto() {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };
    }
}
