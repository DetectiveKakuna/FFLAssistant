namespace FFLAssistant.Models;

public class DraftRanking
{
    public Player Player { get; set; } = new Player();
    public string Tier { get; set; } = string.Empty;
    public int OverallRank { get; set; }
    public int BestRank { get; set; }
    public int WorstRank { get; set; }
    public double AverageRank { get; set; }
    public double StandardDeviation { get; set; }
}