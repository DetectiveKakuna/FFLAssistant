using FFLAssistant.Models;
using Microsoft.AspNetCore.Components;

namespace FFLAssistant.Client.Components;

public partial class DraftRankingsList
{
    [Parameter] public List<DraftRanking> FilteredRankings { get; set; } = new();
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public int DefaultDisplay { get; set; } = 5;

    private bool IsExpanded { get; set; } = false;

    private void ToggleExpanded()
    {
        IsExpanded = !IsExpanded;
    }

    private string GetCardStyle()
    {
        return $"background-color: {ColorPalette.GrassyGreenMedium}; " +
               $"border: 2px solid {ColorPalette.FieldLineGreen}; " +
               $"border-radius: 8px; " +
               $"height: {(IsExpanded ? "60.5vh" : "auto")};";
    }

    private string GetTitleStyle()
    {
        return $"color: {ColorPalette.ChalkWhite}; " +
               $"font-weight: bold; " +
               $"text-shadow: 1px 1px 2px rgba(0,0,0,0.5);";
    }

    private string GetPlayerTextStyle(bool isDrafted = false)
    {
        var color = ColorPalette.ChalkWhite;
        var decoration = isDrafted ? "line-through" : "none";
        var opacity = isDrafted ? "0.6" : "1";

        return $"color: {color}; " +
               $"text-decoration: {decoration}; " +
               $"opacity: {opacity}; " +
               $"font-size: 14px; " +
               $"line-height: 1.3;";
    }

    private string GetEmptyTextStyle()
    {
        return $"color: {ColorPalette.ChalkDust}; " +
               $"font-style: italic; " +
               $"opacity: 0.7;";
    }

    private string GetArrowButtonStyle()
    {
        return $"background-color: {ColorPalette.FootballBrown}; " +
               $"border-radius: 50%; " +
               $"border: 1px solid {ColorPalette.FieldLineGreen}; " +
               $"width: 32px; " +
               $"height: 32px; " +
               $"min-width: 32px; " +
               $"min-height: 32px;";
    }
}