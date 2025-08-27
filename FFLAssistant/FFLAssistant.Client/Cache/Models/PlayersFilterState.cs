using FFLAssistant.Models;
using FFLAssistant.Models.Enums;
using MudBlazor;

namespace FFLAssistant.Client.Cache.Models;
public class PlayersFilterState
{
    public string? SearchText { get; set; }
    public Team? SelectedTeam { get; set; }
    public FFLAssistant.Models.Enums.Position? SelectedPosition { get; set; }
    public GridState<Player>? GridState { get; set; }
}