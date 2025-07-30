using IdentityProvider.Configuration.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MinimalEndpoints.Abstractions;

namespace IdentityProvider.Api.Endpoints.Auth.AuthorizeEndpoint;

internal class AuthorizeGetHandler : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("connect/authorize", Handle);
    }

    private static Task<IResult> Handle(
        [AsParameters] AuthorizeRequest request,
        IOptionsMonitor<OAuthClients> oauthClientsMonitor,
        HttpContext httpContext)
    {
        // Validate the authorization request
        var validationResult = AuthorizeRequestValidator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Task.FromResult(Results.BadRequest(new
            {
                error = validationResult.Error,
                error_description = validationResult.ErrorDescription,
                state = request.State
            }));
        }

        // Get OAuth client configuration
        var clients = oauthClientsMonitor.CurrentValue;

        if (!clients.TryGetValue(request.ClientId, out var client))
        {
            return Task.FromResult(Results.BadRequest(new
            {
                error = "invalid_client",
                error_description = "The client identifier provided is invalid",
                state = request.State
            }));
        }

        // Validate redirect URI
        if (!string.IsNullOrEmpty(request.RedirectUri) &&
            !string.Equals(request.RedirectUri, client.RedirectUri, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(Results.BadRequest(new
            {
                error = "invalid_request",
                error_description = "The redirect_uri provided does not match the registered redirect_uri",
                state = request.State
            }));
        }

        // Validate PKCE requirements
        if (client.RequirePkce && !request.UsesPkce())
        {
            var redirectUri = request.RedirectUri ?? client.RedirectUri;
            return Task.FromResult(Results.Redirect($"{redirectUri}?error=invalid_request&error_description=PKCE+required&state={request.State}"));
        }

        // Validate requested scopes
        var requestedScopes = request.GetScopes();
        var unauthorizedScopes = requestedScopes.Except(client.Scopes).ToArray();

        if (unauthorizedScopes.Length > 0)
        {
            var redirectUri = request.RedirectUri ?? client.RedirectUri;
            var errorDescription = $"Requested+scopes+are+not+allowed:+{string.Join("+", unauthorizedScopes)}";
            return Task.FromResult(Results.Redirect($"{redirectUri}?error=invalid_scope&error_description={errorDescription}&state={request.State}"));
        }

        // Check if the user is authenticated
        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
        {
            // Check prompt parameter
            var prompts = request.GetPrompts();
            if (prompts.Contains("none", StringComparer.OrdinalIgnoreCase))
            {
                // If prompt=none and user not authenticated, return error
                var redirectUri = request.RedirectUri ?? client.RedirectUri;
                return Task.FromResult(Results.Redirect(
                    $"{redirectUri}?error=login_required&error_description=User+is+not+authenticated&state={request.State}"));
            }

            // Build login URL with returnUrl pointing back to this authorize endpoint
            var returnUrl = $"/connect/authorize?client_id={Uri.EscapeDataString(request.ClientId)}&response_type={Uri.EscapeDataString(request.ResponseType)}";
            if (!string.IsNullOrEmpty(request.RedirectUri))
                returnUrl += $"&redirect_uri={Uri.EscapeDataString(request.RedirectUri)}";
            if (!string.IsNullOrEmpty(request.Scope))
                returnUrl += $"&scope={Uri.EscapeDataString(request.Scope)}";
            if (!string.IsNullOrEmpty(request.State))
                returnUrl += $"&state={Uri.EscapeDataString(request.State)}";
            if (!string.IsNullOrEmpty(request.Nonce))
                returnUrl += $"&nonce={Uri.EscapeDataString(request.Nonce)}";
            if (!string.IsNullOrEmpty(request.CodeChallenge))
            {
                returnUrl += $"&code_challenge={Uri.EscapeDataString(request.CodeChallenge)}";
                if (!string.IsNullOrEmpty(request.CodeChallengeMethod))
                    returnUrl += $"&code_challenge_method={Uri.EscapeDataString(request.CodeChallengeMethod)}";
            }

            // Redirect to login page
            return Task.FromResult(Results.Redirect($"/account/login?returnUrl={Uri.EscapeDataString(returnUrl)}"));
        }

        // At this point, the user is authenticated
        // Handle the prompt parameter if specified
        var promptValues = request.GetPrompts();
        
        // Handle "login" prompt - force re-authentication
        if (promptValues.Contains("login", StringComparer.OrdinalIgnoreCase))
        {
            // In a real implementation, we would check authentication time and force re-auth if needed
            // For demo purposes, we'll just continue
        }
        
        // Handle "select_account" prompt - allow user to select a different account
        if (promptValues.Contains("select_account", StringComparer.OrdinalIgnoreCase))
        {
            // For demo purposes, we'll just continue
            // In a real implementation, we would redirect to an account selection page
        }
        
        // Handle "consent" prompt - show consent screen
        if (promptValues.Contains("consent", StringComparer.OrdinalIgnoreCase))
        {
            // In a real implementation, we would check if consent was already given and redirect to consent page if not
            // For demo purposes, we'll just continue
        }

        // TODO: Implement the rest of the authorization flow:
        // 1. Generate authorization code for code flow
        // 2. Handle different response types (code, id_token, token, hybrid)
        // 3. Support different response modes (query, fragment, form_post)

        // For now, return a message indicating authentication success
        return Task.FromResult(Results.Ok(new
        {
            message = "User is authenticated!",
            username = httpContext.User.Identity.Name,
            flow = request.IsAuthorizationCodeFlow() ? "authorization_code" : 
                   request.IsImplicitFlow() ? "implicit" : "hybrid",
            // Add more information as needed
        }));
    }
}
