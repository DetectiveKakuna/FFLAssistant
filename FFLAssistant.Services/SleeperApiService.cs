using FFLAssistant.Models.Configurations;
using FFLAssistant.Models.Dtos;
using FFLAssistant.Models.Enums;
using FFLAssistant.Models.Players;
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

            var players = sleeperPlayers.Values
                .Where(player => player.Active == true && player.Status == "Active")
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
            Id = int.TryParse(sleeperPlayer.PlayerId, out var id) ? id : 0,
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