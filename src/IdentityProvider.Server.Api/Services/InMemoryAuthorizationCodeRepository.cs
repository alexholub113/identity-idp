using IdentityProvider.Server.Api.Models;
using System.Collections.Concurrent;

namespace IdentityProvider.Server.Api.Services;

/// <summary>
/// In-memory implementation of authorization code repository
/// Thread-safe using ConcurrentDictionary
/// </summary>
internal class InMemoryAuthorizationCodeRepository(ILogger<InMemoryAuthorizationCodeRepository> logger) : IAuthorizationCodeRepository
{
    private readonly ConcurrentDictionary<string, AuthorizationCode> _codes = new();
    private readonly ILogger<InMemoryAuthorizationCodeRepository> _logger = logger;

    public ValueTask StoreAsync(string code, AuthorizationCode authorizationCode, CancellationToken cancellationToken = default)
    {
        _codes[code] = authorizationCode;
        _logger.LogDebug("Stored authorization code for client {ClientId}, user {UserId}",
            authorizationCode.ClientId, authorizationCode.UserId);

        return ValueTask.CompletedTask;
    }

    public ValueTask<AuthorizationCode?> GetAndRemoveAsync(string code, CancellationToken cancellationToken = default)
    {
        if (_codes.TryRemove(code, out var authorizationCode))
        {
            _logger.LogDebug("Retrieved and removed authorization code for client {ClientId}, user {UserId}",
                authorizationCode.ClientId, authorizationCode.UserId);

            return ValueTask.FromResult<AuthorizationCode?>(authorizationCode);
        }

        _logger.LogWarning("Authorization code not found: {Code}", code);
        return ValueTask.FromResult<AuthorizationCode?>(null);
    }

    public ValueTask CleanupExpiredAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var expiredCodes = _codes
            .Where(kvp => kvp.Value.ExpiresAt < now)
            .Select(kvp => kvp.Key)
            .ToList();

        var removedCount = 0;
        foreach (var code in expiredCodes)
        {
            if (_codes.TryRemove(code, out _))
            {
                removedCount++;
            }
        }

        if (removedCount > 0)
        {
            _logger.LogInformation("Cleaned up {Count} expired authorization codes", removedCount);
        }

        return ValueTask.CompletedTask;
    }
}
