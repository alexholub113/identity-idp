using IdentityProvider.Server.Api.Models;
using IdentityProvider.Server.Api.Services;
using IdentityProvider.Server.Configuration;
using IdentityProvider.Server.Configuration.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MinimalEndpoints.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityProvider.Server.Api.Endpoints;

public class TokenRequest
{
    [FromForm]
    public string grant_type { get; set; } = string.Empty;

    [Required]
    [FromForm]
    public required string code { get; set; }

    [FromForm]
    public string? redirect_uri { get; set; }

    [FromForm]
    public string? client_id { get; set; }

    [FromForm]
    public string? client_secret { get; set; }
}

internal class TokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("token", Handle)
           .DisableAntiforgery();
    }

    private static async Task<IResult> Handle(
        [FromForm] TokenRequest request,
        IOptionsMonitor<OAuthClients> oauthClientsMonitor,
        IOptionsMonitor<JwtConfiguration> jwtConfigMonitor,
        IAuthorizationCodeRepository repository,
        IUserRepository userRepository,
        HttpContext httpContext)
    {
        // Basic validation for authorization code flow
        if (request.grant_type != "authorization_code")
        {
            return Results.BadRequest(new
            {
                error = "unsupported_grant_type",
                error_description = "Only authorization_code grant type is supported"
            });
        }

        // Exchange authorization code for tokens using repository first
        // This gives us the client_id from the authorization flow
        var codeData = await repository.GetAndRemoveAsync(request.code);
        if (codeData == null)
        {
            return Results.BadRequest(new
            {
                error = "invalid_grant",
                error_description = "Invalid or expired authorization code"
            });
        }

        // Check if expired
        if (DateTime.UtcNow > codeData.ExpiresAt)
        {
            return Results.BadRequest(new
            {
                error = "invalid_grant",
                error_description = "Authorization code has expired"
            });
        }

        // Use client_id from authorization code if not provided in request
        var clientId = !string.IsNullOrEmpty(request.client_id) ? request.client_id : codeData.ClientId;

        // Validate client exists
        var clients = oauthClientsMonitor.CurrentValue;
        if (!clients.ContainsKey(clientId))
        {
            return Results.BadRequest(new
            {
                error = "invalid_client",
                error_description = "Invalid client_id"
            });
        }

        // Validate that the client matches the one from authorization code
        if (codeData.ClientId != clientId)
        {
            return Results.BadRequest(new
            {
                error = "invalid_grant",
                error_description = "Authorization code was issued to a different client"
            });
        }

        // Validate redirect URI if provided
        if (!string.IsNullOrEmpty(request.redirect_uri) && codeData.RedirectUri != request.redirect_uri)
        {
            return Results.BadRequest(new
            {
                error = "invalid_grant",
                error_description = "redirect_uri does not match"
            });
        }

        // Generate tokens
        var jwtConfig = jwtConfigMonitor.CurrentValue;
        var accessToken = await GenerateAccessTokenAsync(codeData, jwtConfig, userRepository);
        var idToken = await GenerateIdTokenAsync(codeData, jwtConfig, userRepository);

        var response = new
        {
            access_token = accessToken,
            token_type = "Bearer",
            expires_in = jwtConfig.AccessTokenExpirationMinutes * 60, // Convert minutes to seconds
            id_token = idToken,
            scope = codeData.Scope
        };

        return Results.Ok(response);
    }

    private static async Task<string> GenerateAccessTokenAsync(AuthorizationCode code, JwtConfiguration jwtConfig, IUserRepository userRepository)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        // Access tokens should be minimal - only authorization-related claims
        var claims = new List<Claim>
        {
            new("sub", code.UserId), // Standard OAuth2/OIDC subject claim (user identifier)
            new("client_id", code.ClientId),
            new("scope", code.Scope) // Keep original scope string for compatibility
        };

        // Add individual scope claims for better granular access control
        if (!string.IsNullOrEmpty(code.Scope))
        {
            var individualScopes = code.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var scope in individualScopes)
            {
                claims.Add(new Claim("scp", scope)); // 'scp' is a common claim for individual scopes
            }
        }

        // Use RSA signing with proper kid header
        var signingCredentials = new SigningCredentials(jwtConfig.RsaPrivateKey, SecurityAlgorithms.RsaSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(jwtConfig.AccessTokenExpirationMinutes),
            Issuer = jwtConfig.Issuer,
            Audience = code.ClientId,
            SigningCredentials = signingCredentials
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return await Task.FromResult(tokenHandler.WriteToken(token));
    }

    private static async Task<string> GenerateIdTokenAsync(AuthorizationCode code, JwtConfiguration jwtConfig, IUserRepository userRepository)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var issuedAt = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        var expiresAt = new DateTimeOffset(DateTime.UtcNow.AddMinutes(jwtConfig.IdTokenExpirationMinutes)).ToUnixTimeSeconds();

        // Fetch user data from repository
        var user = await userRepository.GetByIdAsync(code.UserId);

        var claims = new List<Claim>
        {
            new("sub", code.UserId), // OIDC standard claim
            new(ClaimTypes.NameIdentifier, code.UserId), // .NET Framework claim
            new("aud", code.ClientId),
            new("iss", jwtConfig.Issuer),
            new("iat", issuedAt.ToString(), ClaimValueTypes.Integer64),
            new("exp", expiresAt.ToString(), ClaimValueTypes.Integer64)
        };

        // Add nonce if it was provided in the original authorization request
        if (!string.IsNullOrEmpty(code.Nonce))
        {
            claims.Add(new Claim("nonce", code.Nonce));
        }

        // Add identity-related claims based on scopes
        if (!string.IsNullOrEmpty(code.Scope))
        {
            var scopes = code.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Standard OpenID Connect profile scope claims
            if (scopes.Contains("profile") && user != null)
            {
                var displayName = string.Empty;
                if (!string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName))
                {
                    displayName = $"{user.FirstName} {user.LastName}";
                    claims.Add(new Claim("name", displayName)); // OIDC standard claim
                    claims.Add(new Claim(ClaimTypes.Name, displayName)); // .NET Framework claim
                }
                else if (!string.IsNullOrEmpty(user.FirstName))
                {
                    displayName = user.FirstName;
                    claims.Add(new Claim("name", displayName));
                    claims.Add(new Claim(ClaimTypes.Name, displayName));
                }
                else if (!string.IsNullOrEmpty(user.Username))
                {
                    displayName = user.Username;
                    claims.Add(new Claim(ClaimTypes.Name, displayName));
                }

                claims.Add(new Claim("preferred_username", user.Username));

                if (!string.IsNullOrEmpty(user.FirstName))
                {
                    claims.Add(new Claim("given_name", user.FirstName));
                    claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
                }

                if (!string.IsNullOrEmpty(user.LastName))
                {
                    claims.Add(new Claim("family_name", user.LastName));
                    claims.Add(new Claim(ClaimTypes.Surname, user.LastName));
                }

                if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                    claims.Add(new Claim("picture", user.ProfilePictureUrl));
            }

            // Standard OpenID Connect email scope claims
            if (scopes.Contains("email") && user != null)
            {
                claims.Add(new Claim("email", user.Email)); // OIDC standard claim
                claims.Add(new Claim(ClaimTypes.Email, user.Email)); // .NET Framework claim
                claims.Add(new Claim("email_verified", user.EmailVerified.ToString().ToLower()));
            }

            // Standard OpenID Connect phone scope claims
            if (scopes.Contains("phone") && user != null && !string.IsNullOrEmpty(user.PhoneNumber))
            {
                claims.Add(new Claim("phone_number", user.PhoneNumber));
                claims.Add(new Claim("phone_number_verified", user.PhoneNumberVerified.ToString().ToLower()));
            }

            // Standard OpenID Connect address scope claims
            if (scopes.Contains("address") && user?.Address != null)
            {
                if (!string.IsNullOrEmpty(user.Address.Formatted))
                    claims.Add(new Claim("address", user.Address.Formatted));

                // Add structured address claims
                var addressClaims = new Dictionary<string, string?>();
                if (!string.IsNullOrEmpty(user.Address.StreetAddress))
                    addressClaims["street_address"] = user.Address.StreetAddress;
                if (!string.IsNullOrEmpty(user.Address.Locality))
                    addressClaims["locality"] = user.Address.Locality;
                if (!string.IsNullOrEmpty(user.Address.Region))
                    addressClaims["region"] = user.Address.Region;
                if (!string.IsNullOrEmpty(user.Address.PostalCode))
                    addressClaims["postal_code"] = user.Address.PostalCode;
                if (!string.IsNullOrEmpty(user.Address.Country))
                    addressClaims["country"] = user.Address.Country;

                if (addressClaims.Any())
                {
                    var addressJson = System.Text.Json.JsonSerializer.Serialize(addressClaims);
                    claims.Add(new Claim("address", addressJson));
                }
            }

            // Add scope information to the ID token
            claims.Add(new Claim("scope", code.Scope));
        }

        // Use RSA signing with proper kid header
        var signingCredentials = new SigningCredentials(jwtConfig.RsaPrivateKey, SecurityAlgorithms.RsaSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(jwtConfig.IdTokenExpirationMinutes),
            Issuer = jwtConfig.Issuer,
            Audience = code.ClientId,
            SigningCredentials = signingCredentials
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}