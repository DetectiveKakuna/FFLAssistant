using FFLAssistant.Models;
using FFLAssistant.Models.Configurations;
using FFLAssistant.Models.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace FFLAssistant.Services;

public class BorisChenService(
    HttpClient httpClient,
    ISleeperPlayersService sleeperPlayersService,
    IOptions<DraftRankingsConfiguration> draftRankingsOptions,
    ILogger<BorisChenService> logger) : IBorisChenService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ISleeperPlayersService _sleeperPlayersService = sleeperPlayersService;
    private readonly DraftRankingsConfiguration _draftRankingsConfig = draftRankingsOptions.Value;
    private readonly ILogger<BorisChenService> _logger = logger;

    // Name mapping dictionary for players whose names don't match between Boris Chen CSV and Sleeper
    private static readonly Dictionary<string, string> NameMappingDictionary = new(StringComparer.OrdinalIgnoreCase)
    {
        // Boris Chen CSV Name -> Sleeper Player Name format (FirstName LastName)
        { "Marquise Brown", "Hollywood Brown" },
    };
    
    public async Task<IList<DraftRanking>?> FetchDraftRankingsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching draft rankings from external source: {CsvUrl}", _draftRankingsConfig.RankingsCsvPath);

            // Fetch CSV data from the configured URL
            var response = await _httpClient.GetAsync(_draftRankingsConfig.RankingsCsvPath);
            response.EnsureSuccessStatusCode();
            
            var csvContent = await response.Content.ReadAsStringAsync();
            
            // Get Sleeper players for name matching
            var sleeperPlayers = await _sleeperPlayersService.GetPlayersAsync();
            if (sleeperPlayers == null || !sleeperPlayers.Any())
            {
                _logger.LogWarning("No Sleeper players available for name matching");
                return null;
            }

            // Parse CSV and create draft rankings
            var result = ParseCsvAndCreateRankings(csvContent, sleeperPlayers);

            _logger.LogInformation("Successfully fetched {Count} draft rankings", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching draft rankings from external source");
            return null;
        }
    }

    private List<DraftRanking> ParseCsvAndCreateRankings(string csvContent, IList<Player> sleeperPlayers)
    {
        var result = new List<DraftRanking>();
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var unmatchedPlayers = new List<string>();

        // Skip header line
        for (int i = 1; i < lines.Length; i++)
        {
            try
            {
                var columns = ParseCsvLine(lines[i]);
                if (columns.Length < 8) continue; // Skip incomplete rows - CSV has 8 columns
                
                // CSV Format: "Rank","Player.Name","Tier","Position","Best.Rank","Worst.Rank","Avg.Rank","Std.Dev"
                var overallRankStr = columns[0].Trim();
                var playerName = columns[1].Trim();
                var tier = columns[2].Trim();
                var position = columns[3].Trim();
                var bestRankStr = columns[4].Trim();
                var worstRankStr = columns[5].Trim();
                var avgRankStr = columns[6].Trim();
                var stdDevStr = columns[7].Trim();
                
                // Parse ranking data
                if (!int.TryParse(overallRankStr, out var overallRank)) continue;
                if (!int.TryParse(bestRankStr, out var bestRank)) continue;
                if (!int.TryParse(worstRankStr, out var worstRank)) continue;
                if (!double.TryParse(avgRankStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var averageRank)) continue;
                if (!double.TryParse(stdDevStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var standardDeviation)) continue;

                // Find matching player
                var matchedPlayer = FindMatchingPlayer(playerName, sleeperPlayers);
                if (matchedPlayer == null)
                {
                    unmatchedPlayers.Add(playerName);
                    continue;
                }

                var draftRanking = new DraftRanking
                {
                    Player = matchedPlayer,
                    Tier = tier,
                    OverallRank = overallRank,
                    BestRank = bestRank,
                    WorstRank = worstRank,
                    AverageRank = averageRank,
                    StandardDeviation = standardDeviation
                };

                result.Add(draftRanking);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing CSV line {LineNumber}: {Line}", i + 1, lines[i]);
            }
        }

        return result;
    }

    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var inQuotes = false;
        var currentField = string.Empty;
        
        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField);
                currentField = string.Empty;
            }
            else
            {
                currentField += c;
            }
        }
        
        result.Add(currentField); // Add the last field
        return result.ToArray();
    }

    private Player? FindMatchingPlayer(string csvPlayerName, IList<Player> sleeperPlayers)
    {
        // Clean the CSV player name
        var cleanCsvName = CleanPlayerName(csvPlayerName);
        
        // First try the name mapping dictionary
        if (NameMappingDictionary.TryGetValue(cleanCsvName, out var mappedName))
        {
            var mappedMatch = sleeperPlayers.FirstOrDefault(p => 
                string.Equals(p.FullName, mappedName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals($"{p.FirstName} {p.LastName}", mappedName, StringComparison.OrdinalIgnoreCase));
            
            if (mappedMatch != null) return mappedMatch;
        }
        
        // Try exact match
        var exactMatch = sleeperPlayers.FirstOrDefault(p => 
            string.Equals(p.FullName, cleanCsvName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals($"{p.FirstName} {p.LastName}", cleanCsvName, StringComparison.OrdinalIgnoreCase));
        
        if (exactMatch != null) return exactMatch;
        
        return null;
    }

    private static string CleanPlayerName(string name)
    {
        // Remove common suffixes and prefixes
        var cleanName = name.Trim()
            .Replace(" Jr.", "")
            .Replace(" Sr.", "")
            .Replace(" III", "")
            .Replace(" II", "")
            .Replace(" IV", "")
            .Replace("*", "")
            .Trim();
        
        return cleanName;
    }
}