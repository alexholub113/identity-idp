namespace IdentityProvider.Server.Api.Services;

/// <summary>
/// Background service to periodically clean up expired authorization codes
/// </summary>
public class AuthorizationCodeCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuthorizationCodeCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5); // Clean up every 5 minutes

    public AuthorizationCodeCleanupService(
        IServiceProvider serviceProvider,
        ILogger<AuthorizationCodeCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Authorization code cleanup service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IAuthorizationCodeRepository>();

                await repository.CleanupExpiredAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authorization code cleanup");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }
    }
}
