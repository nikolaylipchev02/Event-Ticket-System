using System.Net;
using System.Net.Http.Json;
using Frontend.Contracts;

namespace Frontend.Services;

public sealed class UserApiClient(HttpClient httpClient) : IUserApiClient
{
    public async Task<UserResponse?> LoginAsync(LoginUserRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/users/login", request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserResponse>(cancellationToken);
    }

    public async Task<UserResponse?> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/users/register", request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserResponse>(cancellationToken);
    }
}
