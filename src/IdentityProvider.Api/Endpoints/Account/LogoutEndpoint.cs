using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MinimalEndpoints.Abstractions;

namespace IdentityProvider.Api.Endpoints.Account;

public class LogoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("account/logout", Handle);
    }

    private static async Task<IResult> Handle(
        [FromQuery] string? returnUrl,
        HttpContext httpContext)
    {
        // Sign out the user
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Redirect to returnUrl or default page
        var redirectUrl = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "/";
        return Results.Redirect(redirectUrl);
    }
}