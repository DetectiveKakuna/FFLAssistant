namespace FFLAssistant.Models.Interfaces;

public interface IBorisChenService
{
    Task<IList<DraftRanking>?> FetchDraftRankingsAsync();
}