namespace FFLAssistant.Models.Interfaces;

public interface ISleeperApiService
{
    Task<List<Player>?> FetchPlayersAsync();
}