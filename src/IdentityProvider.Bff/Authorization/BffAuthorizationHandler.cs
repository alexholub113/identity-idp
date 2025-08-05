using Microsoft.AspNetCore.Authorization;

namespace IdentityProvider.Bff.Authorization;

public class BffAuthorizationHandler : AuthorizationHandler<IAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        // TODO: Implement authorization logic
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
