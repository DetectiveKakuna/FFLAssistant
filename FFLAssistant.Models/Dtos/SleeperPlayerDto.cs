using System.Text.Json.Serialization;

namespace FFLAssistant.Models.Dtos;

public class SleeperPlayerDto
{
    [JsonPropertyName("player_id")]
    public string? PlayerId { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("fantasy_positions")]
    public List<string>? FantasyPositions { get; set; }

    [JsonPropertyName("depth_chart_order")]
    public int? DepthChartOrder { get; set; }

    [JsonPropertyName("team")]
    public string? Team { get; set; }

    [JsonPropertyName("age")]
    public int? Age { get; set; }

    [JsonPropertyName("years_exp")]
    public int? YearsExperience { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("injury_status")]
    public string? InjuryStatus { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }
}