using IdentityProvider.Server.Configuration;
using IdentityProvider.Server.Configuration.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityProvider.Server.Configuration.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Identity Provider configuration to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddIdentityProviderConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure the full IdentityProviderConfiguration
        services.Configure<IdentityProviderConfiguration>(
            configuration.GetSection("IdentityProvider"));

        // Configure OAuthClients directly for easier injection
        services.Configure<OAuthClients>(
            configuration.GetSection("IdentityProvider:OAuthClients"));

        return services;
    }
}