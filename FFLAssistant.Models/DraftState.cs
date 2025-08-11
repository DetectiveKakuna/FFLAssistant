namespace FFLAssistant.Models;

public class DraftState
{
    public int CurrentPick { get; set; }
    public List<DraftTeam> Teams { get; set; } = [];
    public int TotalTeams => Teams.Count;
    public int TotalRounds { get; set; }
    public int FirstRelevantRound => TotalTeams == 0 || TotalRounds == 0
                                     ? 1
                                     : Math.Clamp((CurrentPick - 1) / TotalTeams, 1, TotalRounds - 2);
    public List<DraftPick> Picks { get; set; } = [];
}

public class DraftTeam
{
    public int Position { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public bool IsMyTeam { get; set; }
}

public class DraftPick
{
    public int PickNumber { get; set; }
    public string PlayerId { get; set; } = string.Empty;
}