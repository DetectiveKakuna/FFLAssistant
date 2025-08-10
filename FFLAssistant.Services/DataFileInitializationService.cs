using FFLAssistant.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FFLAssistant.Services;

public class DataFileInitializationService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<DataFileInitializationService> logger)
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<DataFileInitializationService> _logger = logger;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting Sleeper players initialization");

            using var scope = _serviceScopeFactory.CreateScope();
            var sleeperPlayersService = scope.ServiceProvider.GetRequiredService<ISleeperPlayersService>();

            await sleeperPlayersService.GetPlayersAsync();

            _logger.LogInformation("Sleeper players initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Sleeper players data on startup");
            throw; // Re-throw to prevent application startup
        }
    }
}