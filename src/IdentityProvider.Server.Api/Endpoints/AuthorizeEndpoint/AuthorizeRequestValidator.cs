namespace IdentityProvider.Server.Api.Endpoints.AuthorizeEndpoint;

/// <summary>
/// Simple validation result for authorization requests
/// </summary>
internal class AuthorizeRequestValidationResult
{
    public bool IsValid => string.IsNullOrEmpty(Error);
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }
}

/// <summary>
/// Simplified validator for authorization requests - only supports authorization code flow
/// </summary>
internal static class AuthorizeRequestValidator
{
    /// <summary>
    /// Validates an authorization request for the basic authorization code flow
    /// </summary>
    /// <param name="request">The authorization request to validate</param>
    /// <returns>A validation result containing any errors found</returns>
    public static AuthorizeRequestValidationResult Validate(AuthorizeRequest request)
    {
        var result = new AuthorizeRequestValidationResult();

        // Validate required parameters
        if (string.IsNullOrEmpty(request.ClientId))
        {
            result.Error = "invalid_request";
            result.ErrorDescription = "client_id is required";
            return result;
        }

        if (string.IsNullOrEmpty(request.ResponseType))
        {
            result.Error = "invalid_request";
            result.ErrorDescription = "response_type is required";
            return result;
        }

        // Only support authorization code flow
        if (request.ResponseType != "code")
        {
            result.Error = "unsupported_response_type";
            result.ErrorDescription = "Only 'code' response type is supported";
            return result;
        }

        return result;
    }
}