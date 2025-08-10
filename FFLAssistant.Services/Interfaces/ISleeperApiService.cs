using FFLAssistant.Models.Players;

namespace FFLAssistant.Services.Interfaces;

public interface ISleeperApiService
{
    Task<List<Player>?> FetchPlayersAsync();
}