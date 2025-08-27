using FFLAssistant.Models;
using FFLAssistant.Models.Interfaces;
using System.Net.Http.Json;

namespace FFLAssistant.Client.Services;

public class ClientSleeperPlayersService(HttpClient httpClient) : ISleeperPlayersService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<IList<Player>> GetPlayersAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<IList<Player>>("api/players");
            return response ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling players API: {ex.Message}");
            return [];
        }
    }
}