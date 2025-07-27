using System;
using MinimalEndpoints.Abstractions;

namespace IdentityProvider.Api.Endpoints.Auth;

public record UserInfoRequest(string AccessToken);

public record UserInfoResponse(
    string Sub,
    string? Name,
    string? GivenName,
    string? FamilyName,
    string? Email,
    bool? EmailVerified,
    string? Picture,
    string? Locale,
    string? ZoneInfo,
    DateTime? UpdatedAt
);

public class UserInfoEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("connect/userinfo", Handle);
        app.MapPost("connect/userinfo", Handle);
    }

    private static async Task<IResult> Handle(HttpContext context)
    {
        // Extract access token from Authorization header or request body
        string? accessToken = null;

        // Try Authorization header first (Bearer token)
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ") == true)
        {
            accessToken = authHeader.Substring("Bearer ".Length);
        }

        if (string.IsNullOrEmpty(accessToken))
        {
            return Results.Unauthorized();
        }

        // Implement UserInfo endpoint logic here
        // This should:
        // - Validate the access token
        // - Extract user claims from the token
        // - Return user information based on granted scopes

        throw new NotImplementedException();
    }
}
