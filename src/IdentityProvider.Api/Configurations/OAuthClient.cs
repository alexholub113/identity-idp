using System.ComponentModel.DataAnnotations;

namespace IdentityProvider.Api.Configurations;

/// <summary>
/// OAuth client configuration
/// </summary>
public class OAuthClient
{
    /// <summary>
    /// Unique identifier for the OAuth client
    /// </summary>
    [Required]
    [MinLength(1)]
    public required string ClientId { get; init; }

    /// <summary>
    /// Allowed redirect URI for the client
    /// </summary>
    [Required]
    [Url]
    public required string RedirectUri { get; init; }

    /// <summary>
    /// Scopes that this client is allowed to request
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "At least one scope must be specified")]
    public required string[] Scopes { get; init; }

    /// <summary>
    /// Optional client name for display purposes
    /// </summary>
    public string? ClientName { get; init; }

    /// <summary>
    /// Whether this client requires PKCE (Proof Key for Code Exchange)
    /// </summary>
    public bool RequirePkce { get; init; } = true;

    /// <summary>
    /// Whether this client is allowed to request offline access (refresh tokens)
    /// </summary>
    public bool AllowOfflineAccess { get; init; } = false;

    /// <summary>
    /// Access token lifetime in seconds (if not specified, uses global setting)
    /// </summary>
    [Range(60, 86400, ErrorMessage = "Access token lifetime must be between 1 minute and 24 hours")]
    public int? AccessTokenLifetime { get; init; }
}
