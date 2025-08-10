using FFLAssistant.Client.Cache.Models;

namespace FFLAssistant.Client.Cache.Services.Interfaces;

public interface IPlayersFiltersCache
{
    Task<PlayersFilterState?> LoadStateAsync();
    Task SaveStateAsync(PlayersFilterState state);
}