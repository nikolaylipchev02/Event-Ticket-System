using UserService.Domain.Entities;

namespace UserService.Application.DTOs;

public class AuthResponseDto {
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required UserRole Role { get; set; }
    public required string AccessToken { get; set; }
}
