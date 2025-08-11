using FFLAssistant.Models;

namespace FFLAssistant.Services.Interfaces;

public interface ISleeperLiveDraftService
{
    Task<DraftState?> GetDraftStateAsync(string draftId);
}