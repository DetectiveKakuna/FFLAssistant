namespace FFLAssistant.Models;

public class PlayerNotes
{
    public string PlayerId { get; set; } = string.Empty;
    public IList<AnalystNote> Notes { get; set; } = [];
}

public class AnalystNote
{
    public string Note { get; set; }
    public string Analyst { get; set; }
    public string Timestamp { get; set; }
}