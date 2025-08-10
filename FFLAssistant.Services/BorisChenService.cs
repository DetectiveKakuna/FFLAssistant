using FFLAssistant.Models.Players;
using FFLAssistant.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace FFLAssistant.Services;

public class BorisChenService(
    HttpClient httpClient,
    ISleeperPlayersService sleeperPlayersService,
    ILogger<BorisChenService> logger) : IBorisChenService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ISleeperPlayersService _sleeperPlayersService = sleeperPlayersService;
    private readonly ILogger<BorisChenService> _logger = logger;
    
    public async Task<IList<DraftRanking>?> FetchDraftRankingsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching draft rankings from external source");

            // Fetch CSV data from the URL
            var response = await _httpClient.GetAsync("https://s3-us-west-1.amazonaws.com/fftiers/out/weekly-ALL-PPR.csv");
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
        
        // Skip header line
        for (int i = 1; i < lines.Length; i++)
        {
            try
            {
                var columns = ParseCsvLine(lines[i]);
                if (columns.Length < 7) continue; // Skip incomplete rows
                
                var playerName = columns[0].Trim();
                var tier = columns[1].Trim();
                
                // Parse ranking data
                if (!int.TryParse(columns[2], out var overallRank)) continue;
                if (!int.TryParse(columns[3], out var bestRank)) continue;
                if (!int.TryParse(columns[4], out var worstRank)) continue;
                if (!double.TryParse(columns[5], NumberStyles.Float, CultureInfo.InvariantCulture, out var averageRank)) continue;
                if (!double.TryParse(columns[6], NumberStyles.Float, CultureInfo.InvariantCulture, out var standardDeviation)) continue;

                // Find matching player
                var matchedPlayer = FindMatchingPlayer(playerName, sleeperPlayers);
                if (matchedPlayer == null)
                {
                    _logger.LogDebug("No matching player found for: {PlayerName}", playerName);
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
        
        // Try exact match first
        var exactMatch = sleeperPlayers.FirstOrDefault(p => 
            string.Equals(p.FullName, cleanCsvName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals($"{p.FirstName} {p.LastName}", cleanCsvName, StringComparison.OrdinalIgnoreCase));
        
        if (exactMatch != null) return exactMatch;
        
        // Try partial matches
        var partialMatch = sleeperPlayers.FirstOrDefault(p =>
            cleanCsvName.Contains(p.LastName, StringComparison.OrdinalIgnoreCase) &&
            cleanCsvName.Contains(p.FirstName, StringComparison.OrdinalIgnoreCase));
        
        if (partialMatch != null) return partialMatch;
        
        // Try last name only match as fallback
        var lastNameMatch = sleeperPlayers.FirstOrDefault(p =>
            string.Equals(p.LastName, cleanCsvName.Split(' ').LastOrDefault(), StringComparison.OrdinalIgnoreCase));
        
        return lastNameMatch;
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