namespace FFLAssistant.Models.Configurations;
public class SleeperConfiguration
{
    public const string SectionName = "Sleeper";
    public required string BaseUrl { get; set; }
    public required string SaveFilePath { get; set; }
    public string Url_GetPlayers => Path.Combine(BaseUrl, "v1/players/nfl");
}