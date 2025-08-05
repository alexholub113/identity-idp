using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MinimalEndpoints.Abstractions;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using IdentityProvider.Server.Api.Endpoints.Auth.AuthorizeEndpoint;
using IdentityProvider.Server.Api.Models;
using IdentityProvider.Server.Configuration.Models;

namespace IdentityProvider.Server.Api.Endpoints.Auth.TokenEndpoint;

internal class TokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("connect/token", Handle);
    }

    private static Task<IResult> Handle(
        [FromForm] TokenRequest request,
        IOptionsMonitor<OAuthClients> oauthClientsMonitor,
        HttpContext httpContext)
    {
        // Basic validation for authorization code flow
        if (request.GrantType != "authorization_code")
        {
            return Task.FromResult(Results.BadRequest(new
            {
                error = "unsupported_grant_type",
                error_description = "Only authorization_code grant type is supported"
            }));
        }

        if (string.IsNullOrEmpty(request.Code))
        {
            return Task.FromResult(Results.BadRequest(new
            {
                error = "invalid_request",
                error_description = "code is required"
            }));
        }

        if (string.IsNullOrEmpty(request.ClientId))
        {
            return Task.FromResult(Results.BadRequest(new
            {
                error = "invalid_request",
                error_description = "client_id is required"
            }));
        }

        // Validate client
        var clients = oauthClientsMonitor.CurrentValue;
        if (!clients.TryGetValue(request.ClientId, out var client))
        {
            return Task.FromResult(Results.BadRequest(new
            {
                error = "invalid_client",
                error_description = "Invalid client_id"
            }));
        }

        // Exchange authorization code for tokens
        if (!AuthorizeGetHandler.TryGetAndRemoveCode(request.Code, out var codeData))
        {
            return Task.FromResult(Results.BadRequest(new
            {
                error = "invalid_grant",
                error_description = "Invalid or expired authorization code"
            }));
        }

        // Validate that the client matches
        if (codeData?.ClientId != request.ClientId)
        {
            return Task.FromResult(Results.BadRequest(new
            {
                error = "invalid_grant",
                error_description = "Authorization code was issued to a different client"
            }));
        }

        // Validate redirect URI if provided
        if (!string.IsNullOrEmpty(request.RedirectUri) && codeData.RedirectUri != request.RedirectUri)
        {
            return Task.FromResult(Results.BadRequest(new
            {
                error = "invalid_grant",
                error_description = "redirect_uri does not match"
            }));
        }

        // Generate tokens
        var accessToken = GenerateAccessToken(codeData);
        var idToken = GenerateIdToken(codeData);

        var response = new
        {
            access_token = accessToken,
            token_type = "Bearer",
            expires_in = 3600, // 1 hour
            id_token = idToken,
            scope = codeData.Scope
        };

        return Task.FromResult(Results.Ok(response));
    }

    private static string GenerateAccessToken(AuthorizationCode code)
    {
        // Simple JWT access token (in production, use proper signing keys)
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("this-is-a-sample-secret-key-for-demo-purposes-only-use-proper-key-in-production"); // Use proper key management

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, code.UserId),
                new Claim(ClaimTypes.Name, code.UserId),
                new Claim("client_id", code.ClientId),
                new Claim("scope", code.Scope)
            ]),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "https://localhost:5001", // Should come from configuration
            Audience = code.ClientId,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static string GenerateIdToken(AuthorizationCode code)
    {
        // Simple JWT ID token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("this-is-a-sample-secret-key-for-demo-purposes-only-use-proper-key-in-production");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("sub", code.UserId),
                new Claim("name", code.UserId),
                new Claim("aud", code.ClientId),
                new Claim("iss", "https://localhost:5001"),
                new Claim("iat", new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("exp", new DateTimeOffset(DateTime.UtcNow.AddHours(1)).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("nonce", Guid.NewGuid().ToString()) // In production, use the nonce from the original request
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "https://localhost:5001",
            Audience = code.ClientId,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

// Token request model
public class TokenRequest
{
    [FromForm(Name = "grant_type")]
    public string GrantType { get; set; } = string.Empty;

    [FromForm(Name = "code")]
    public string? Code { get; set; }

    [FromForm(Name = "redirect_uri")]
    public string? RedirectUri { get; set; }

    [FromForm(Name = "client_id")]
    public string ClientId { get; set; } = string.Empty;

    [FromForm(Name = "client_secret")]
    public string? ClientSecret { get; set; }
}