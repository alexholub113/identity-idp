# OpenID Connect Authorization Request Implementation

This document describes the implementation of the OpenID Connect Authorization Request based on the [OpenID Connect Core 1.0 specification](https://openid.net/specs/openid-connect-core-1_0.html).

## Overview

The `AuthorizeRequest` model implements all required and optional parameters for OAuth 2.0 and OpenID Connect authorization requests, providing comprehensive validation and helper methods for different flow types.

## Files Structure

```
src/IdentityProvider.Api/Endpoints/Auth/AuthorizeEndpoint/
??? AuthorizeRequest.cs              # Main request model
??? AuthorizeRequestValidator.cs     # Validation logic
??? AuthorizeGetHandler.cs          # HTTP handler implementation
??? Tests/
    ??? AuthorizeRequestTests.cs     # Unit tests for request model
    ??? AuthorizeRequestValidatorTests.cs # Unit tests for validation
```

## Supported Parameters

### Required Parameters (OAuth 2.0)
- **`client_id`**: OAuth 2.0 Client Identifier
- **`response_type`**: Determines the authorization processing flow

### Optional Parameters (OAuth 2.0)
- **`redirect_uri`**: Redirection URI for the response
- **`scope`**: Space-delimited list of scope values
- **`state`**: Opaque value to maintain state and prevent CSRF

### OpenID Connect Parameters
- **`nonce`**: String to associate Client session with ID Token
- **`prompt`**: Controls authentication and consent prompts
- **`max_age`**: Maximum authentication age in seconds
- **`ui_locales`**: Preferred languages for the user interface
- **`id_token_hint`**: Previously issued ID Token as a hint
- **`login_hint`**: Login identifier hint for the End-User
- **`acr_values`**: Requested Authentication Context Class Reference
- **`claims`**: JSON object containing additional claims
- **`request`**: Request object as a JWT
- **`request_uri`**: URL referencing a Request Object

### PKCE Parameters (RFC 7636)
- **`code_challenge`**: Code challenge for PKCE
- **`code_challenge_method`**: Code challenge method (`plain` or `S256`)

### Advanced Parameters
- **`response_mode`**: Mechanism for returning parameters (`query`, `fragment`, `form_post`)

## Supported Flows

### 1. Authorization Code Flow
```
response_type=code
```
- Most secure flow for confidential clients
- Returns authorization code in query parameter
- Requires server-to-server token exchange

### 2. Implicit Flow
```
response_type=id_token
response_type=token
response_type=id_token token
```
- For public clients (SPAs)
- Returns tokens directly in URL fragment
- **Requires `nonce` parameter for `id_token`**

### 3. Hybrid Flow
```
response_type=code id_token
response_type=code token
response_type=code id_token token
```
- Combination of authorization code and implicit flows
- Returns authorization code and tokens
- **Requires `nonce` parameter when `id_token` is requested**

## Validation Rules

The `AuthorizeRequestValidator` implements comprehensive validation according to the specifications:

### Basic Validation
- ? `client_id` is required
- ? `response_type` is required and must be supported
- ? `redirect_uri` format validation

### OpenID Connect Validation
- ? `openid` scope required when requesting `id_token`
- ? `nonce` required for implicit and hybrid flows with `id_token`

### PKCE Validation
- ? `code_challenge` length between 43-128 characters
- ? `code_challenge_method` must be `plain` or `S256`
- ? Base64URL encoding validation for `S256` method

### Prompt Parameter Validation
- ? Valid values: `none`, `login`, `consent`, `select_account`
- ? `none` cannot be combined with other values

### Response Mode Validation
- ? Valid values: `query`, `fragment`, `form_post`

## Helper Methods

The `AuthorizeRequest` class provides convenient helper methods:

```csharp
// Parse scope string into array
string[] scopes = request.GetScopes();

// Check if it's an OpenID Connect request
bool isOidc = request.IsOpenIdConnectRequest();

// Check if PKCE is being used
bool usesPkce = request.UsesPkce();

// Flow type detection
bool isAuthCode = request.IsAuthorizationCodeFlow();
bool isImplicit = request.IsImplicitFlow();
bool isHybrid = request.IsHybridFlow();

// Parse other parameters
int? maxAge = request.GetMaxAgeSeconds();
string[] prompts = request.GetPrompts();
string[] acrValues = request.GetAcrValues();
string[] locales = request.GetUiLocales();
```

## Usage Example

```csharp
[AsParameters] AuthorizeRequest request,
IOptionsMonitor<OAuthClients> oauthClientsMonitor
```

```csharp
// Validate the request
var validationResult = AuthorizeRequestValidator.Validate(request);
if (!validationResult.IsValid)
{
    return Results.BadRequest(new {
        error = validationResult.Error,
        error_description = validationResult.ErrorDescription
    });
}

// Check client configuration
var clients = oauthClientsMonitor.CurrentValue;
if (!clients.TryGetValue(request.ClientId, out var client))
{
    return Results.BadRequest(new {
        error = "invalid_client",
        error_description = "Unknown client"
    });
}

// Validate PKCE requirements
if (client.RequirePkce && !request.UsesPkce())
{
    return Results.BadRequest(new {
        error = "invalid_request",
        error_description = "PKCE required"
    });
}
```

## Standards Compliance

This implementation follows these specifications:

- **OAuth 2.0 Authorization Framework** ([RFC 6749](https://tools.ietf.org/html/rfc6749))
- **OpenID Connect Core 1.0** ([Specification](https://openid.net/specs/openid-connect-core-1_0.html))
- **Proof Key for Code Exchange (PKCE)** ([RFC 7636](https://tools.ietf.org/html/rfc7636))
- **OAuth 2.0 Multiple Response Type Encoding Practices** ([Draft](https://openid.net/specs/oauth-v2-multiple-response-types-1_0.html))

## Testing

Comprehensive unit tests are provided covering:
- ? Parameter parsing and validation
- ? Flow type detection
- ? Helper method functionality
- ? Validation scenarios (success and error cases)
- ? PKCE validation
- ? OpenID Connect specific rules

Run tests with:
```bash
dotnet test src/IdentityProvider.Api/
```