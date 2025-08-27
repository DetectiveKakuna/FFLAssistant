namespace FFLAssistant.Models.Interfaces;

public interface IDraftRankingsService
{
    Task<IList<DraftRanking>?> GetDraftRankingsAsync();
    Task RefreshDraftRankingsAsync();
    Task<bool> IsRefreshNeededAsync();
}