using IdentityProvider.Server.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MinimalEndpoints.Abstractions;
using System.Security.Claims;

namespace IdentityProvider.Server.Api.Endpoints;

public class UserInfoEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("userinfo", Handle)
           .RequireAuthorization(policy =>
           {
               policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
               policy.RequireAuthenticatedUser();
           });
    }

    private static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        IUserRepository userRepository,
        HttpContext httpContext)
    {
        if (!principal.Identity?.IsAuthenticated == true)
        {
            return Results.Unauthorized();
        }

        // Extract claims from the authenticated user (JWT token)
        var sub = principal.FindFirst("sub")?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var scopes = principal.FindAll("scp").Select(c => c.Value).ToArray();
        var scopeString = principal.FindFirst("scope")?.Value;

        if (string.IsNullOrEmpty(sub))
        {
            return Results.BadRequest(new
            {
                error = "invalid_token",
                error_description = "Token does not contain a valid subject claim"
            });
        }

        // Fetch user data from repository
        var user = await userRepository.GetByIdAsync(sub);
        if (user == null)
        {
            return Results.BadRequest(new
            {
                error = "invalid_token",
                error_description = "User not found"
            });
        }

        // Build response based on requested scopes
        var userInfo = new Dictionary<string, object>
        {
            ["sub"] = sub
        };

        // Add profile information if profile scope is present
        if (scopes.Contains("profile") || scopeString?.Contains("profile") == true)
        {
            if (!string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName))
            {
                userInfo["name"] = $"{user.FirstName} {user.LastName}";
            }
            else if (!string.IsNullOrEmpty(user.FirstName))
            {
                userInfo["name"] = user.FirstName;
            }
            else if (!string.IsNullOrEmpty(user.Username))
            {
                userInfo["name"] = user.Username;
            }

            userInfo["preferred_username"] = user.Username;

            if (!string.IsNullOrEmpty(user.FirstName))
                userInfo["given_name"] = user.FirstName;

            if (!string.IsNullOrEmpty(user.LastName))
                userInfo["family_name"] = user.LastName;

            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                userInfo["picture"] = user.ProfilePictureUrl;
        }

        // Add email information if email scope is present
        if (scopes.Contains("email") || scopeString?.Contains("email") == true)
        {
            userInfo["email"] = user.Email;
            userInfo["email_verified"] = user.EmailVerified;
        }

        // Add phone information if phone scope is present
        if (scopes.Contains("phone") || scopeString?.Contains("phone") == true)
        {
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                userInfo["phone_number"] = user.PhoneNumber;
                userInfo["phone_number_verified"] = user.PhoneNumberVerified;
            }
        }

        // Add address information if address scope is present
        if (scopes.Contains("address") || scopeString?.Contains("address") == true)
        {
            if (user.Address != null)
            {
                var address = new Dictionary<string, string?>();

                if (!string.IsNullOrEmpty(user.Address.Formatted))
                    address["formatted"] = user.Address.Formatted;
                if (!string.IsNullOrEmpty(user.Address.StreetAddress))
                    address["street_address"] = user.Address.StreetAddress;
                if (!string.IsNullOrEmpty(user.Address.Locality))
                    address["locality"] = user.Address.Locality;
                if (!string.IsNullOrEmpty(user.Address.Region))
                    address["region"] = user.Address.Region;
                if (!string.IsNullOrEmpty(user.Address.PostalCode))
                    address["postal_code"] = user.Address.PostalCode;
                if (!string.IsNullOrEmpty(user.Address.Country))
                    address["country"] = user.Address.Country;

                if (address.Any())
                    userInfo["address"] = address;
            }
        }

        return Results.Ok(userInfo);
    }
}