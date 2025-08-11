using FFLAssistant.Models;

namespace FFLAssistant.Services.Interfaces;

public interface ISleeperApiService
{
    Task<List<Player>?> FetchPlayersAsync();
}