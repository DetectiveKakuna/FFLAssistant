using FFLAssistant.Models;

namespace FFLAssistant.Services.Interfaces;

public interface IBorisChenService
{
    Task<IList<DraftRanking>?> FetchDraftRankingsAsync();
}