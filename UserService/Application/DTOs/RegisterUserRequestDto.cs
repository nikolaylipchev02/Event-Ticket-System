using UserService.Domain.Entities;

namespace UserService.Application.DTOs;

public class RegisterUserRequestDto {
    
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required UserRole Role { get; set; }

}
