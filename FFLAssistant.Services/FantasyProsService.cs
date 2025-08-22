using FFLAssistant.Models;
using FFLAssistant.Models.Configurations;
using FFLAssistant.Services.Interfaces;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace FFLAssistant.Services;

public class FantasyProsService(HttpClient httpClient, IOptions<FantasyProsConfiguration> options) : IFantasyProsService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly FantasyProsConfiguration _config = options.Value;

    private static readonly Dictionary<string, string> _nameOverrides =
    new(StringComparer.OrdinalIgnoreCase)
    {
        { "brian-thomas", "brian-thomas-jr" },
        { "marvin-harrison", "marvin-harrison-jr" },
        { "tyrone-tracy", "tyrone-tracy-jr" },
        { "brian-robinson", "brian-robinson-jr" },
        { "luther-burden", "luther-burden-iii" },
        { "michael-penix", "michael-penix-jr" },
        { "hollywood-brown", "marquise-brown" },
        { "joshua-palmer", "josh-palmer" },
        { "chig-okonkwo", "chigoziem-okonkwo" }
    };

    public async Task<Dictionary<string, string>> GetDraftProjectionsAsync(string firstName, string lastName)
    {
        var results = new Dictionary<string, string>();

        var doc = await ScrapePage($"{_config.BaseUrl}projections/", firstName, lastName, true);

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

    public async Task<Dictionary<string, string>> GetPlayerRankingsAsync(string firstName, string lastName)
    {
        var results = new Dictionary<string, string>();

        var doc = await ScrapePage($"{_config.BaseUrl}rankings/", firstName, lastName, true);

        // Find the subsection div that contains an h5 with "Season Projections"
        var rankingsDiv = doc.DocumentNode.SelectSingleNode(
            "//div[contains(@class,'subsection')]" +
            "[.//h5[contains(text(), 'Consensus Rankings')]]"
        );

        if (rankingsDiv != null)
        {
            // Find headers
            var headers = rankingsDiv.SelectNodes(".//table//thead//th");
            // Find data cells
            var dataCells = rankingsDiv.SelectNodes(".//table//tbody//tr[1]//td");

            if (headers != null && dataCells != null && headers.Count == dataCells.Count)
            {
                for (int i = 0; i < headers.Count; i++)
                {
                    var headerText = headers[i].InnerText.Trim();
                    var valueText = dataCells[i].InnerText.Trim();

                    if (!string.IsNullOrWhiteSpace(headerText))
                        results[headerText] = valueText;
                }
            }
        }
        return results;
    }

    public async Task<PlayerNotes> GetPlayerNotesAsync(string firstName, string lastName)
    {
        var doc = await ScrapePage($"{_config.BaseUrl}notes/", firstName, lastName, false);

        var playerId = GetPlayerId(doc);
        var notes = GetNotes(doc);

        return new()
        {
            PlayerId = playerId,
            Notes = notes,
        };
    }

    private static string GetPlayerId(HtmlDocument? doc)
    {
        var h1Node = doc?.DocumentNode.SelectSingleNode("//div[contains(@class,'primary-heading-subheading')]//h1");
        var h1Class = h1Node?.GetAttributeValue("class", "") ?? "";
        return h1Class.Split('-', 2).Length > 1 ? h1Class.Split('-', 2)[1] : string.Empty;
    }

    private static List<AnalystNote> GetNotes(HtmlDocument? doc)
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

    private async Task<HtmlDocument> ScrapePage(string baseUrl, string firstName, string lastName, bool needsPPR)
    {
        var cleanFirstName = Regex.Replace(firstName.ToLower(), "[^a-z]", "");
        var cleanLastName = Regex.Replace(lastName.ToLower(), "[^a-z-]", "");
        var cleanFullName = $"{cleanFirstName}-{cleanLastName}";

        // Apply override if it exists
        if (_nameOverrides.TryGetValue(cleanFullName, out var overridden))
            cleanFullName = overridden;

        var url = $"{baseUrl}{cleanFullName}.php";
        if (needsPPR)
            url += "?scoring=PPR";

        var html = await _httpClient.GetStringAsync(url);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc;
    }
}