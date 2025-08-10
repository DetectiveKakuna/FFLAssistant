using FFLAssistant.Client.Cache.Models;
using FFLAssistant.Client.Cache.Services.Interfaces;
using FFLAssistant.Models.Enums;
using FFLAssistant.Models.Players;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using Position = FFLAssistant.Models.Enums.Position;

namespace FFLAssistant.Client.Pages;

public partial class Players : ComponentBase
{
    #region Dependency Injection

    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IPlayersCache CacheService { get; set; } = default!;
    [Inject] private IPlayersFiltersCache FiltersCacheService { get; set; } = default!;
    [Inject] private ILogger<Players> Logger { get; set; } = default!;

    #endregion

    #region Private Fields

    private List<Player> allPlayers = [];
    private List<Player> players = [];
    private string searchText = string.Empty;
    private Team? selectedTeam;
    private Position? selectedPosition;
    private string lastUpdated = string.Empty;
    private MudDataGrid<Player>? dataGrid;
    private GridState<Player>? currentGridState;

    private readonly IEnumerable<Team> availableTeams = Enum.GetValues<Team>().OrderBy(t => t.ToString());
    private readonly IEnumerable<Position> availablePositions = Enum.GetValues<Position>().OrderBy(p => p.ToString());

    #endregion

    #region Component Lifecycle

    /// <summary>
    /// Initializes the component by loading cached filter state and player data.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        try
        {
            await LoadPlayersFilterCacheAsync();
            await LoadPlayersAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error initializing Players component");
        }
    }

    #endregion

    #region Filter State Management

    /// <summary>
    /// Loads previously saved filter state from cache to restore user preferences.
    /// </summary>
    private async Task LoadPlayersFilterCacheAsync()
    {
        try
        {
            var savedFilters = await FiltersCacheService.LoadStateAsync();
            if (savedFilters is not null)
            {
                searchText = savedFilters.SearchText ?? string.Empty;
                selectedTeam = savedFilters.SelectedTeam;
                selectedPosition = savedFilters.SelectedPosition;
                currentGridState = savedFilters.GridState;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading players state");
        }
    }

    /// <summary>
    /// Saves current filter state to cache for persistence across sessions.
    /// </summary>
    private async Task SavePlayersFiltersCacheAsync()
    {
        try
        {
            var filterState = new PlayersFilterState
            {
                SearchText = searchText,
                SelectedTeam = selectedTeam,
                SelectedPosition = selectedPosition,
                GridState = currentGridState
            };

            await FiltersCacheService.SaveStateAsync(filterState);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving players state");
        }
    }

    #endregion

    #region Data Loading and Caching

    /// <summary>
    /// Loads player data from cache if available, otherwise fetches from API.
    /// Initiates background refresh if cached data is stale.
    /// </summary>
    private async Task LoadPlayersAsync()
    {
        try
        {
            var cachedData = await CacheService.GetCachedPlayersAsync();
            if (cachedData is not null)
            {
                GetRelevantPlayers(cachedData);

                // Update last updated timestamp from cache
                var timestamp = await CacheService.GetCacheTimestampAsync();
                if (timestamp.HasValue)
                {
                    lastUpdated = timestamp.Value.ToString("MM/dd/yyyy HH:mm");
                }

                // Start background refresh if data is stale
                if (await CacheService.IsCacheStaleAsync())
                {
                    _ = Task.Run(RefreshDataAsync);
                }
            }
            else
            {
                await RefreshDataAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading players");
        }
    }

    /// <summary>
    /// Fetches fresh player data from the API and updates the cache.
    /// </summary>
    private async Task RefreshDataAsync()
    {
        try
        {
            var response = await Http.GetFromJsonAsync<List<Player>>("api/players");
            if (response is not null)
            {
                GetRelevantPlayers(response);
                await CacheService.SavePlayersToCache(allPlayers);
                lastUpdated = DateTime.Now.ToString("MM/dd/yyyy HH:mm");
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error refreshing players data");
        }
    }

    /// <summary>
    /// Processes raw player data by filtering out invalid entries and applying default sorting.
    /// </summary>
    /// <param name="rawData">Raw player data from API or cache</param>
    private void GetRelevantPlayers(List<Player> rawData)
    {
        try
        {
            // Filter out players with missing essential data and sort by name
            allPlayers = rawData
                .Where(p => p is not null && p.Team.HasValue && p.Positions?.Count > 0)
                .OrderBy(p => p.SortableFullName ?? p.FullName ?? $"{p.LastName}, {p.FirstName}")
                .ToList();

            ApplyFilters();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing players data");
            allPlayers = [];
            players = [];
        }
    }

    #endregion

    #region Filtering Logic

    /// <summary>
    /// Applies current filter criteria (search text, team, position) to the complete player list.
    /// </summary>
    private void ApplyFilters()
    {
        try
        {
            var filtered = allPlayers.AsEnumerable();

            // Apply search text filter across name fields
            if (!string.IsNullOrEmpty(searchText))
            {
                var search = searchText.ToLowerInvariant();
                filtered = filtered.Where(p =>
                    p.FirstName?.Contains(search, StringComparison.InvariantCultureIgnoreCase) == true ||
                    p.LastName?.Contains(search, StringComparison.InvariantCultureIgnoreCase) == true ||
                    p.FullName?.Contains(search, StringComparison.InvariantCultureIgnoreCase) == true);
            }

            // Apply team filter
            if (selectedTeam.HasValue)
            {
                filtered = filtered.Where(p => p.Team == selectedTeam.Value);
            }

            // Apply position filter
            if (selectedPosition.HasValue)
            {
                filtered = filtered.Where(p => p.Positions?.Contains(selectedPosition.Value) == true);
            }

            players = filtered.ToList();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error applying filters");
            players = [];
        }
    }

    /// <summary>
    /// Handles filter changes by reapplying filters, saving state, and refreshing the grid.
    /// </summary>
    private async Task OnFiltersChanged()
    {
        try
        {
            ApplyFilters();
            await SavePlayersFiltersCacheAsync();
            if (dataGrid is not null)
            {
                await dataGrid.ReloadServerData();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in OnFiltersChanged");
        }
    }

    #endregion

    #region Data Grid Management

    /// <summary>
    /// Handles server-side data operations for the MudBlazor data grid including sorting and pagination.
    /// </summary>
    /// <param name="state">Current grid state with sorting and pagination information</param>
    /// <returns>Grid data with items for current page and total count</returns>
    private async Task<GridData<Player>> ServerReload(GridState<Player> state)
    {
        try
        {
            currentGridState = state;
            await SavePlayersFiltersCacheAsync();

            var data = players.AsEnumerable();

            // Apply sorting if specified
            if (state.SortDefinitions?.Count > 0)
            {
                var sortDefinition = state.SortDefinitions.First();
                data = ApplySort(data, sortDefinition);
            }

            // Apply pagination
            var totalItemsCount = data.Count();
            var pagedData = data.Skip(state.Page * state.PageSize).Take(state.PageSize).ToArray();

            return new GridData<Player>
            {
                Items = pagedData,
                TotalItems = totalItemsCount
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in ServerReload");
            return new GridData<Player> { Items = [], TotalItems = 0 };
        }
    }

    /// <summary>
    /// Applies sorting to player data based on the specified sort definition.
    /// </summary>
    /// <param name="data">Player data to sort</param>
    /// <param name="sortDefinition">Sort criteria including field and direction</param>
    /// <returns>Sorted player data</returns>
    private static IEnumerable<Player> ApplySort(IEnumerable<Player> data, SortDefinition<Player> sortDefinition)
    {
        return sortDefinition.SortBy switch
        {
            nameof(Player.FirstName) => sortDefinition.Descending
                ? data.OrderByDescending(p => p.FirstName)
                : data.OrderBy(p => p.FirstName),
            nameof(Player.LastName) => sortDefinition.Descending
                ? data.OrderByDescending(p => p.LastName)
                : data.OrderBy(p => p.LastName),
            nameof(Player.Age) => sortDefinition.Descending
                ? data.OrderByDescending(p => p.Age)
                : data.OrderBy(p => p.Age),
            nameof(Player.YearsExperience) => sortDefinition.Descending
                ? data.OrderByDescending(p => p.YearsExperience)
                : data.OrderBy(p => p.YearsExperience),
            nameof(Player.DepthChartPosition) => sortDefinition.Descending
                ? data.OrderByDescending(p => p.DepthChartPosition ?? 999)
                : data.OrderBy(p => p.DepthChartPosition ?? 999),
            // Default sort by name
            _ => data.OrderBy(p => p.SortableFullName ?? p.FullName ?? string.Empty)
        };
    }

    #endregion

    #region Search Functionality

    /// <summary>
    /// Provides filtered team options for the team autocomplete component.
    /// </summary>
    /// <param name="value">Search text to filter teams</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Filtered team options matching the search text</returns>
    private Task<IEnumerable<Team?>> SearchTeams(string value, CancellationToken token = default)
    {
        try
        {
            if (string.IsNullOrEmpty(value))
                return Task.FromResult(availableTeams.Cast<Team?>());

            return Task.FromResult(availableTeams.Cast<Team?>()
                .Where(t => t.ToString().Contains(value, StringComparison.InvariantCultureIgnoreCase)));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in SearchTeams");
            return Task.FromResult(Enumerable.Empty<Team?>());
        }
    }

    /// <summary>
    /// Provides filtered position options for the position autocomplete component.
    /// </summary>
    /// <param name="value">Search text to filter positions</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Filtered position options matching the search text</returns>
    private Task<IEnumerable<Position?>> SearchPositions(string value, CancellationToken token = default)
    {
        try
        {
            if (string.IsNullOrEmpty(value))
                return Task.FromResult(availablePositions.Cast<Position?>());

            return Task.FromResult(availablePositions.Cast<Position?>()
                .Where(p => p.ToString().Contains(value, StringComparison.InvariantCultureIgnoreCase)));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in SearchPositions");
            return Task.FromResult(Enumerable.Empty<Position?>());
        }
    }

    #endregion

    #region UI Helper Methods

    /// <summary>
    /// Returns the appropriate CSS color variable based on player injury status for visual indication.
    /// </summary>
    /// <param name="status">Player's current injury status</param>
    /// <returns>CSS color variable string for styling</returns>
    private static string GetInjuryStatusColorString(InjuryStatus status) => status switch
    {
        InjuryStatus.Out or InjuryStatus.IR or InjuryStatus.PUP => "var(--mud-palette-error)",
        InjuryStatus.Q or InjuryStatus.D => "var(--mud-palette-warning)",
        InjuryStatus.Sus => "var(--mud-palette-info)",
        InjuryStatus.COV => "var(--mud-palette-secondary)",
        _ => "var(--mud-palette-text-primary)"
    };

    #endregion
}