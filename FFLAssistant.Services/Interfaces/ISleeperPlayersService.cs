using FFLAssistant.Models.Players;

namespace FFLAssistant.Services.Interfaces;

public interface ISleeperPlayersService
{
    Task<IList<Player>?> GetPlayersAsync();
    Task RefreshPlayersAsync();
    Task<bool> IsRefreshNeededAsync();
}