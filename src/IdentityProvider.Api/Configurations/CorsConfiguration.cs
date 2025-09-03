using System.ComponentModel.DataAnnotations;

namespace IdentityProvider.Api.Configurations;

/// <summary>
/// CORS configuration for cross-origin requests
/// </summary>
public class CorsConfiguration
{
    public const string SectionName = "Cors";

    /// <summary>
    /// List of allowed origins for CORS requests
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "At least one allowed origin must be specified")]
    public required string[] AllowedOrigins { get; init; }

    /// <summary>
    /// Whether to allow credentials in CORS requests (default: true)
    /// </summary>
    public bool AllowCredentials { get; init; } = true;

    /// <summary>
    /// Maximum age for preflight cache in seconds (default: 1 hour)
    /// </summary>
    [Range(0, int.MaxValue)]
    public int MaxAge { get; init; } = 3600;
}
