using Blazored.LocalStorage;
using FFLAssistant.Client.Cache.Models;
using FFLAssistant.Client.Cache.Services.Interfaces;

namespace FFLAssistant.Client.Cache.Services;

public class PlayersFiltersCache : IPlayersFiltersCache
{
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<PlayersFiltersCache> _logger;

    private const string PLAYERS_FILTER_KEY = "players_filter";

    public PlayersFiltersCache(ILocalStorageService localStorage, ILogger<PlayersFiltersCache> logger)
    {
        _localStorage = localStorage;
        _logger = logger;
    }

    public async Task<PlayersFilterState?> LoadStateAsync()
    {
        try
        {
            return await _localStorage.GetItemAsync<PlayersFilterState?>(PLAYERS_FILTER_KEY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading players page state");
            return null;
        }
    }

    public async Task SaveStateAsync(PlayersFilterState state)
    {
        try
        {
            await _localStorage.SetItemAsync(PLAYERS_FILTER_KEY, state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving players page state");
        }
    }
}