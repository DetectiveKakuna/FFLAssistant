using FFLAssistant.Models;

namespace FFLAssistant.Services.Interfaces;

public interface ISleeperPlayersService
{
    Task<IList<Player>?> GetPlayersAsync();
}