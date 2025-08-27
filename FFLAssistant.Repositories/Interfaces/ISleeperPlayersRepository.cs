using FFLAssistant.Models;

namespace FFLAssistant.Repositories.Interfaces;

public interface ISleeperPlayersRepository
{
    Task<IList<Player>?> GetPlayersAsync();
    Task SavePlayersAsync(IList<Player> players);
    Task<bool> FileExistsAsync();
    Task<DateTime?> GetFileLastModifiedAsync();
}