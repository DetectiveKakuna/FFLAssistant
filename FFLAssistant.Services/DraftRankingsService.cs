using FFLAssistant.Models.Players;
using FFLAssistant.Repositories.Interfaces;
using FFLAssistant.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FFLAssistant.Services;

public class DraftRankingsService(
    IDraftRankingsRepository repository,
    IBorisChenService borisChenService,
    ILogger<DraftRankingsService> logger) : IDraftRankingsService
{
    private readonly IDraftRankingsRepository _repository = repository;
    private readonly IBorisChenService _borisChenService = borisChenService;
    private readonly ILogger<DraftRankingsService> _logger = logger;
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(15);

    public async Task<IList<DraftRanking>?> GetDraftRankingsAsync()
    {
        try
        {
            if (await IsRefreshNeededAsync())
            {
                _logger.LogInformation("Draft rankings refresh needed, fetching from external source");
                await RefreshDraftRankingsAsync();
            }

            return await _repository.GetDraftRankingsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting draft rankings");
            return null;
        }
    }

    public async Task RefreshDraftRankingsAsync()
    {
        try
        {
            var externalData = await _borisChenService.FetchDraftRankingsAsync();
            if (externalData != null)
            {
                await _repository.SaveDraftRankingsAsync(externalData);
                _logger.LogInformation("Draft rankings refreshed successfully");
            }
            else
            {
                _logger.LogWarning("Failed to fetch draft rankings from external source");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing draft rankings");
            throw;
        }
    }

    public async Task<bool> IsRefreshNeededAsync()
    {
        try
        {
            if (!await _repository.FileExistsAsync())
            {
                _logger.LogInformation("Draft rankings file does not exist, refresh needed");
                return true;
            }

            var lastModified = await _repository.GetFileLastModifiedAsync();
            if (lastModified == null)
            {
                _logger.LogInformation("Could not determine file last modified time, refresh needed");
                return true;
            }

            var timeSinceUpdate = DateTime.Now - lastModified.Value;
            var refreshNeeded = timeSinceUpdate > RefreshInterval;

            _logger.LogInformation("File last modified: {LastModified}, Time since update: {TimeSince}, Refresh needed: {RefreshNeeded}",
                lastModified, timeSinceUpdate, refreshNeeded);

            return refreshNeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if refresh is needed");
            return true; // Default to refresh on error
        }
    }
}