namespace Frontend.Contracts;

public enum UserRole
{
    User,
    Admin
}

public sealed class UserResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }
}

public sealed class LoginUserRequest
{
    public required string Email { get; set; }

    public required string Password { get; set; }
}

public sealed class RegisterUserRequest
{
    public required string Name { get; set; }

    public required string Email { get; set; }

    public required string Password { get; set; }

    public UserRole Role { get; set; }
}
