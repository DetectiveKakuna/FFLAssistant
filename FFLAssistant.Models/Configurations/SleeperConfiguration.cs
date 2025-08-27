namespace FFLAssistant.Models.Configurations;

public class SleeperConfiguration
{
    public const string SectionName = "Sleeper";
    public required string ApiBaseUrl { get; set; }
    public required string DraftBaseUrl { get; set; }
    public required string UserName { get; set; }
    public required string SaveFilePath { get; set; }
    public string Url_GetPlayers => Path.Combine(ApiBaseUrl, "v1/players/nfl");
}