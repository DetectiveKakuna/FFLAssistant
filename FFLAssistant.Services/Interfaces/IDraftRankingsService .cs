using FFLAssistant.Models;

namespace FFLAssistant.Services.Interfaces;

public interface IDraftRankingsService
{
    Task<IList<DraftRanking>?> GetDraftRankingsAsync();
    Task RefreshDraftRankingsAsync();
    Task<bool> IsRefreshNeededAsync();
}