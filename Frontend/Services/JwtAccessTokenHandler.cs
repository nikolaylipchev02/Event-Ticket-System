using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace Frontend.Services;

public sealed class JwtAccessTokenHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler {
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) {
        string? accessToken = ResolveAccessToken();

        if (!string.IsNullOrWhiteSpace(accessToken) && request.Headers.Authorization is null) {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return base.SendAsync(request, cancellationToken);
    }

    string? ResolveAccessToken() {
        HttpContext? httpContext = httpContextAccessor.HttpContext;

        if (httpContext is null) {
            return null;
        }

        if (httpContext.Request.Cookies.TryGetValue(AuthTokenConstants.AccessTokenCookieName, out string? cookieToken)
            && !string.IsNullOrWhiteSpace(cookieToken)) {
            return cookieToken;
        }

        if (httpContext.Request.Headers.Authorization.Count == 1) {
            string authorizationHeader = httpContext.Request.Headers.Authorization.ToString();
            const string bearerPrefix = "Bearer ";

            if (authorizationHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase)) {
                return authorizationHeader[bearerPrefix.Length..].Trim();
            }
        }

        return null;
    }
}