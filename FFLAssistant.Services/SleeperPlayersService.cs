using FFLAssistant.Models;
using FFLAssistant.Repositories.Interfaces;
using FFLAssistant.Models.Interfaces;
using Microsoft.Extensions.Logging;

namespace FFLAssistant.Services;

public class SleeperPlayersService(
    ISleeperPlayersRepository repository,
    ISleeperApiService sleeperApiService,
    ILogger<SleeperPlayersService> logger) : ISleeperPlayersService
{
    private readonly ISleeperPlayersRepository _repository = repository;
    private readonly ISleeperApiService _sleeperApiService = sleeperApiService;
    private readonly ILogger<SleeperPlayersService> _logger = logger;
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(24);

    public async Task<IList<Player>?> GetPlayersAsync()
    {
        try
        {
            if (await IsRefreshNeededAsync())
            {
                _logger.LogInformation("Sleeper players refresh needed, fetching from external source");
                await RefreshPlayersAsync();
            }

            return await _repository.GetPlayersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sleeper players");
            return null;
        }
    }

    private async Task RefreshPlayersAsync()
    {
        try
        {
            var externalData = await _sleeperApiService.FetchPlayersAsync();
            if (externalData != null && externalData.Count > 0)
            {
                await _repository.SavePlayersAsync(externalData);
                _logger.LogInformation("Sleeper players refreshed successfully with {Count} players", externalData.Count);
            }
            else
            {
                _logger.LogWarning("Failed to fetch sleeper players from external source or no data returned");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing sleeper players");
            throw;
        }
    }

    private async Task<bool> IsRefreshNeededAsync()
    {
        try
        {
            if (!await _repository.FileExistsAsync())
            {
                _logger.LogInformation("Sleeper players file does not exist, refresh needed");
                return true;
            }

            var lastModified = await _repository.GetFileLastModifiedAsync();
            if (lastModified == null)
            {
                _logger.LogInformation("Could not determine sleeper players file last modified time, refresh needed");
                return true;
            }

            var timeSinceUpdate = DateTime.Now - lastModified.Value;
            var refreshNeeded = timeSinceUpdate > RefreshInterval;

            _logger.LogInformation("Sleeper players file last modified: {LastModified}, Time since update: {TimeSince}, Refresh needed: {RefreshNeeded}",
                lastModified, timeSinceUpdate, refreshNeeded);

            return refreshNeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if sleeper players refresh is needed");
            return true; // Default to refresh on error
        }
    }
}