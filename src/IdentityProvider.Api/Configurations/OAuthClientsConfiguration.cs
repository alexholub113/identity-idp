using System.ComponentModel.DataAnnotations;

namespace IdentityProvider.Api.Configurations;

/// <summary>
/// Collection of OAuth clients keyed by client ID
/// </summary>
public class OAuthClientsConfiguration : Dictionary<string, OAuthClient>
{
    public const string SectionName = "OAuthClients";

    /// <summary>
    /// Validates that all client configurations are valid
    /// </summary>
    /// <returns>Validation results</returns>
    public IEnumerable<ValidationResult> Validate()
    {
        var results = new List<ValidationResult>();

        if (Count == 0)
        {
            results.Add(new ValidationResult("At least one OAuth client must be configured"));
            return results;
        }

        foreach (var (clientId, client) in this)
        {
            if (client.ClientId != clientId)
            {
                results.Add(new ValidationResult(
                    $"Client ID mismatch: key '{clientId}' does not match client.ClientId '{client.ClientId}'"));
            }

            var validationContext = new ValidationContext(client);
            var clientResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(client, validationContext, clientResults, true))
            {
                results.AddRange(clientResults.Select(vr =>
                    new ValidationResult($"Client '{clientId}': {vr.ErrorMessage}", vr.MemberNames)));
            }
        }

        return results;
    }

    /// <summary>
    /// Gets a client by ID, returns null if not found
    /// </summary>
    /// <param name="clientId">The client ID to search for</param>
    /// <returns>The OAuth client or null</returns>
    public OAuthClient? GetClient(string clientId)
    {
        TryGetValue(clientId, out var client);
        return client;
    }

    /// <summary>
    /// Checks if a client exists and is valid
    /// </summary>
    /// <param name="clientId">The client ID to check</param>
    /// <returns>True if the client exists and is valid</returns>
    public bool IsValidClient(string clientId)
    {
        return ContainsKey(clientId) && this[clientId] != null;
    }
}
