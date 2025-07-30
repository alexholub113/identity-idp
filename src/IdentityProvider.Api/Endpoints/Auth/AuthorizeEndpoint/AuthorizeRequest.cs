using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace IdentityProvider.Api.Endpoints.Auth.AuthorizeEndpoint;

/// <summary>
/// Authorization request model based on OpenID Connect Core 1.0 specification
/// https://openid.net/specs/openid-connect-core-1_0.html#AuthRequest
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
    /// OAuth 2.0 Response Type value that determines the authorization processing flow to be used.
    /// REQUIRED. Valid values are "code", "id_token", "token", or combinations.
    /// </summary>
    [FromQuery(Name = "response_type")]
    [Required]
    public string ResponseType { get; set; } = string.Empty;

    /// <summary>
    /// Redirection URI to which the response will be sent.
    /// OPTIONAL for implicit grant, REQUIRED for authorization code grant.
    /// </summary>
    [FromQuery(Name = "redirect_uri")]
    public string? RedirectUri { get; set; }

    /// <summary>
    /// OpenID Connect requests MUST contain the openid scope value.
    /// OPTIONAL. Space-delimited, case-sensitive list of scope values.
    /// </summary>
    [FromQuery(Name = "scope")]
    public string? Scope { get; set; }

    /// <summary>
    /// Opaque value used to maintain state between the request and the callback.
    /// RECOMMENDED. Unguessable random string to prevent CSRF attacks.
    /// </summary>
    [FromQuery(Name = "state")]
    public string? State { get; set; }

    /// <summary>
    /// String value used to associate a Client session with an ID Token, and to mitigate replay attacks.
    /// OPTIONAL.
    /// </summary>
    [FromQuery(Name = "nonce")]
    public string? Nonce { get; set; }

    /// <summary>
    /// Code challenge for PKCE (Proof Key for Code Exchange).
    /// OPTIONAL but REQUIRED for public clients.
    /// </summary>
    [FromQuery(Name = "code_challenge")]
    public string? CodeChallenge { get; set; }

    /// <summary>
    /// Code challenge method for PKCE. Default is "plain", recommended is "S256".
    /// OPTIONAL.
    /// </summary>
    [FromQuery(Name = "code_challenge_method")]
    public string? CodeChallengeMethod { get; set; }

    /// <summary>
    /// Informs the Authorization Server of the mechanism to be used for returning parameters.
    /// OPTIONAL. Valid values are "query", "fragment", "form_post".
    /// </summary>
    [FromQuery(Name = "response_mode")]
    public string? ResponseMode { get; set; }

    /// <summary>
    /// Space delimited, case sensitive list of ASCII string values that specifies whether the Authorization Server prompts the End-User for reauthentication and consent.
    /// OPTIONAL. Valid values are "none", "login", "consent", "select_account".
    /// </summary>
    [FromQuery(Name = "prompt")]
    public string? Prompt { get; set; }

    /// <summary>
    /// Maximum Authentication Age. Specifies the allowable elapsed time in seconds since the last time the End-User was actively authenticated.
    /// OPTIONAL.
    /// </summary>
    [FromQuery(Name = "max_age")]
    public string? MaxAge { get; set; }

    /// <summary>
    /// End-User's preferred languages and scripts for the user interface.
    /// OPTIONAL. Space-separated list of BCP47 language tag values.
    /// </summary>
    [FromQuery(Name = "ui_locales")]
    public string? UiLocales { get; set; }

    /// <summary>
    /// ID Token previously issued by the Authorization Server being passed as a hint.
    /// OPTIONAL.
    /// </summary>
    [FromQuery(Name = "id_token_hint")]
    public string? IdTokenHint { get; set; }

    /// <summary>
    /// Hint to the Authorization Server about the login identifier the End-User might use to log in.
    /// OPTIONAL.
    /// </summary>
    [FromQuery(Name = "login_hint")]
    public string? LoginHint { get; set; }

    /// <summary>
    /// Requested Authentication Context Class Reference values.
    /// OPTIONAL. Space-separated string that specifies the acr values.
    /// </summary>
    [FromQuery(Name = "acr_values")]
    public string? AcrValues { get; set; }

    /// <summary>
    /// Custom claims parameter for passing additional information.
    /// OPTIONAL. JSON object containing additional claims.
    /// </summary>
    [FromQuery(Name = "claims")]
    public string? Claims { get; set; }

    /// <summary>
    /// Request object as a JWT containing the request parameters.
    /// OPTIONAL. Alternative to passing parameters as query parameters.
    /// </summary>
    [FromQuery(Name = "request")]
    public string? Request { get; set; }

    /// <summary>
    /// URL that references a resource containing a Request Object value.
    /// OPTIONAL. Alternative to the request parameter.
    /// </summary>
    [FromQuery(Name = "request_uri")]
    public string? RequestUri { get; set; }

    /// <summary>
    /// Gets the maximum age as an integer value in seconds.
    /// Returns null if MaxAge is not a valid integer.
    /// </summary>
    public int? GetMaxAgeSeconds()
    {
        return int.TryParse(MaxAge, out var maxAge) ? maxAge : null;
    }

    /// <summary>
    /// Gets the scope values as an array of strings.
    /// </summary>
    public string[] GetScopes()
    {
        return string.IsNullOrEmpty(Scope)
            ? []
            : Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Gets the prompt values as an array of strings.
    /// </summary>
    public string[] GetPrompts()
    {
        return string.IsNullOrEmpty(Prompt)
            ? []
            : Prompt.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Gets the ACR values as an array of strings.
    /// </summary>
    public string[] GetAcrValues()
    {
        return string.IsNullOrEmpty(AcrValues)
            ? []
            : AcrValues.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Gets the UI locales as an array of strings.
    /// </summary>
    public string[] GetUiLocales()
    {
        return string.IsNullOrEmpty(UiLocales)
            ? []
            : UiLocales.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Validates if this is an OpenID Connect request (contains 'openid' scope).
    /// </summary>
    public bool IsOpenIdConnectRequest()
    {
        var scopes = GetScopes();
        return scopes.Contains("openid", StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates if PKCE is being used (code_challenge is present).
    /// </summary>
    public bool UsesPkce()
    {
        return !string.IsNullOrEmpty(CodeChallenge);
    }

    /// <summary>
    /// Gets the response types as an array of strings.
    /// </summary>
    public string[] GetResponseTypes()
    {
        return string.IsNullOrEmpty(ResponseType)
            ? []
            : ResponseType.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Validates if this is an authorization code flow request.
    /// </summary>
    public bool IsAuthorizationCodeFlow()
    {
        var responseTypes = GetResponseTypes();
        return responseTypes.Length == 1 && responseTypes[0].Equals("code", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates if this is an implicit flow request.
    /// </summary>
    public bool IsImplicitFlow()
    {
        var responseTypes = GetResponseTypes();
        return responseTypes.Contains("token", StringComparer.OrdinalIgnoreCase) ||
               responseTypes.Contains("id_token", StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates if this is a hybrid flow request.
    /// </summary>
    public bool IsHybridFlow()
    {
        var responseTypes = GetResponseTypes();
        return responseTypes.Contains("code", StringComparer.OrdinalIgnoreCase) &&
               (responseTypes.Contains("token", StringComparer.OrdinalIgnoreCase) ||
                responseTypes.Contains("id_token", StringComparer.OrdinalIgnoreCase));
    }
}
