using FFLAssistant.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FFLAssistant.Client.Components;

public partial class DraftRankingsList : ComponentBase
{
    [Parameter] public List<DraftRanking> Rankings { get; set; } = [];
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public int DefaultDisplay { get; set; } = 10;
    [Parameter] public bool ShowPositions { get; set; }

    [Inject] private IDialogService DialogService { get; set; } = default!;

    private bool IsExpanded { get; set; } = false;

    private void ToggleExpanded()
    {
        IsExpanded = !IsExpanded;
        StateHasChanged();
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

    private string GetInjuryTextStyle(bool isDrafted, string color)
    {
        var decoration = isDrafted ? "line-through" : "none";
        var opacity = isDrafted ? "0.6" : "1";

        return $"color: {color}; " +
               $"text-decoration: {decoration}; " +
               $"opacity: {opacity}; " +
               $"font-size: 12px; " +
               $"line-height: 1.3;";
    }

    private string GetPlayerTextStyle(bool isDrafted)
    {
        var color = ColorPalette.ChalkWhite;
        var decoration = isDrafted ? "line-through" : "none";
        var opacity = isDrafted ? "0.6" : "1";

        return $"color: {color}; " +
               $"text-decoration: {decoration}; " +
               $"opacity: {opacity}; " +
               $"font-size: 12px; " +
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

    private string GetDisplayText(DraftRanking draftRanking)
    {
        if (ShowPositions)
        {
            return $"{draftRanking.OverallRank}. {draftRanking.Player.FullName} - {draftRanking.Player.Positions.FirstOrDefault()} ({draftRanking.Player.Team})";
        }

        return $"{draftRanking.OverallRank}. {draftRanking.Player.FullName} - ({draftRanking.Player.Team})";
    }

    private async Task OpenPlayerDialog(Player player)
    {
        var parameters = new DialogParameters
        {
            ["Player"] = player,
        };


        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Large,
            FullWidth = true,
            CloseButton = true,
            BackdropClick = true,
            CloseOnEscapeKey = true,
            NoHeader = true,
        };

        await DialogService.ShowAsync<PlayerDialog>($"{player.FullName}", parameters, options);
    }
}