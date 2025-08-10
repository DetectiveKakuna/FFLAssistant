using FFLAssistant.Models.Players;

namespace FFLAssistant.Services.Interfaces;

public interface IBorisChenService
{
    Task<IList<DraftRanking>?> FetchDraftRankingsAsync();
}