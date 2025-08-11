using FFLAssistant.Models;
using FFLAssistant.Models.Components;
using FFLAssistant.Models.Enums;
using Microsoft.AspNetCore.Components;

namespace FFLAssistant.Client.Components;

public partial class DraftPickCard
{
    [Parameter] public DraftPickCardModel PickCard { get; set; } = null!;
    [Parameter] public int MyTeamIndex { get; set; }
    [Parameter] public int TotalTeams { get; set; }

    private static string ActivePickTextStyle => $"color: {ColorPalette.FootballBrown}; font-weight: bold; text-align: center; font-size: 11px; line-height: 1.1;";
    private static string PlayerNameStyle => $"color: {ColorPalette.MidnightHuddle}; font-weight: 500; font-size: 14px; font-weight: bold; line-height: 1.1; margin: 0; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;";
    private static string PositionTeamStyle => $"color: {ColorPalette.MidnightHuddle}; font-size: 9px; font-weight: bold; line-height: 1.1; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;";

    private string GetCardStyle()
    {
        var backgroundColor = GetBackgroundColor();
        var borderColor = GetBorderColor();
        return $"background-color: {backgroundColor}; border-radius: 6px; border: 2px solid {borderColor}; margin: 2px;";
    }

    private string GetBackgroundColor()
    {
        if (PickCard.IsActivePick)
        {
            return ColorPalette.ActivePickYellow;
        }
        else if (PickCard.IsDrafted && PickCard.Player?.Positions?.Any() == true)
        {
            var primaryPosition = PickCard.Player.Positions.First();
            return primaryPosition switch
            {
                Position.RB => ColorPalette.Position_RB,
                Position.WR => ColorPalette.Position_WR,
                Position.QB => ColorPalette.Position_QB,
                Position.TE => ColorPalette.Position_TE,
                Position.K => ColorPalette.Position_K,
                Position.DEF => ColorPalette.Position_DEF,
                _ => ColorPalette.GrassyGreenMedium
            };
        }
        else
        {
            return ColorPalette.Blackboard;
        }
    }

    private string GetBorderColor()
    {
        if (IsMyTeam())
        {
            return ColorPalette.FootballFlagYellow;
        }
        else if (PickCard.IsActivePick)
        {
            return ColorPalette.FootballBrown;
        }
        else if (PickCard.IsDrafted)
        {
            return ColorPalette.MidnightHuddle;
        }
        else
        {
            return ColorPalette.FieldLineGreen;
        }
    }

    private bool IsMyTeam()
    {
        if (PickCard.IsOddRound)
        {
            // Odd rounds: normal order (team 0 picks first, then 1, 2, 3...)
            return (PickCard.PickInRound - 1) == MyTeamIndex;
        }
        else
        {
            // Even rounds: reverse order (team (TotalTeams-1) picks first, then (TotalTeams-2)...)
            return (TotalTeams - PickCard.PickInRound) == MyTeamIndex;
        }
    }

    private string GetPickNumberColor()
    {
        if (PickCard.IsActivePick)
        {
            return ColorPalette.FootballBrown;
        }
        else if (PickCard.IsDrafted)
        {
            return ColorPalette.MidnightHuddle;
        }
        else
        {
            return ColorPalette.ChalkDust;
        }
    }
}