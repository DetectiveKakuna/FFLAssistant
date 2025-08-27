namespace FFLAssistant.Models.Components;

public class DraftPickCardModel(int totalTeams)
{
    public int PickNumber { get; set; }
    public Player? Player { get; set; }
    public bool IsActivePick { get; set; }
    public bool IsDrafted => Player != null;

    // Snake draft calculation helpers using dynamic team count
    public int Round => ((PickNumber - 1) / totalTeams) + 1;
    public int PickInRound => ((PickNumber - 1) % totalTeams) + 1;
    public bool IsOddRound => Round % 2 == 1;
}