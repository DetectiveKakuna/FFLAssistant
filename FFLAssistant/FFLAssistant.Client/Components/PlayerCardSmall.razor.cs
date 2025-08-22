using FFLAssistant.Models;
using FFLAssistant.Models.Constants;
using FFLAssistant.Models.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FFLAssistant.Client.Components;

public partial class PlayerCardSmall : ComponentBase
{
    [Parameter] public int Ranking { get; set; }
    [Parameter] public Player Player { get; set; } = new Player();

    [Inject] private IDialogService DialogService { get; set; } = default!;


    private string GetCardStyle()
    {
        return "border-radius: 12px; overflow: hidden; border: 2px solid black; cursor: pointer;";
    }

    private string GetRankingSectionStyle()
    {
        return $"background-color: {ColorPalette.GrassyGreenMedium}; border-radius: 0;";
    }

    private string GetRankingTextStyle()
    {
        return "color: white; font-weight: bold; line-height: 1;";
    }

    private string GetPlayerInfoSectionStyle()
    {
        return $"background-color: {ColorPalette.GrassyGreenMedium};";
    }

    private string GetPlayerNameStyle()
    {
        return "color: white; font-weight: bold; line-height: 1.1;";
    }

    private string GetPlayerDetailsStyle()
    {
        return "color: white; font-size: 10px; line-height: 1.1;";
    }

    private string GetInjuryStatusStyle()
    {
        return "color: red; font-size: 10px; line-height: 1.1; font-weight: bold;";
    }

    private string GetTeamByeStyle()
    {
        return "color: white; font-size: 10px; line-height: 1.1; width:100%; text-align:right;";
    }

    private string GetPositionSectionStyle()
    {
        var position = GetPrimaryPosition();
        var backgroundColor = GetPositionColor(position);
        return $"background-color: {backgroundColor}; border-radius: 0;";
    }

    private string GetPositionTextStyle()
    {
        return "color: white; font-weight: bold; line-height: 1;";
    }

    private string GetDepthChartStyle()
    {
        return "color: white; font-size: 15px; font-weight: bold; line-height: 1.1;";
    }

    private string GetPrimaryPosition()
    {
        return Player.Positions?.FirstOrDefault().ToString() ?? "N/A";
    }

    private string GetPositionColor(string position)
    {
        return position switch
        {
            "RB" => ColorPalette.Position_RB,
            "WR" => ColorPalette.Position_WR,
            "QB" => ColorPalette.Position_QB,
            "TE" => ColorPalette.Position_TE,
            "K" => ColorPalette.Position_K,
            "DEF" => ColorPalette.Position_DEF,
            _ => ColorPalette.GrassyGreenMedium
        };
    }

    private string GetTeamByeText()
    {
        if (Player.Team == null)
            return "Team • Bye";

        var byeWeek = GetByeWeek(Player.Team.Value);
        return $"{Player.Team} • {(byeWeek.HasValue ? $"Bye {byeWeek}" : "Bye")}";
    }

    private int? GetByeWeek(Team team)
    {
        foreach (var kvp in ByeWeeks.Schedule)
        {
            if (kvp.Value.Contains(team))
            {
                return kvp.Key;
            }
        }
        return null;
    }

    private async Task OpenPlayerDialog()
    {
        var parameters = new DialogParameters
        {
            ["Player"] = Player,
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

        await DialogService.ShowAsync<PlayerDialog>(string.Empty, parameters, options);
    }

    private static string ToOrdinal19(int? number)
    {
        if (!number.HasValue)
            return string.Empty;

        int n = number.Value;

        // Handle special teens (11th, 12th, 13th)
        if (n % 100 is 11 or 12 or 13)
            return $"{n}th";

        return (n % 10) switch
        {
            1 => $"{n}st",
            2 => $"{n}nd",
            3 => $"{n}rd",
            _ => $"{n}th"
        };
    }
}