namespace FFLAssistant.Models.Interfaces;

public interface ISleeperPlayersService
{
    Task<IList<Player>?> GetPlayersAsync();
}