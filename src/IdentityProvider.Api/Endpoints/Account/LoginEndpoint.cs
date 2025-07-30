using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MinimalEndpoints.Abstractions;

namespace IdentityProvider.Api.Endpoints.Account;

public record LoginRequest(
    string Username,
    string Password,
    string? ReturnUrl,
    bool RememberMe = false
);

public record LoginResponse(
    bool Success,
    string? RedirectUrl,
    string? ErrorMessage
);

public class LoginEndpoint : IEndpoint
{
    // Simple demo user store - in a real app, use proper identity storage
    private static readonly Dictionary<string, string> _users = new(StringComparer.OrdinalIgnoreCase)
    {
        { "admin", "password123" },
        { "user", "password123" }
    };

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("account/login", HandleGet);
        app.MapPost("account/login", HandlePost);
    }

    private static IResult HandleGet([FromQuery] string? returnUrl)
    {
        // Return a simple login form HTML for testing
        var html = @"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset=""utf-8"" />
                <title>Login</title>
                <style>
                    body { font-family: Arial, sans-serif; margin: 20px; }
                    .form-container { max-width: 400px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }
                    .form-group { margin-bottom: 15px; }
                    label { display: block; margin-bottom: 5px; }
                    input[type=""text""], input[type=""password""] { width: 100%; padding: 8px; box-sizing: border-box; }
                    button { padding: 10px 15px; background-color: #0066cc; color: white; border: none; cursor: pointer; }
                </style>
            </head>
            <body>
                <div class=""form-container"">
                    <h2>Login</h2>
                    <form action=""/account/login"" method=""post"">
                        <div class=""form-group"">
                            <label for=""username"">Username:</label>
                            <input type=""text"" id=""username"" name=""username"" required />
                        </div>
                        <div class=""form-group"">
                            <label for=""password"">Password:</label>
                            <input type=""password"" id=""password"" name=""password"" required />
                        </div>
                        <div class=""form-group"">
                            <label>
                                <input type=""checkbox"" name=""rememberMe"" value=""true"" /> Remember me
                            </label>
                        </div>
                        <input type=""hidden"" name=""returnUrl"" value=""" + (returnUrl ?? "/") + @""" />
                        <button type=""submit"">Login</button>
                    </form>
                </div>
            </body>
            </html>";

        return Results.Content(html, "text/html");
    }

    private static async Task<IResult> HandlePost(
        HttpContext httpContext,
        [FromForm] string username,
        [FromForm] string password,
        [FromForm] string? returnUrl,
        [FromForm] bool rememberMe = false)
    {
        // Validate credentials (in real app, check against database)
        if (!_users.TryGetValue(username, out var storedPassword) || password != storedPassword)
        {
            return Results.BadRequest(new LoginResponse(
                Success: false,
                RedirectUrl: null,
                ErrorMessage: "Invalid username or password"
            ));
        }

        // Create claims for the user
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.NameIdentifier, username),
            // Add other claims as needed (email, roles, etc.)
            new Claim("sub", username),
            new Claim("name", username)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var authProperties = new AuthenticationProperties
        {
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(rememberMe ? 30 : 1),
            IsPersistent = rememberMe,
            IssuedUtc = DateTimeOffset.UtcNow
        };

        // Sign in the user
        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            claimsPrincipal,
            authProperties);

        // Redirect to returnUrl or default page
        var redirectUrl = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "/";
        return Results.Redirect(redirectUrl);
    }
}