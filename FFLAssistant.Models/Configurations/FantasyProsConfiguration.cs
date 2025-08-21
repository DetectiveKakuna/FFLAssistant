namespace FFLAssistant.Models.Configurations;

public class FantasyProsConfiguration
{
    public const string SectionName = "FantasyPros";
    public required string BaseUrl { get; set; }
    public required string BaseImagesUrl { get; set; }
}