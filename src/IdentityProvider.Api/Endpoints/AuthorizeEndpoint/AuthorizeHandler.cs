using IdentityProvider.Api.Configurations;
using IdentityProvider.Api.Models;
using IdentityProvider.Api.Services;
using Microsoft.Extensions.Options;
using MinimalEndpoints.Abstractions;
using System.Security.Claims;
using System.Security.Cryptography;

namespace IdentityProvider.Api.Endpoints.AuthorizeEndpoint;

internal class AuthorizeHandler : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("authorize", HandleGet);
        app.MapPost("authorize", HandlePost)
           .DisableAntiforgery();
    }

    private static Task<IResult> HandleGet(
        [AsParameters] AuthorizeRequest request,
        IOptionsMonitor<OAuthClientsConfiguration> oauthClientsMonitor,
        IAuthorizationCodeRepository repository,
        HttpContext httpContext)
    {
        return Handle(request, oauthClientsMonitor, repository, httpContext);
    }

    private static async Task<IResult> HandlePost(
        IOptionsMonitor<OAuthClientsConfiguration> oauthClientsMonitor,
        IAuthorizationCodeRepository repository,
        HttpContext httpContext)
    {
        // For POST requests, read from form data
        var form = await httpContext.Request.ReadFormAsync();

        var request = new AuthorizeRequest
        {
            ClientId = form["client_id"].FirstOrDefault() ?? string.Empty,
            ResponseType = form["response_type"].FirstOrDefault() ?? string.Empty,
            RedirectUri = form["redirect_uri"].FirstOrDefault(),
            Scope = form["scope"].FirstOrDefault(),
            State = form["state"].FirstOrDefault(),
            Nonce = form["nonce"].FirstOrDefault()
        };

        return await Handle(request, oauthClientsMonitor, repository, httpContext);
    }

    private static async Task<IResult> Handle(
        AuthorizeRequest request,
        IOptionsMonitor<OAuthClientsConfiguration> oauthClientsMonitor,
        IAuthorizationCodeRepository repository,
        HttpContext httpContext)
    {
        // Use the simplified validator
        var validationResult = AuthorizeRequestValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(new
            {
                error = validationResult.Error,
                error_description = validationResult.ErrorDescription,
                state = request.State
            });
        }

        // Get OAuth client configuration
        var clients = oauthClientsMonitor.CurrentValue;
        if (!clients.TryGetValue(request.ClientId, out var client))
        {
            return Results.BadRequest(new
            {
                error = "invalid_client",
                error_description = "Invalid client_id",
                state = request.State
            });
        }

        // Use client's redirect URI if not provided
        var redirectUri = request.RedirectUri ?? client.RedirectUri;

        // Simple redirect URI validation
        if (!string.IsNullOrEmpty(request.RedirectUri) &&
            !string.Equals(request.RedirectUri, client.RedirectUri, StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest(new
            {
                error = "invalid_request",
                error_description = "Invalid redirect_uri",
                state = request.State
            });
        }

        // Check if user is authenticated
        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
        {
            // Build the complete authorize URL to return to after login
            var returnUrl = $"/authorize?client_id={Uri.EscapeDataString(request.ClientId)}&response_type=code";
            if (!string.IsNullOrEmpty(request.RedirectUri))
                returnUrl += $"&redirect_uri={Uri.EscapeDataString(request.RedirectUri)}";
            if (!string.IsNullOrEmpty(request.State))
                returnUrl += $"&state={Uri.EscapeDataString(request.State)}";
            if (!string.IsNullOrEmpty(request.Scope))
                returnUrl += $"&scope={Uri.EscapeDataString(request.Scope)}";
            if (!string.IsNullOrEmpty(request.Nonce))
                returnUrl += $"&nonce={Uri.EscapeDataString(request.Nonce)}";

            return Results.Redirect($"/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        // User is authenticated - generate authorization code
        var authorizationCode = GenerateAuthorizationCode();
        var codeData = new AuthorizationCode
        {
            ClientId = request.ClientId,
            UserId = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                     httpContext?.User?.FindFirst("sub")?.Value ??
                     "unknown",
            RedirectUri = redirectUri,
            Scope = request.Scope ?? "openid profile",
            Nonce = request.Nonce, // Store the nonce for later use in ID token
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10) // 10 minute expiration
        };

        // Store the code using repository
        await repository.StoreAsync(authorizationCode, codeData);

        // Build redirect URL with authorization code
        var separator = redirectUri.Contains('?') ? "&" : "?";
        var responseUrl = $"{redirectUri}{separator}code={Uri.EscapeDataString(authorizationCode)}";

        if (!string.IsNullOrEmpty(request.State))
        {
            responseUrl += $"&state={Uri.EscapeDataString(request.State)}";
        }

        return Results.Redirect(responseUrl);
    }

    private static string GenerateAuthorizationCode()
    {
        // Generate a secure random authorization code
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
