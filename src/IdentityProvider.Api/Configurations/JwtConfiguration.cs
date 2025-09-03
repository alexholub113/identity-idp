using System.ComponentModel.DataAnnotations;
using Microsoft.IdentityModel.Tokens;

namespace IdentityProvider.Api.Configurations;

/// <summary>
/// JWT token configuration
/// </summary>
public class JwtConfiguration
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// Secret key for HMAC signing (used as fallback)
    /// </summary>
    [Required]
    [MinLength(32, ErrorMessage = "Secret key must be at least 32 characters long")]
    public required string SecretKey { get; init; }

    /// <summary>
    /// JWT issuer identifier
    /// </summary>
    [Required]
    public required string Issuer { get; init; }

    /// <summary>
    /// Signing algorithm (default: RS256)
    /// </summary>
    public string Algorithm { get; init; } = SecurityAlgorithms.RsaSha256;

    /// <summary>
    /// Access token expiration in minutes (default: 60 minutes)
    /// </summary>
    [Range(1, 43200, ErrorMessage = "Access token expiration must be between 1 minute and 30 days")]
    public int AccessTokenExpirationMinutes { get; init; } = 60;

    /// <summary>
    /// ID token expiration in minutes (default: 60 minutes)
    /// </summary>
    [Range(1, 43200, ErrorMessage = "ID token expiration must be between 1 minute and 30 days")]
    public int IdTokenExpirationMinutes { get; init; } = 60;

    /// <summary>
    /// Key ID for the signing key
    /// </summary>
    public string KeyId { get; init; } = "default-rsa-key";

    /// <summary>
    /// RSA private key for token signing
    /// </summary>
    public RsaSecurityKey RsaPrivateKey => _rsaKeys.Value.privateKey;

    /// <summary>
    /// RSA public key for token validation
    /// </summary>
    public RsaSecurityKey RsaPublicKey => _rsaKeys.Value.publicKey;

    /// <summary>
    /// Gets the access token lifetime as TimeSpan
    /// </summary>
    public TimeSpan AccessTokenLifetime => TimeSpan.FromMinutes(AccessTokenExpirationMinutes);

    /// <summary>
    /// Gets the ID token lifetime as TimeSpan
    /// </summary>
    public TimeSpan IdTokenLifetime => TimeSpan.FromMinutes(IdTokenExpirationMinutes);

    // For RSA keys, we'll generate them at runtime for this demo
    // In production, you'd store these securely (Azure Key Vault, etc.)
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
}
