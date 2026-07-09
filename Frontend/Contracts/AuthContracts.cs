namespace Frontend.Contracts;

public sealed class AuthLoginRequest {
    public required string Email { get; set; }

    public required string Password { get; set; }
}

public sealed class AuthRegisterRequest {
    public required string Name { get; set; }

    public required string Email { get; set; }

    public required string Password { get; set; }

    public bool IsAdmin { get; set; }
}