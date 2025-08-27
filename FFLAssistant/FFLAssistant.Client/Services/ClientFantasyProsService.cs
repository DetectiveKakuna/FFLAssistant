using FFLAssistant.Models;
using FFLAssistant.Models.Interfaces;
using System.Net.Http.Json;

namespace FFLAssistant.Client.Services;

public class ClientFantasyProsService(HttpClient httpClient) : IFantasyProsService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<PlayerNotes> GetPlayerNotesAsync(string firstName, string lastName)
    {
        return await _httpClient.GetFromJsonAsync<PlayerNotes>(
            $"api/Players/notes?firstName={Uri.EscapeDataString(firstName)}&lastName={Uri.EscapeDataString(lastName)}")
            ?? new();
    }

    public async Task<Dictionary<string, string>> GetPlayerRankingsAsync(string firstName, string lastName)
    {
        return await _httpClient.GetFromJsonAsync<Dictionary<string, string>>(
            $"api/Players/rankings?firstName={Uri.EscapeDataString(firstName)}&lastName={Uri.EscapeDataString(lastName)}")
            ?? [];
    }

    public async Task<Dictionary<string, string>> GetDraftProjectionsAsync(string firstName, string lastName)
    {
        return await _httpClient.GetFromJsonAsync<Dictionary<string, string>>(
            $"api/Draft/projections?firstName={Uri.EscapeDataString(firstName)}&lastName={Uri.EscapeDataString(lastName)}")
            ?? [];
    }
}