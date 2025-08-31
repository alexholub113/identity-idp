using IdentityProvider.Server.Api.Models;

namespace IdentityProvider.Server.Api.Services;

/// <summary>
/// Repository interface for managing authorization codes
/// </summary>
internal interface IAuthorizationCodeRepository
{
    /// <summary>
    /// Store an authorization code
    /// </summary>
    ValueTask StoreAsync(string code, AuthorizationCode authorizationCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve and remove an authorization code (single use)
    /// </summary>
    ValueTask<AuthorizationCode?> GetAndRemoveAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clean up expired authorization codes
    /// </summary>
    ValueTask CleanupExpiredAsync(CancellationToken cancellationToken = default);
}
