using IdentityProvider.Server.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MinimalEndpoints.Abstractions;

namespace IdentityProvider.Server.Api.Endpoints.Account;

public class LogoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("account/logout", Handle);
        app.MapPost("account/logout", Handle);
    }

    private static async Task<IResult> Handle(
        [FromQuery] string? returnUrl,
        HttpContext httpContext,
        [FromServices] IOptions<IdentityProviderConfiguration> config)
    {
        // Sign out the user
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Redirect to returnUrl or configured logout page
        var redirectUrl = !string.IsNullOrEmpty(returnUrl) ? returnUrl : config.Value.FrontendUrls.LogoutUrl;
        return Results.Redirect(redirectUrl);
    }
}