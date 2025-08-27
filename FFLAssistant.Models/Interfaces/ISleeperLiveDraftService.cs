namespace FFLAssistant.Models.Interfaces;

public interface ISleeperLiveDraftService
{
    Task<DraftState?> GetDraftStateAsync(string draftId);
}