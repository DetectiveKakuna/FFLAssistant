using FFLAssistant.Models;
using FFLAssistant.Services.Interfaces;
using System.Net.Http.Json;

namespace FFLAssistant.Client.Services;

public class ClientSleeperLiveDraftService(HttpClient httpClient) : ISleeperLiveDraftService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<DraftState?> GetDraftStateAsync(string draftId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<DraftState>($"api/draft/{draftId}");
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling draft API: {ex.Message}");
            return null;
        }
    }
}