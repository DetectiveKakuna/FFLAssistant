using FFLAssistant.Models.Players;

namespace FFLAssistant.Client.Cache.Services.Interfaces;

public interface IPlayersCache
{
    Task<List<Player>?> GetCachedPlayersAsync();
    Task SavePlayersToCache(List<Player> players);
    Task<bool> IsCacheStaleAsync();
    Task<DateTime?> GetCacheTimestampAsync();
}