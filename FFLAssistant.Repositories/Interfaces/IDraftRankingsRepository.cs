using FFLAssistant.Models.Players;

namespace FFLAssistant.Repositories.Interfaces;
public interface IDraftRankingsRepository
{
    Task<IList<DraftRanking>?> GetDraftRankingsAsync();
    Task SaveDraftRankingsAsync(IList<DraftRanking> rankings);
    Task<bool> FileExistsAsync();
    Task<DateTime?> GetFileLastModifiedAsync();
}