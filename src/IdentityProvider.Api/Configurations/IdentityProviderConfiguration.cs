using System.ComponentModel.DataAnnotations;

namespace IdentityProvider.Api.Configurations;

/// <summary>
/// Main configuration class for the Identity Provider
/// </summary>
public class IdentityProviderConfiguration
{
    public const string SectionName = "IdentityProvider";

    /// <summary>
    /// OAuth client configurations
    /// </summary>
    [Required]
    public required OAuthClientsConfiguration OAuthClients { get; init; }

    /// <summary>
    /// Frontend application URLs
    /// </summary>
    [Required]
    public required FrontendUrlsConfiguration FrontendUrls { get; init; }

    /// <summary>
    /// CORS configuration
    /// </summary>
    [Required]
    public required CorsConfiguration Cors { get; init; }

    /// <summary>
    /// JWT token configuration
    /// </summary>
    [Required]
    public required JwtConfiguration Jwt { get; init; }
}
