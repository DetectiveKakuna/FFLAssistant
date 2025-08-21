using FFLAssistant.Models;
using FFLAssistant.Models.Configurations;
using FFLAssistant.Services.Interfaces;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace FFLAssistant.Services;

public class FantasyProsService : IFantasyProsService
{
    private readonly HttpClient _httpClient;
    private readonly FantasyProsConfiguration _config;

    public FantasyProsService(HttpClient httpClient, IOptions<FantasyProsConfiguration> options)
    {
        _httpClient = httpClient;
        _config = options.Value;
    }

    public async Task<PlayerNotes> GetPlayerNotesAsync(string firstName, string lastName)
    {
        var cleanFirstName = Regex.Replace(firstName.ToLower(), "[^a-z]", "");
        var cleanLastName = Regex.Replace(lastName.ToLower(), "[^a-z]", "");
        var cleanFullName = $"{cleanFirstName}-{cleanLastName}";

        var url = $"{_config.BaseUrl}notes/{cleanFullName}.php";
        var html = await _httpClient.GetStringAsync(url);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var playerId = GetPlayerId(doc);
        var notes = GetNotes(doc);

        return new()
        {
            PlayerId = playerId,
            Notes = notes,
        };
    }

    public async Task<Dictionary<string, string>> GetDraftProjectionsAsync(string firstName, string lastName)
    {
        var results = new Dictionary<string, string>();

        var cleanFirstName = Regex.Replace(firstName.ToLower(), "[^a-z]", "");
        var cleanLastName = Regex.Replace(lastName.ToLower(), "[^a-z]", "");
        var cleanFullName = $"{cleanFirstName}-{cleanLastName}";

        var url = $"{_config.BaseUrl}projections/{cleanFullName}.php?scoring=PPR";
        var html = await _httpClient.GetStringAsync(url);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Find the subsection div that contains an h5 with "Season Projections"
        var seasonDiv = doc.DocumentNode.SelectSingleNode(
            "//div[contains(@class,'subsection')]" +
            "[.//h5[contains(text(), 'Season Projections')]]"
        );

        if (seasonDiv != null)
        {
            // Find headers
            var headers = seasonDiv.SelectNodes(".//table//thead//th");
            // Find data cells
            var dataCells = seasonDiv.SelectNodes(".//table//tbody//tr[1]//td");

            if (headers != null && dataCells != null && headers.Count == dataCells.Count)
            {
                for (int i = 0; i < headers.Count; i++)
                {
                    var headerText = headers[i].InnerText.Trim();
                    var valueText = dataCells[i].InnerText.Trim();

                    results[headerText] = valueText;
                }
            }
        }

        return results;
    }

    private string GetPlayerId(HtmlDocument? doc)
    {
        var h1Node = doc?.DocumentNode.SelectSingleNode("//div[contains(@class,'primary-heading-subheading')]//h1");
        var h1Class = h1Node?.GetAttributeValue("class", "") ?? "";
        return h1Class.Split('-', 2).Length > 1 ? h1Class.Split('-', 2)[1] : string.Empty;
    }

    private IList<AnalystNote> GetNotes(HtmlDocument? doc)
    {
        var notes = new List<AnalystNote>();
        var notesDiv = doc?.QuerySelectorAll("div.subsection");

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
                    notes.Add(new AnalystNote
                    {
                        Note = paragraph,
                        Analyst = linkText,
                        Timestamp = timestamp
                    });
                }
            }
        }

        return notes;
    }
}