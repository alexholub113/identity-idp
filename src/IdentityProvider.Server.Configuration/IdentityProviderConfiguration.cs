using IdentityProvider.Server.Configuration.Models;
using Microsoft.IdentityModel.Tokens;

namespace IdentityProvider.Server.Configuration;

public class IdentityProviderConfiguration
{
    public const string SectionName = "IdentityProvider";

    public required OAuthClients OAuthClients { get; init; }
    public required FrontendUrls FrontendUrls { get; init; }
    public required CorsConfiguration Cors { get; init; }
    public required JwtConfiguration Jwt { get; init; }
}

public class FrontendUrls
{
    public const string SectionName = "FrontendUrls";

    public required string LoginUrl { get; init; }
    public required string DashboardUrl { get; init; }
    public required string LogoutUrl { get; init; }
    public required string AccessDeniedUrl { get; init; }
}

public class CorsConfiguration
{
    public const string SectionName = "Cors";

    public required string[] AllowedOrigins { get; init; }
}

public class JwtConfiguration
{
    public const string SectionName = "Jwt";

    public required string SecretKey { get; init; }
    public required string Issuer { get; init; }
    public string Algorithm { get; init; } = "RS256";
    public int AccessTokenExpirationMinutes { get; init; } = 60;
    public int IdTokenExpirationMinutes { get; init; } = 60;

    // For RSA keys, we'll generate them at runtime for this demo
    // In production, you'd store these securely
    private static readonly Lazy<(RsaSecurityKey privateKey, RsaSecurityKey publicKey)> _rsaKeys = new(() =>
    {
        var rsa = System.Security.Cryptography.RSA.Create(2048);
        var privateKey = new RsaSecurityKey(rsa) { KeyId = "default-rsa-key" };

        // Create a public-only RSA key for signature validation
        var publicRsa = System.Security.Cryptography.RSA.Create();
        publicRsa.ImportRSAPublicKey(rsa.ExportRSAPublicKey(), out _);
        var publicKey = new RsaSecurityKey(publicRsa) { KeyId = "default-rsa-key" };

        return (privateKey, publicKey);
    });

    public RsaSecurityKey RsaPrivateKey => _rsaKeys.Value.privateKey;
    public RsaSecurityKey RsaPublicKey => _rsaKeys.Value.publicKey;
}
