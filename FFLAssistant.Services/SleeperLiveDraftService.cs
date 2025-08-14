using FFLAssistant.Models;
using FFLAssistant.Models.Configurations;
using FFLAssistant.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace FFLAssistant.Services;

public partial class SleeperLiveDraftService(ILogger<SleeperLiveDraftService> logger, IOptions<SleeperConfiguration> config) : ISleeperLiveDraftService
{
    private readonly ILogger<SleeperLiveDraftService> _logger = logger;
    private readonly SleeperConfiguration _config = config.Value;

    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private bool _disposed = false;

    public async Task<DraftState?> GetDraftStateAsync(string draftId)
    {
        try
        {
            await InitializeBrowserAsync();

            var page = await _browser!.NewPageAsync();
            await page.GotoAsync(Path.Combine(_config.DraftBaseUrl, draftId));

            // Wait for the draft board to load
            await page.WaitForSelectorAsync(".draft-layout-container", new PageWaitForSelectorOptions
            {
                Timeout = 30000
            });

            var draftState = new DraftState
            {
                Teams = await GetTeamsAsync(page),
                CurrentPick = await GetCurrentPickAsync(page),
                TotalRounds = await GetTotalRoundsAsync(page),
                Picks = await GetPicksAsync(page)
            };
            await page.CloseAsync();

            _logger.LogInformation("Successfully scraped draft state for league");
            return draftState;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping draft state from id: {Url}", draftId);
            return null;
        }
    }

    private async Task InitializeBrowserAsync()
    {
        if (_browser != null) return;

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args = ["--no-sandbox", "--disable-dev-shm-usage"]
        });
    }

    private async Task<List<DraftTeam>> GetTeamsAsync(IPage page)
    {
        var teams = new List<DraftTeam>();

        try
        {
            // Find all team columns
            var teamColumns = await page.QuerySelectorAllAsync("div.team-column");

            _logger.LogDebug("Found {Count} team columns", teamColumns.Count);

            for (int i = 0; i < teamColumns.Count; i++)
            {
                try
                {
                    // Find the header text within this team column
                    var headerElement = await teamColumns[i].QuerySelectorAsync("h1.header-text");

                    if (headerElement == null)
                    {
                        _logger.LogWarning("No header-text element found in team column {Position}", i + 1);
                        continue;
                    }

                    // Get the text content of the header
                    var headerText = await headerElement.TextContentAsync();

                    if (string.IsNullOrEmpty(headerText))
                    {
                        _logger.LogWarning("Empty header text in team column {Position}", i + 1);
                        continue;
                    }

                    var teamName = headerText.Trim();
                    var isMyTeam = teamName.Equals(_config.UserName, StringComparison.OrdinalIgnoreCase);

                    var team = new DraftTeam
                    {
                        Position = i + 1,
                        TeamName = teamName,
                        IsMyTeam = isMyTeam
                    };

                    teams.Add(team);

                    if (isMyTeam)
                    {
                        _logger.LogDebug("Found my team '{TeamName}' at position {Position}", teamName, i + 1);
                    }
                    else
                    {
                        _logger.LogDebug("Found team '{TeamName}' at position {Position}", teamName, i + 1);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing team column at position {Position}", i + 1);
                }
            }

            _logger.LogInformation("Successfully extracted {Count} teams", teams.Count);
            return teams;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting teams");
            return teams;
        }
    }

    private async Task<int> GetTotalRoundsAsync(IPage page)
    {
        try
        {
            // Count the number of .team-column divs
            var randomTeamColumn = await page.QuerySelectorAsync("div.team-column");

            if (randomTeamColumn == null)
            {
                _logger.LogWarning("No team column found to count rounds");
                return 0;
            }

            // Find all rows in the first team column
            var rowCount = await randomTeamColumn.QuerySelectorAllAsync("div.cell-container");

            if (rowCount.Count == 0)
            {
                _logger.LogWarning("No rows found in the team column to count rounds");
                return 0;
            }

            _logger.LogDebug("Found {TotalTeams} team columns", rowCount.Count);

            return rowCount.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting team columns");
            return 0;
        }
    }

    private async Task<int> GetCurrentPickAsync(IPage page)
    {
        try
        {
            // Find the div with .is-active class
            var activeElement = await page.QuerySelectorAsync("div.is-active");
            
            if (activeElement == null)
            {
                _logger.LogWarning("No active pick element found");
                return 0;
            }

            // Get the id attribute
            var id = await activeElement.GetAttributeAsync("id");
            
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Active pick element has no id attribute");
                return 0;
            }

            // Extract pick number from id format: draft-cell-<pickNumber>
            var match = DraftCellIdRegex().Match(id);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int pickNumber))
            {
                _logger.LogDebug("Current pick extracted: {PickNumber}", pickNumber);
                return pickNumber;
            }

            _logger.LogWarning("Could not parse pick number from id: {Id}", id);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting current pick");
            return 0;
        }
    }

    private async Task<List<DraftPick>> GetPicksAsync(IPage page)
    {
        var picks = new List<DraftPick>();

        try
        {
            // Find all divs with .drafted class
            var draftedElements = await page.QuerySelectorAllAsync("div.drafted");
            
            _logger.LogDebug("Found {Count} drafted elements", draftedElements.Count);

            foreach (var element in draftedElements)
            {
                try
                {
                    // Get the id attribute to extract pick number
                    var id = await element.GetAttributeAsync("id");
                    
                    if (string.IsNullOrEmpty(id))
                    {
                        _logger.LogWarning("Drafted element has no id attribute");
                        continue;
                    }

                    // Extract pick number from id format: draft-cell-<pickNumber>
                    var pickMatch = DraftCellIdRegex().Match(id);
                    if (!pickMatch.Success || !int.TryParse(pickMatch.Groups[1].Value, out int pickNumber))
                    {
                        _logger.LogWarning("Could not parse pick number from id: {Id}", id);
                        continue;
                    }

                    // Find the player avatar element (either img or div) within this element
                    var avatarElement = await element.QuerySelectorAsync(".avatar-player");
                    
                    if (avatarElement == null)
                    {
                        _logger.LogWarning("No avatar-player element found for pick {PickNumber}", pickNumber);
                        continue;
                    }

                    // Get the aria-label attribute
                    var ariaLabel = await avatarElement.GetAttributeAsync("aria-label");
                    
                    if (string.IsNullOrEmpty(ariaLabel))
                    {
                        _logger.LogWarning("Avatar element has no aria-label for pick {PickNumber}", pickNumber);
                        continue;
                    }

                    // Extract player ID from aria-label format: "nfl Player <playerId>"
                    var playerMatch = PlayerIdFromAriaLabelRegex().Match(ariaLabel);
                    if (!playerMatch.Success)
                    {
                        _logger.LogWarning("Could not parse player ID from aria-label: {AriaLabel}", ariaLabel);
                        continue;
                    }

                    var playerId = playerMatch.Groups[1].Value.Trim();

                    var pick = new DraftPick
                    {
                        PickNumber = pickNumber,
                        PlayerId = playerId
                    };

                    picks.Add(pick);
                    _logger.LogDebug("Extracted pick {PickNumber} with player {PlayerId}", pickNumber, playerId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing drafted element");
                }
            }

            picks = [.. picks.OrderBy(p => p.PickNumber)];

            _logger.LogInformation("Successfully extracted {Count} draft picks", picks.Count);
            return picks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting draft picks");
            return picks;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _browser?.DisposeAsync();
        _playwright?.Dispose();
        _disposed = true;
    }

    [GeneratedRegex(@"draft-cell-(\d+)")]
    private static partial Regex DraftCellIdRegex();
    [GeneratedRegex(@"nfl Player (.+)$")]
    private static partial Regex PlayerIdFromAriaLabelRegex();
}