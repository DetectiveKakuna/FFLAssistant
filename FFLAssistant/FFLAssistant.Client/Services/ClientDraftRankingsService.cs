using FFLAssistant.Client.Pages;
using FFLAssistant.Models;
using FFLAssistant.Services.Interfaces;
using System.Net.Http.Json;

namespace FFLAssistant.Client.Services;

public class ClientDraftRankingsService(HttpClient httpClient) : IDraftRankingsService
{
    private readonly HttpClient _httpClient = httpClient;
    private IList<DraftRanking>? _cachedRankings;
    private DateTime? _lastLoaded;
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(15);

    public async Task<IList<DraftRanking>?> GetDraftRankingsAsync()
    {
        try
        {
            // Check if we have cached data that's still valid
            if (_cachedRankings != null && _lastLoaded.HasValue &&
                DateTime.Now - _lastLoaded.Value < CacheExpiry)
            {
                return _cachedRankings;
            }

            // Load from static file in wwwroot
            var rankings = await _httpClient.GetFromJsonAsync<IList<DraftRanking>?>($"api/draft/rankings");

            if (rankings != null)
            {
                _cachedRankings = rankings;
                _lastLoaded = DateTime.Now;
            }

            return rankings;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading draft rankings: {ex.Message}");
            return _cachedRankings; // Return cached data if available
        }
    }

    public async Task RefreshDraftRankingsAsync()
    {
        try
        {
            // Clear cache and reload
            _cachedRankings = null;
            _lastLoaded = null;

            await GetDraftRankingsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refreshing draft rankings: {ex.Message}");
            throw;
        }
    }

    public Task<bool> IsRefreshNeededAsync()
    {
        // In client-side, we can't check file modification time
        // So we use cache expiry time instead
        if (_lastLoaded == null)
            return Task.FromResult(true);

        var refreshNeeded = DateTime.Now - _lastLoaded.Value > CacheExpiry;
        return Task.FromResult(refreshNeeded);
    }
}