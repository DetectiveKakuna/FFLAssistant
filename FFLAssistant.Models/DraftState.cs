namespace FFLAssistant.Models;

public class DraftState
{
    public int TotalTeams { get; set; }
    public int CurrentPick { get; set; }

    public List<DraftPick> Picks { get; set; } = [];
}

public class DraftPick
{
    public int PickNumber { get; set; }
    public string PlayerId { get; set; } = string.Empty;
}