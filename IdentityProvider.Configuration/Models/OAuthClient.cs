namespace IdentityProvider.Configuration.Models;
public class OAuthClient
{
    public required string ClientId { get; init; }

    public required string RedirectUri { get; init; }

    public required string[] Scopes { get; init; }

    public bool RequirePkce { get; init; }
}
