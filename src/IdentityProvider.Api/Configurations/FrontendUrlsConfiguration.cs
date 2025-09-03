using System.ComponentModel.DataAnnotations;

namespace IdentityProvider.Api.Configurations;

/// <summary>
/// Configuration for frontend application URLs
/// </summary>
public class FrontendUrlsConfiguration
{
    public const string SectionName = "FrontendUrls";

    /// <summary>
    /// URL for the login page
    /// </summary>
    [Required]
    [Url]
    public required string LoginUrl { get; init; }

    /// <summary>
    /// URL for the dashboard page after successful login
    /// </summary>
    [Required]
    [Url]
    public required string DashboardUrl { get; init; }

    /// <summary>
    /// URL for the logout page
    /// </summary>
    [Required]
    [Url]
    public required string LogoutUrl { get; init; }

    /// <summary>
    /// URL for the access denied page
    /// </summary>
    [Required]
    [Url]
    public required string AccessDeniedUrl { get; init; }
}
