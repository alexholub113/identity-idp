using IdentityProvider.Configuration.Models;

namespace IdentityProvider.Configuration;

public class IdentityProviderConfiguration
{
    public required OAuthClients OAuthClients { get; init; }
}
