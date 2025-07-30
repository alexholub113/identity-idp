namespace IdentityProvider.Api.Endpoints.Auth.AuthorizeEndpoint;

/// <summary>
/// Validation result for authorization requests
/// </summary>
internal class AuthorizeRequestValidationResult
{
    public bool IsValid => !Errors.Any();
    public IReadOnlyDictionary<string, List<string>> Errors => _errors;
    public string? Error => _errors.Keys.FirstOrDefault();
    public string? ErrorDescription => Error != null ? string.Join("; ", _errors[Error]) : null;
    public List<string> ValidationErrors => _errors.Values.SelectMany(v => v).ToList();

    private readonly Dictionary<string, List<string>> _errors = [];

    /// <summary>
    /// Adds an error with the specified code and description
    /// </summary>
    /// <param name="errorCode">OAuth 2.0 error code</param>
    /// <param name="errorDescription">Human-readable error description</param>
    public void AddError(string errorCode, string errorDescription)
    {
        if (!_errors.TryGetValue(errorCode, out var descriptions))
        {
            descriptions = new List<string>();
            _errors[errorCode] = descriptions;
        }

        descriptions.Add(errorDescription);
    }
}

/// <summary>
/// Validator for authorization requests according to OpenID Connect specification
/// </summary>
internal static class AuthorizeRequestValidator
{
    // Standard OAuth 2.0 and OpenID Connect response types
    private static readonly string[] ValidResponseTypes =
    [
        "code",                    // Authorization Code Flow
        "id_token",               // Implicit Flow (OpenID Connect)
        "token",                  // Implicit Flow (OAuth 2.0)
        "code id_token",          // Hybrid Flow
        "code token",             // Hybrid Flow
        "id_token token",         // Hybrid Flow
        "code id_token token"     // Hybrid Flow
    ];

    // Standard OpenID Connect prompt values
    private static readonly string[] ValidPromptValues =
    [
        "none",
        "login",
        "consent",
        "select_account"
    ];

    // Standard response modes
    private static readonly string[] ValidResponseModes =
    [
        "query",
        "fragment",
        "form_post"
    ];

    // Standard PKCE code challenge methods
    private static readonly string[] ValidCodeChallengeMethods =
    [
        "plain",
        "S256"
    ];

    /// <summary>
    /// Validates an authorization request according to OpenID Connect specification
    /// </summary>
    /// <param name="request">The authorization request to validate</param>
    /// <returns>A validation result containing any errors found</returns>
    public static AuthorizeRequestValidationResult Validate(AuthorizeRequest request)
    {
        var result = new AuthorizeRequestValidationResult();

        // Required parameters validation
        ValidateRequiredParameters(request, result);

        // Response type validation
        ValidateResponseType(request, result);

        // Scope validation for OpenID Connect
        ValidateOpenIdConnectScope(request, result);

        // PKCE validation
        ValidatePkce(request, result);

        // Prompt parameter validation
        ValidatePrompt(request, result);

        // Response mode validation
        ValidateResponseMode(request, result);

        // Nonce validation for implicit/hybrid flows
        ValidateNonce(request, result);

        // Validate response type and mode compatibility
        ValidateResponseTypeAndMode(request, result);

        // Validate redirect_uri for authorization code flow
        ValidateRedirectUriRequirements(request, result);

        return result;
    }

    private static void ValidateRequiredParameters(AuthorizeRequest request, AuthorizeRequestValidationResult result)
    {
        if (string.IsNullOrEmpty(request.ClientId))
        {
            result.AddError("invalid_client", "client_id is required");
        }

        if (string.IsNullOrEmpty(request.ResponseType))
        {
            result.AddError("invalid_request", "response_type is required");
        }
    }

    private static void ValidateResponseType(AuthorizeRequest request, AuthorizeRequestValidationResult result)
    {
        if (string.IsNullOrEmpty(request.ResponseType))
            return;

        // Normalize response type (sort the values)
        var responseTypes = request.GetResponseTypes();
        var normalizedResponseType = string.Join(" ", responseTypes.OrderBy(x => x));

        if (!ValidResponseTypes.Contains(normalizedResponseType, StringComparer.OrdinalIgnoreCase))
        {
            result.AddError("unsupported_response_type", $"response_type '{request.ResponseType}' is not supported");
        }
    }

    private static void ValidateOpenIdConnectScope(AuthorizeRequest request, AuthorizeRequestValidationResult result)
    {
        var scopes = request.GetScopes();
        var responseTypes = request.GetResponseTypes();

        // If response type includes id_token, openid scope is required
        if (responseTypes.Contains("id_token", StringComparer.OrdinalIgnoreCase) &&
            !scopes.Contains("openid", StringComparer.OrdinalIgnoreCase))
        {
            result.AddError("invalid_scope", "openid scope is required when requesting id_token");
        }
    }

    private static void ValidatePkce(AuthorizeRequest request, AuthorizeRequestValidationResult result)
    {
        if (string.IsNullOrEmpty(request.CodeChallenge))
            return;

        // Validate code challenge method
        if (!string.IsNullOrEmpty(request.CodeChallengeMethod) &&
            !ValidCodeChallengeMethods.Contains(request.CodeChallengeMethod, StringComparer.OrdinalIgnoreCase))
        {
            result.AddError("invalid_request", $"code_challenge_method '{request.CodeChallengeMethod}' is not supported");
        }

        // Validate code challenge format
        if (request.CodeChallenge.Length < 43 || request.CodeChallenge.Length > 128)
        {
            result.AddError("invalid_request", "code_challenge must be between 43 and 128 characters");
        }

        // For S256 method, validate base64url encoding
        if ("S256".Equals(request.CodeChallengeMethod, StringComparison.OrdinalIgnoreCase) &&
            !IsValidBase64Url(request.CodeChallenge))
        {
            result.AddError("invalid_request", "code_challenge must be base64url encoded for S256 method");
        }
    }

    private static void ValidatePrompt(AuthorizeRequest request, AuthorizeRequestValidationResult result)
    {
        if (string.IsNullOrEmpty(request.Prompt))
            return;

        var prompts = request.GetPrompts();
        var invalidPrompts = prompts.Where(p => !ValidPromptValues.Contains(p, StringComparer.OrdinalIgnoreCase)).ToList();

        foreach (var prompt in invalidPrompts)
        {
            result.AddError("invalid_request", $"prompt value '{prompt}' is not supported");
        }

        // 'none' cannot be combined with other values
        if (prompts.Contains("none", StringComparer.OrdinalIgnoreCase) && prompts.Length > 1)
        {
            result.AddError("invalid_request", "prompt=none cannot be combined with other prompt values");
        }
    }

    private static void ValidateResponseMode(AuthorizeRequest request, AuthorizeRequestValidationResult result)
    {
        if (string.IsNullOrEmpty(request.ResponseMode))
            return;

        if (!ValidResponseModes.Contains(request.ResponseMode, StringComparer.OrdinalIgnoreCase))
        {
            result.AddError("invalid_request", $"response_mode '{request.ResponseMode}' is not supported");
        }
    }

    private static void ValidateNonce(AuthorizeRequest request, AuthorizeRequestValidationResult result)
    {
        var responseTypes = request.GetResponseTypes();

        // Nonce is required for implicit and hybrid flows when requesting id_token
        if (responseTypes.Contains("id_token", StringComparer.OrdinalIgnoreCase) &&
            string.IsNullOrEmpty(request.Nonce))
        {
            result.AddError("invalid_request", "nonce is required when requesting id_token in implicit or hybrid flows");
        }
    }

    private static void ValidateResponseTypeAndMode(AuthorizeRequest request, AuthorizeRequestValidationResult result)
    {
        if (string.IsNullOrEmpty(request.ResponseType) || string.IsNullOrEmpty(request.ResponseMode))
            return;

        // If response_type includes 'token' or 'id_token' and response_mode is 'query', it's a security risk
        var responseTypes = request.GetResponseTypes();

        if ((responseTypes.Contains("token", StringComparer.OrdinalIgnoreCase) ||
             responseTypes.Contains("id_token", StringComparer.OrdinalIgnoreCase)) &&
            "query".Equals(request.ResponseMode, StringComparison.OrdinalIgnoreCase))
        {
            result.AddError("invalid_request", "response_mode 'query' is not allowed for response types including token or id_token due to security considerations");
        }
    }

    private static void ValidateRedirectUriRequirements(AuthorizeRequest request, AuthorizeRequestValidationResult result)
    {
        // redirect_uri is required for authorization code flow
        if (request.IsAuthorizationCodeFlow() && string.IsNullOrEmpty(request.RedirectUri))
        {
            result.AddError("invalid_request", "redirect_uri is required for authorization code flow");
        }
    }

    private static bool IsValidBase64Url(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        // Base64URL uses A-Z, a-z, 0-9, -, _ characters only
        return input.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
    }
}