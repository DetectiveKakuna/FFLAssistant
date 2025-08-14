using FFLAssistant.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Text.RegularExpressions;

namespace FFLAssistant.Client.Components;

public partial class PlayerDialog
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public Player Player { get; set; } = new Player();

    [Inject] private HttpClient HttpClient { get; set; } = default!;

    private IList<AnalystNote> Notes { get; set; } = [];
    private string PlayerId = string.Empty;
    private string? HeadshotUrl => $"https://images.fantasypros.com/images/players/nfl/{PlayerId}/headshot/160x160.png";

    protected override async Task OnInitializedAsync()
    {
        var cleanFirstName = Regex.Replace(Player.FirstName.ToLower(), "[^a-z]", "");
        var cleanLastName = Regex.Replace(Player.LastName.ToLower(), "[^a-z]", "");

        var url = $"https://www.fantasypros.com/nfl/notes/{cleanFirstName}-{cleanLastName}.php";
        var html = await HttpClient.GetStringAsync(url);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        if (doc != null)
        {
            GetPlayerId(doc);
            GetNotes(doc);
        }
    }

    private void Close() => MudDialog.Close();

    private void GetPlayerId(HtmlDocument doc)
    {
        var h1Node = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'primary-heading-subheading')]//h1");

        var h1Class = h1Node?.GetAttributeValue("class", "") ?? "";

        this.PlayerId = h1Class.Split('-', 2)[1];
    }

    private void GetNotes(HtmlDocument doc)
    {
        var notesDiv = doc.QuerySelectorAll("div.subsection");
        
        if (notesDiv != null)
        {
            foreach (var note in notesDiv)
            {
                // Grab the <p> content
                var pNode = note.SelectSingleNode(".//div[contains(@class,'body-row')]//div[contains(@class,'content')]/p");
                var paragraph = pNode?.InnerText.Trim() ?? "";

                // Grab the <a> content
                var aNode = note.SelectSingleNode(".//div[contains(@class,'foot-row')]//a");
                var linkText = aNode?.InnerText?.Split(" - ", 2, StringSplitOptions.TrimEntries).FirstOrDefault() ?? "";

                // Grab the <span> content
                var spanNode = note.SelectSingleNode(".//div[contains(@class,'foot-row')]//span");
                var timestamp = spanNode?.InnerText.Trim() ?? "";

                // Skip if all fields are empty
                if (!string.IsNullOrWhiteSpace(paragraph))
                {
                    this.Notes.Add(new AnalystNote
                    {
                        Note = paragraph,
                        Analyst = linkText,
                        Timestamp = timestamp
                    });
                }
            }
        }
    }

    private string GetDialogStyle()
    {
        return $"border-radius: 8px; background-color: {ColorPalette.GrassyGreenDark};";
    }

    private string GetNoteStyle()
    {
        return $"border-radius: 8px; background-color: {ColorPalette.GrassyGreenMedium}";
    }

    private string GetPositionDepthStyle()
    {
        var positionColor = string.Empty;

        switch (this.Player.PrimaryPosition)
        {
            case Models.Enums.Position.QB:
                positionColor = ColorPalette.Position_QB;
                break;
            case Models.Enums.Position.RB:
                positionColor = ColorPalette.Position_RB;
                break;
            case Models.Enums.Position.WR:
                positionColor = ColorPalette.Position_WR;
                break;
            case Models.Enums.Position.TE:
                positionColor = ColorPalette.Position_TE;
                break;
            case Models.Enums.Position.K:
                positionColor = ColorPalette.Position_K;
                break;
            case Models.Enums.Position.DEF:
                positionColor = ColorPalette.Position_DEF;
                break;
        }

        return $"background-color: {positionColor};";
    }
}