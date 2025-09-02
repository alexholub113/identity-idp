using IdentityProvider.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using MinimalEndpoints.Abstractions;

namespace IdentityProvider.Api.Endpoints;

public class LogoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("logout", Handle);
        app.MapGet("logout", Handle); // Support both GET and POST
    }

    private static async Task<IResult> Handle(
        HttpContext httpContext,
        IOptionsMonitor<IdentityProviderConfiguration> configMonitor,
        string? post_logout_redirect_uri = null,
        string? id_token_hint = null,
        string? state = null)
    {
        var config = configMonitor.CurrentValue;

        // Sign out the user if they're authenticated
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        // Determine where to redirect after logout
        string redirectUrl;

        if (!string.IsNullOrEmpty(post_logout_redirect_uri))
        {
            // Validate that the redirect URI is allowed (in a real implementation)
            // For now, we'll trust it but you should validate against registered URIs
            redirectUrl = post_logout_redirect_uri;

            // Add state parameter if provided
            if (!string.IsNullOrEmpty(state))
            {
                var separator = redirectUrl.Contains('?') ? "&" : "?";
                redirectUrl = $"{redirectUrl}{separator}state={Uri.EscapeDataString(state)}";
            }
        }
        else
        {
            // Default to frontend logout URL
            redirectUrl = config.FrontendUrls.LogoutUrl;
        }

        return Results.Redirect(redirectUrl);
    }
}
