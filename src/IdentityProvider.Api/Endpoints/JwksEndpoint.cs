using IdentityProvider.Configuration;
using Microsoft.Extensions.Options;
using MinimalEndpoints.Abstractions;

namespace IdentityProvider.Api.Endpoints;

public class JwksEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(".well-known/jwks", Handle);
    }

    private static IResult Handle(IOptionsMonitor<JwtConfiguration> jwtConfigMonitor)
    {
        var jwtConfig = jwtConfigMonitor.CurrentValue;

        // Get RSA public key parameters
        var rsa = jwtConfig.RsaPublicKey.Rsa;
        var parameters = rsa.ExportParameters(false); // Export public key only

        // Convert to Base64URL format
        var n = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Encode(parameters.Modulus);
        var e = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Encode(parameters.Exponent);

        var jwks = new
        {
            keys = new[]
            {
                new
                {
                    kty = "RSA", // Key type: RSA
                    use = "sig", // Key use: signature
                    alg = "RS256", // Algorithm
                    kid = "default-rsa-key", // Key ID
                    n, // Modulus
                    e  // Exponent
                }
            }
        };

        return Results.Ok(jwks);
    }
}
