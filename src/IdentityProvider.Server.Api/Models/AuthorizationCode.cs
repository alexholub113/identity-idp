namespace IdentityProvider.Server.Api.Models;

internal class AuthorizationCode
{
    public required string ClientId { get; set; }
    public required string UserId { get; set; }
    public required string RedirectUri { get; set; }
    public required string Scope { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
