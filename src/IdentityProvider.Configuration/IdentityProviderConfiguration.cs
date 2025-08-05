using IdentityProvider.Configuration.Models;

namespace IdentityProvider.Configuration;

public class IdentityProviderConfiguration
{
    public required OAuthClients OAuthClients { get; init; }
    public required FrontendUrls FrontendUrls { get; init; }
}

public class FrontendUrls
{
    public required string LoginUrl { get; init; }
    public required string DashboardUrl { get; init; }
    public required string LogoutUrl { get; init; }
    public required string AccessDeniedUrl { get; init; }
}
