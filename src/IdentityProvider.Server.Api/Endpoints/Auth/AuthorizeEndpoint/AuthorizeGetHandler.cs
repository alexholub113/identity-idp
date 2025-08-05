using IdentityProvider.Server.Api.Models;
using IdentityProvider.Server.Configuration.Models;
using Microsoft.Extensions.Options;
using MinimalEndpoints.Abstractions;
using System.Security.Cryptography;

namespace IdentityProvider.Server.Api.Endpoints.Auth.AuthorizeEndpoint;

internal class AuthorizeGetHandler : IEndpoint
{
    // Simple in-memory store for authorization codes (replace with proper storage in production)
    private static readonly Dictionary<string, AuthorizationCode> _authorizationCodes = [];

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("connect/authorize", Handle);
    }

    private static Task<IResult> Handle(
        [AsParameters] AuthorizeRequest request,
        IOptionsMonitor<OAuthClients> oauthClientsMonitor,
        HttpContext httpContext)
    {
        // Use the simplified validator
        var validationResult = AuthorizeRequestValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return Task.FromResult(Results.BadRequest(new
            {
                error = validationResult.Error,
                error_description = validationResult.ErrorDescription,
                state = request.State
            }));
        }

        // Get OAuth client configuration
        var clients = oauthClientsMonitor.CurrentValue;
        if (!clients.TryGetValue(request.ClientId, out var client))
        {
            return Task.FromResult(Results.BadRequest(new
            {
                error = "invalid_client",
                error_description = "Invalid client_id",
                state = request.State
            }));
        }

        // Use client's redirect URI if not provided
        var redirectUri = request.RedirectUri ?? client.RedirectUri;

        // Simple redirect URI validation
        if (!string.IsNullOrEmpty(request.RedirectUri) &&
            !string.Equals(request.RedirectUri, client.RedirectUri, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(Results.BadRequest(new
            {
                error = "invalid_request",
                error_description = "Invalid redirect_uri",
                state = request.State
            }));
        }

        // Check if user is authenticated
        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
        {
            // Build simple login URL
            var returnUrl = $"/connect/authorize?client_id={Uri.EscapeDataString(request.ClientId)}&response_type=code";
            if (!string.IsNullOrEmpty(request.RedirectUri))
                returnUrl += $"&redirect_uri={Uri.EscapeDataString(request.RedirectUri)}";
            if (!string.IsNullOrEmpty(request.State))
                returnUrl += $"&state={Uri.EscapeDataString(request.State)}";
            if (!string.IsNullOrEmpty(request.Scope))
                returnUrl += $"&scope={Uri.EscapeDataString(request.Scope)}";

            return Task.FromResult(Results.Redirect($"/account/login?returnUrl={Uri.EscapeDataString(returnUrl)}"));
        }

        // User is authenticated - generate authorization code
        var authorizationCode = GenerateAuthorizationCode();
        var codeData = new AuthorizationCode
        {
            ClientId = request.ClientId,
            UserId = httpContext?.User?.Identity?.Name ?? "unknown",
            RedirectUri = redirectUri,
            Scope = request.Scope ?? "openid profile",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10) // 10 minute expiration
        };

        // Store the code (in production, use proper storage)
        _authorizationCodes[authorizationCode] = codeData;

        // Clean up expired codes
        CleanupExpiredCodes();

        // Build redirect URL with authorization code
        var separator = redirectUri.Contains('?') ? "&" : "?";
        var responseUrl = $"{redirectUri}{separator}code={Uri.EscapeDataString(authorizationCode)}";

        if (!string.IsNullOrEmpty(request.State))
        {
            responseUrl += $"&state={Uri.EscapeDataString(request.State)}";
        }

        return Task.FromResult(Results.Redirect(responseUrl));
    }

    private static string GenerateAuthorizationCode()
    {
        // Generate a secure random authorization code
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private static void CleanupExpiredCodes()
    {
        var now = DateTime.UtcNow;
        var expiredCodes = _authorizationCodes
            .Where(kvp => kvp.Value.ExpiresAt < now)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var code in expiredCodes)
        {
            _authorizationCodes.Remove(code);
        }
    }

    // Make authorization codes accessible for token exchange
    public static bool TryGetAndRemoveCode(string code, out AuthorizationCode? codeData)
    {
        if (_authorizationCodes.TryGetValue(code, out codeData))
        {
            _authorizationCodes.Remove(code);
            return !IsExpired(codeData);
        }

        codeData = null;
        return false;
    }

    private static bool IsExpired(AuthorizationCode codeData)
    {
        return DateTime.UtcNow > codeData.ExpiresAt;
    }
}
