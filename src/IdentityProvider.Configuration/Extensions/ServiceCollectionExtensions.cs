using IdentityProvider.Configuration.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityProvider.Configuration.Extensions;

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
            configuration.GetSection(IdentityProviderConfiguration.SectionName));

        // Configure OAuthClients directly for easier injection
        services.Configure<OAuthClients>(
            configuration.GetSection($"{IdentityProviderConfiguration.SectionName}:OAuthClients"));

        services.Configure<FrontendUrls>(
            configuration.GetSection($"{IdentityProviderConfiguration.SectionName}:FrontendUrls"));

        services.Configure<CorsConfiguration>(
            configuration.GetSection($"{IdentityProviderConfiguration.SectionName}:Cors"));

        services.Configure<JwtConfiguration>(
            configuration.GetSection($"{IdentityProviderConfiguration.SectionName}:Jwt"));

        return services;
    }
}