using FFLAssistant.Models;
using FFLAssistant.Models.Configurations;
using FFLAssistant.Models.Dtos;
using FFLAssistant.Models.Enums;
using FFLAssistant.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FFLAssistant.Services;

public class SleeperApiService(
    IOptions<SleeperConfiguration> config,
    HttpClient httpClient,
    ILogger<SleeperApiService> logger) : ISleeperApiService
{
    private readonly SleeperConfiguration _config = config.Value;
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<SleeperApiService> _logger = logger;

    public async Task<List<Player>?> FetchPlayersAsync()
    {
        try
        {
            _logger.LogInformation("Fetching players from Sleeper API");
            
            var response = await _httpClient.GetAsync(_config.Url_GetPlayers);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var sleeperPlayers = JsonSerializer.Deserialize<Dictionary<string, SleeperPlayerDto>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (sleeperPlayers == null)
            {
                _logger.LogWarning("Failed to deserialize players from Sleeper API");
                return null;
            }

            // Fix defense dictionary keys by updating null keys with team-based IDs
            var fixedSleeperPlayers = new Dictionary<string, SleeperPlayerDto>();
            foreach (var kvp in sleeperPlayers)
            {
                var player = kvp.Value;
                var key = kvp.Key;

                // If this is a defense and the key is null/empty, generate a synthetic ID
                if ((string.IsNullOrEmpty(key) || key == "null") &&
                    player.FantasyPositions?.Contains("DEF") == true &&
                    !string.IsNullOrEmpty(player.Team))
                {
                    // Use team abbreviation with "DEF" suffix as synthetic ID
                    key = $"{player.Team}_DEF";
                    player.PlayerId = key; // Also update the player's ID
                }

                if (!string.IsNullOrEmpty(key) && key != "null")
                {
                    fixedSleeperPlayers[key] = player;
                }
            }
            
            var players = sleeperPlayers.Values
                .Where(player => player.Active == true)
                .Select(MapToPlayer)
                .ToList();

            _logger.LogInformation("Successfully fetched and mapped {Count} players from Sleeper API", players.Count);
            return players;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching players from Sleeper API");
            return null;
        }
    }

    private static Player MapToPlayer(SleeperPlayerDto sleeperPlayer)
    {
        var player = new Player
        {
            Id = sleeperPlayer.PlayerId ?? string.Empty,
            FirstName = sleeperPlayer.FirstName ?? string.Empty,
            LastName = sleeperPlayer.LastName ?? string.Empty,
            Age = sleeperPlayer.Age ?? 0,
            YearsExperience = sleeperPlayer.YearsExperience ?? 0,
            Positions = [],
            DepthChartPosition = sleeperPlayer.DepthChartOrder,
        };

        // Map positions
        if (sleeperPlayer.FantasyPositions != null)
        {
            foreach (var fantasyPos in sleeperPlayer.FantasyPositions)
            {
                if (Enum.TryParse<Position>(fantasyPos, true, out var fantasyPosition) && 
                    !player.Positions.Contains(fantasyPosition))
                {
                    player.Positions.Add(fantasyPosition);
                }
            }
        }

        // Map team using team_abbr
        if (!string.IsNullOrEmpty(sleeperPlayer.Team) && 
            Enum.TryParse<Team>(sleeperPlayer.Team, true, out var team))
        {
            player.Team = team;
        }

        // Map injury status using team_abbr
        if (!string.IsNullOrEmpty(sleeperPlayer.InjuryStatus) &&
            Enum.TryParse<InjuryStatus>(sleeperPlayer.InjuryStatus, true, out var iStatus))
        {
            player.InjuryStatus = iStatus;
        }

        return player;
    }
}