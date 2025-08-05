using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace IdentityProvider.Server.Api.Endpoints.Auth.AuthorizeEndpoint;

/// <summary>
/// Simplified authorization request model for basic authorization code flow
/// </summary>
internal class AuthorizeRequest
{
    /// <summary>
    /// OAuth 2.0 Client Identifier valid at the Authorization Server.
    /// REQUIRED.
    /// </summary>
    [FromQuery(Name = "client_id")]
    [Required]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// OAuth 2.0 Response Type value. Only "code" is supported.
    /// REQUIRED.
    /// </summary>
    [FromQuery(Name = "response_type")]
    [Required]
    public string ResponseType { get; set; } = string.Empty;

    /// <summary>
    /// Redirection URI to which the response will be sent.
    /// OPTIONAL - will use client's registered redirect URI if not provided.
    /// </summary>
    [FromQuery(Name = "redirect_uri")]
    public string? RedirectUri { get; set; }

    /// <summary>
    /// Scope values requested by the client.
    /// OPTIONAL. Space-delimited, case-sensitive list of scope values.
    /// </summary>
    [FromQuery(Name = "scope")]
    public string? Scope { get; set; }

    /// <summary>
    /// Opaque value used to maintain state between the request and the callback.
    /// RECOMMENDED for CSRF protection.
    /// </summary>
    [FromQuery(Name = "state")]
    public string? State { get; set; }
}
