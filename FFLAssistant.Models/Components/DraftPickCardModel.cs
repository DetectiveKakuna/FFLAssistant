namespace FFLAssistant.Models.Components;

public class DraftPickCardModel(int totalTeams)
{
    public decimal PickNumber { get; set; }
    public Player? Player { get; set; }
    public bool IsActivePick { get; set; }
    public bool IsDrafted => Player != null;

    // Snake draft calculation helpers using dynamic team count
    public int Round => (int)Math.Ceiling(PickNumber / totalTeams);
    public int PickInRound => IsOddRound ? (int)((PickNumber - 1) % totalTeams) + 1 : totalTeams - (int)((PickNumber - 1) % totalTeams);
    public bool IsOddRound => Round % 2 == 1;
}