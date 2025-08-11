using Blazored.LocalStorage;
using FFLAssistant.Client.Cache.Services.Interfaces;
using FFLAssistant.Models;
using Microsoft.JSInterop;

namespace FFLAssistant.Client.Cache.Services;

public class PlayersCache(ILocalStorageService localStorage, ILogger<PlayersCache> logger) : IPlayersCache
{
    private readonly ILocalStorageService _localStorage = localStorage;
    private readonly ILogger<PlayersCache> _logger = logger;

    private const string PLAYERS_CACHE_KEY = "players_data";
    private const string PLAYERS_TIMESTAMP_KEY = "players_timestamp";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(30);

    public async Task<List<Player>?> GetCachedPlayersAsync()
    {
        try
        {
            return await RetryLocalStorageOperationAsync(async () =>
            {
                var cachedPlayers = await _localStorage.GetItemAsync<List<Player>?>(PLAYERS_CACHE_KEY);
                return cachedPlayers?.Any() == true ? cachedPlayers : null;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading players from cache");
            return null;
        }
    }

    public async Task SavePlayersToCache(List<Player> players)
    {
        try
        {
            await RetryLocalStorageOperationAsync(async () =>
            {
                await _localStorage.SetItemAsync(PLAYERS_CACHE_KEY, players);
                await _localStorage.SetItemAsync(PLAYERS_TIMESTAMP_KEY, DateTime.Now);
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving players to cache");
            throw;
        }
    }

    public async Task<bool> IsCacheStaleAsync()
    {
        try
        {
            var timestamp = await _localStorage.GetItemAsync<DateTime?>(PLAYERS_TIMESTAMP_KEY);
            return !timestamp.HasValue || DateTime.Now - timestamp.Value > CACHE_DURATION;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache staleness");
            return true;
        }
    }

    public async Task<DateTime?> GetCacheTimestampAsync()
    {
        try
        {
            return await _localStorage.GetItemAsync<DateTime?>(PLAYERS_TIMESTAMP_KEY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache timestamp");
            return null;
        }
    }

    private static async Task<T> RetryLocalStorageOperationAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
            try
            {
                return await operation();
            }
            catch (JSException) when (i < maxRetries - 1)
            {
                await Task.Delay(100 * (i + 1));
            }

        return await operation(); // Final attempt without catching
    }
}