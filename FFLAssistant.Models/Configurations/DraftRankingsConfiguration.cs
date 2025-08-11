namespace FFLAssistant.Models.Configurations;

public class DraftRankingsConfiguration
{
    public const string SectionName = "DraftRankings";
    public required string RankingsCsvPath { get; set; }
    public required string RankingsJsonPath { get; set; }
}