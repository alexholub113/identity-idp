using IdentityProvider.Server.Configuration;
using Microsoft.Extensions.Options;
using MinimalEndpoints.Abstractions;

namespace IdentityProvider.Server.Api.Endpoints;

public class DiscoveryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(".well-known/openid-configuration", Handle);
    }

    private static IResult Handle(
        IOptionsMonitor<IdentityProviderConfiguration> configMonitor,
        HttpContext httpContext)
    {
        var config = configMonitor.CurrentValue;
        var request = httpContext.Request;

        // Build the base URL for the issuer
        var baseUrl = $"{request.Scheme}://{request.Host}";

        // Create the OpenID Connect Discovery document
        var discovery = new
        {
            // Required OpenID Connect Discovery fields
            issuer = config.Jwt.Issuer,
            authorization_endpoint = $"{baseUrl}/authorize",
            token_endpoint = $"{baseUrl}/token",
            userinfo_endpoint = $"{baseUrl}/userinfo",
            jwks_uri = $"{baseUrl}/.well-known/jwks",

            // Supported response types
            response_types_supported = new[]
            {
                "code"
            },

            // Supported subject types
            subject_types_supported = new[]
            {
                "public"
            },

            // Supported ID token signing algorithms
            id_token_signing_alg_values_supported = new[]
            {
                config.Jwt.Algorithm
            },

            // Supported scopes
            scopes_supported = GetSupportedScopes(config),

            // Supported response modes
            response_modes_supported = new[]
            {
                "query",
                "fragment",
                "form_post"
            },

            // Supported grant types
            grant_types_supported = new[]
            {
                "authorization_code"
            },

            // Supported token endpoint authentication methods
            token_endpoint_auth_methods_supported = new[]
            {
                "client_secret_post",
                "client_secret_basic",
                "none" // For public clients
            },

            // Supported claims
            claims_supported = new[]
            {
                "sub",
                "iss",
                "aud",
                "exp",
                "iat",
                "nonce",
                "name",
                "preferred_username",
                "email",
                "email_verified",
                "scope"
            },

            // Optional endpoints
            end_session_endpoint = $"{baseUrl}/logout",

            // Code challenge methods for PKCE
            code_challenge_methods_supported = new[]
            {
                "plain",
                "S256"
            },

            // Additional optional fields
            service_documentation = "https://openid.net/connect/",
            ui_locales_supported = new[]
            {
                "en-US"
            },

            // Custom claims for this implementation
            frontpage_uri = $"{baseUrl}",
        };

        return Results.Ok(discovery);
    }

    private static string[] GetSupportedScopes(IdentityProviderConfiguration config)
    {
        var scopes = new HashSet<string>();

        // Add standard OpenID Connect scopes
        scopes.Add("openid");
        scopes.Add("profile");
        scopes.Add("email");
        scopes.Add("address");
        scopes.Add("phone");
        scopes.Add("offline_access");

        // Add all scopes from configured clients
        foreach (var client in config.OAuthClients.Values)
        {
            foreach (var scope in client.Scopes)
            {
                scopes.Add(scope);
            }
        }

        return scopes.OrderBy(s => s).ToArray();
    }
}
