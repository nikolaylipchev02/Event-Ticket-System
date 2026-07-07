using Frontend.Contracts;

namespace Frontend.Services;

public interface IUserApiClient
{
    Task<UserResponse?> LoginAsync(LoginUserRequest request, CancellationToken cancellationToken = default);

    Task<UserResponse?> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
}
