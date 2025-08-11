using FFLAssistant.Models.Enums;

namespace FFLAssistant.Models;
public class Player
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string SortableFullName => $"{LastName}, {FirstName}";
    public List<Position> Positions { get; set; } = [];
    public int? DepthChartPosition { get; set; }
    public Team? Team { get; set; }
    public int Age { get; set; }
    public int YearsExperience { get; set; }
    public InjuryStatus? InjuryStatus { get; set; }
    public bool IsDefense => Positions.Contains(Position.DEF);
}