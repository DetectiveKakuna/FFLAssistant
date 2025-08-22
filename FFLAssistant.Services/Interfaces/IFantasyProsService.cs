using FFLAssistant.Models;

namespace FFLAssistant.Services.Interfaces;

public interface IFantasyProsService
{
    Task<PlayerNotes> GetPlayerNotesAsync(string firstName, string lastName);
    Task<Dictionary<string, string>> GetDraftProjectionsAsync(string firstName, string lastName);
    Task<Dictionary<string, string>> GetPlayerRankingsAsync(string firstName, string lastName);
}