using System;
using MinimalEndpoints.Abstractions;

namespace IdentityProvider.Api.Endpoints.Discovery;

public class WellKnownEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(".well-known/openid-configuration", Handle);
    }

    private static IResult Handle()
    {
        // Implement your discovery logic here
        throw new NotImplementedException();
    }
}
