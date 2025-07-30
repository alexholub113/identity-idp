using MinimalEndpoints.Abstractions;
using System.Security.Claims;

namespace IdentityProvider.Api.Endpoints.Account;

public class ProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("account/profile", Handle)
           .RequireAuthorization(); // This endpoint requires authentication
    }

    private static IResult Handle(ClaimsPrincipal user)
    {
        // Get claims from the authenticated user
        var claims = user.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();

        // Return HTML page with user information
        var html = @"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset=""utf-8"" />
                <title>User Profile</title>
                <style>
                    body { font-family: Arial, sans-serif; margin: 20px; }
                    .profile-container { max-width: 800px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }
                    h1 { color: #0066cc; }
                    table { width: 100%; border-collapse: collapse; }
                    th, td { padding: 8px; text-align: left; border-bottom: 1px solid #ddd; }
                    th { background-color: #f2f2f2; }
                    .logout-link { margin-top: 20px; display: block; }
                </style>
            </head>
            <body>
                <div class=""profile-container"">
                    <h1>User Profile</h1>
                    <p>You are successfully authenticated as: <strong>" + user.Identity?.Name + @"</strong></p>
                    
                    <h2>Claims</h2>
                    <table>
                        <tr>
                            <th>Claim Type</th>
                            <th>Value</th>
                        </tr>";

        foreach (var claim in user.Claims)
        {
            html += $@"
                        <tr>
                            <td>{claim.Type}</td>
                            <td>{claim.Value}</td>
                        </tr>";
        }

        html += @"
                    </table>
                    
                    <a href=""/account/logout"" class=""logout-link"">Logout</a>
                </div>
            </body>
            </html>";

        return Results.Content(html, "text/html");
    }
}