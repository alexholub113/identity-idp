using IdentityProvider.Server.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MinimalEndpoints.Abstractions;
using System.Security.Claims;

namespace IdentityProvider.Server.Api.Endpoints.Account;

public record LoginRequest(
    string Email,
    string Password,
    string? ReturnUrl,
    bool RememberMe = false
);

public record LoginResponse(
    bool Success,
    string? Message,
    string? RedirectUrl = null
);

public class LoginEndpoint : IEndpoint
{
    // Simple demo user store - in a real app, use proper identity storage
    private static readonly Dictionary<string, string> _users = new(StringComparer.OrdinalIgnoreCase)
    {
        { "admin@test.com", "password123" },
        { "user@test.com", "user@test.com" }
    };

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("account/login", HandleGet);
        app.MapPost("account/login", HandlePost);
    }

    private static IResult HandleGet(
        [FromQuery] string? returnUrl,
        [FromServices] IOptions<IdentityProviderConfiguration> config)
    {
        // Always redirect to frontend for GET requests
        var loginUrl = config.Value.FrontendUrls.LoginUrl;

        if (!string.IsNullOrEmpty(returnUrl))
        {
            loginUrl += $"?returnUrl={Uri.EscapeDataString(returnUrl)}";
        }

        return Results.Redirect(loginUrl);
    }

    private static async Task<IResult> HandlePost(
        HttpContext httpContext,
        [FromBody] LoginRequest request,
        [FromServices] IOptions<IdentityProviderConfiguration> config)
    {
        try
        {
            // Validate input
            var validationResult = ValidateLoginRequest(request);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(new LoginResponse(
                    Success: false,
                    Message: validationResult.ErrorMessage
                ));
            }

            // Authenticate user
            var authResult = AuthenticateUser(request.Email, request.Password);
            if (!authResult.IsAuthenticated)
            {
                return Results.Unauthorized(); // Let the client handle the 401
            }

            // Create and sign in user
            await SignInUserAsync(httpContext, authResult.User!, request.RememberMe);

            // Determine redirect URL for successful login
            var redirectUrl = !string.IsNullOrEmpty(request.ReturnUrl)
                ? request.ReturnUrl
                : config.Value.FrontendUrls.DashboardUrl;

            return Results.Ok(new LoginResponse(
                Success: true,
                Message: "Login successful",
                RedirectUrl: redirectUrl
            ));
        }
        catch (Exception)
        {
            // Log the exception (in real app)
            // _logger.LogError(ex, "Login failed for user {Email}", request.Email);

            return Results.Problem(
                title: "Login Error",
                detail: "An error occurred during login. Please try again.",
                statusCode: 500
            );
        }
    }

    private static (bool IsValid, string? ErrorMessage) ValidateLoginRequest(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return (false, "Email is required");

        if (string.IsNullOrWhiteSpace(request.Password))
            return (false, "Password is required");

        if (!IsValidEmail(request.Email))
            return (false, "Invalid email format");

        if (request.Password.Length < 3)
            return (false, "Password must be at least 3 characters long");

        return (true, null);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static (bool IsAuthenticated, UserInfo? User) AuthenticateUser(string email, string password)
    {
        if (_users.TryGetValue(email, out var storedPassword) && password == storedPassword)
        {
            return (true, new UserInfo(email, email));
        }

        return (false, null);
    }

    private static async Task SignInUserAsync(HttpContext httpContext, UserInfo user, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Email),
            new Claim("sub", user.Email)
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

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            claimsPrincipal,
            authProperties);
    }

    private record UserInfo(string Email, string Name);
}