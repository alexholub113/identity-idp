using System;
using Microsoft.Extensions.Options;
using MinimalEndpoints.Abstractions;
using IdentityProvider.Configuration;
using IdentityProvider.Configuration.Models;

namespace IdentityProvider.Api.Endpoints.Account;

public record AuthorizeRequest(
    string ClientId,
    string ResponseType,
    string? RedirectUri,
    string? Scope,
    string? State,
    string? Nonce,
    string? CodeChallenge,
    string? CodeChallengeMethod,
    string? ResponseMode,
    string? Prompt,
    string? MaxAge,
    string? UiLocales,
    string? IdTokenHint,
    string? LoginHint,
    string? AcrValues
);

public record AuthorizeResponse(
    bool Success,
    string? Code,
    string? State,
    string? RedirectUrl,
    string? ErrorMessage,
    string? Error,
    string? ErrorDescription
);

public class AuthorizeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("connect/authorize", HandleGet);
        app.MapPost("connect/authorize", HandlePost);
    }

    private static async Task<IResult> HandleGet(
        string client_id,
        string response_type,
        string? redirect_uri,
        string? scope,
        string? state,
        string? nonce,
        string? code_challenge,
        string? code_challenge_method,
        string? response_mode,
        string? prompt,
        string? max_age,
        string? ui_locales,
        string? id_token_hint,
        string? login_hint,
        string? acr_values,
        IOptionsMonitor<OAuthClients> oauthClientsMonitor,
        IOptionsMonitor<IdentityProviderConfiguration> configMonitor)
    {
        var request = new AuthorizeRequest(
            client_id, response_type, redirect_uri, scope, state, nonce,
            code_challenge, code_challenge_method, response_mode, prompt,
            max_age, ui_locales, id_token_hint, login_hint, acr_values);

        return await ProcessAuthorization(request, oauthClientsMonitor, configMonitor);
    }

    private static async Task<IResult> HandlePost(
        AuthorizeRequest request,
        IOptionsMonitor<OAuthClients> oauthClientsMonitor,
        IOptionsMonitor<IdentityProviderConfiguration> configMonitor)
    {
        return await ProcessAuthorization(request, oauthClientsMonitor, configMonitor);
    }

    private static Task<IResult> ProcessAuthorization(
        AuthorizeRequest request,
        IOptionsMonitor<OAuthClients> oauthClientsMonitor,
        IOptionsMonitor<IdentityProviderConfiguration> configMonitor)
    {
        // You can use either approach:
        
        // Approach 1: Use OAuthClients directly
        var oauthClients = oauthClientsMonitor.CurrentValue;
        
        // Approach 2: Use IdentityProviderConfiguration
        var config = configMonitor.CurrentValue;
        var oauthClientsFromConfig = config.OAuthClients;

        // Validate client_id exists in configuration
        if (!oauthClients.ContainsKey(request.ClientId))
        {
            var errorResponse = new AuthorizeResponse(
                Success: false,
                Code: null,
                State: request.State,
                RedirectUrl: null,
                ErrorMessage: "Invalid client_id",
                Error: "invalid_client",
                ErrorDescription: "The client identifier provided is invalid"
            );
            return Task.FromResult(Results.BadRequest(errorResponse));
        }

        var client = oauthClients[request.ClientId];
        
        // Validate redirect_uri
        if (!string.IsNullOrEmpty(request.RedirectUri) && 
            !string.Equals(request.RedirectUri, client.RedirectUri, StringComparison.OrdinalIgnoreCase))
        {
            var errorResponse = new AuthorizeResponse(
                Success: false,
                Code: null,
                State: request.State,
                RedirectUrl: null,
                ErrorMessage: "Invalid redirect_uri",
                Error: "invalid_request",
                ErrorDescription: "The redirect_uri provided does not match the registered redirect_uri"
            );
            return Task.FromResult(Results.BadRequest(errorResponse));
        }

        // Validate PKCE requirements
        if (client.RequirePkce && string.IsNullOrEmpty(request.CodeChallenge))
        {
            var errorResponse = new AuthorizeResponse(
                Success: false,
                Code: null,
                State: request.State,
                RedirectUrl: client.RedirectUri,
                ErrorMessage: "PKCE required",
                Error: "invalid_request",
                ErrorDescription: "Code challenge required for this client"
            );
            return Task.FromResult(Results.BadRequest(errorResponse));
        }

        // Validate requested scopes
        if (!string.IsNullOrEmpty(request.Scope))
        {
            var requestedScopes = request.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var unauthorizedScopes = requestedScopes.Except(client.Scopes);
            
            if (unauthorizedScopes.Any())
            {
                var errorResponse = new AuthorizeResponse(
                    Success: false,
                    Code: null,
                    State: request.State,
                    RedirectUrl: client.RedirectUri,
                    ErrorMessage: "Invalid scope",
                    Error: "invalid_scope",
                    ErrorDescription: $"Requested scopes are not allowed: {string.Join(", ", unauthorizedScopes)}"
                );
                return Task.FromResult(Results.BadRequest(errorResponse));
            }
        }

        // Implement OAuth2/OIDC authorization endpoint logic here
        // This should:
        // - Check if user is authenticated (redirect to login if not)
        // - Show consent page if needed
        // - Generate authorization code for code flow
        // - Handle PKCE validation for public clients
        // - Support different response types (code, token, id_token)
        // - Handle prompt parameter (none, login, consent, select_account)

        throw new NotImplementedException("Authorization flow implementation pending");
    }
}
