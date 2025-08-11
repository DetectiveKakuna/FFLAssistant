using FFLAssistant.Models;
using FFLAssistant.Models.Components;
using FFLAssistant.Models.Enums;
using Microsoft.AspNetCore.Components;

namespace FFLAssistant.Client.Components;

public partial class DraftPickCard
{
    [Parameter] public DraftPickCardModel PickCard { get; set; } = null!;

    private string GetCardStyle()
    {
        var backgroundColor = GetBackgroundColor();
        var borderColor = GetBorderColor();
        return $"background-color: {backgroundColor}; border-radius: 6px; border: 1px solid {borderColor}; margin: 2px;";
    }

    private string GetBackgroundColor()
    {
        if (PickCard.IsActivePick)
        {
            return "#ffd900"; // Chalk white for active pick (like chalk on blackboard)
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
                _ => ColorPalette.GrassyGreenMedium // Football theme fallback
            };
        }
        else
        {
            return ColorPalette.Blackboard; // Deep blackboard black for empty picks
        }
    }

    private string GetBorderColor()
    {
        if (PickCard.IsActivePick)
        {
            return ColorPalette.FootballBrown; // Football brown border for active pick
        }
        else if (PickCard.IsDrafted)
        {
            return "rgba(0,0,0,0.2)"; // Subtle border for drafted players
        }
        else
        {
            return ColorPalette.FieldLineGreen; // Field line color for empty picks
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
            return "white";
        }
        else
        {
            return ColorPalette.ChalkDust;
        }
    }

    private string GetActivePickTextStyle()
    {
        return $"color: {ColorPalette.FootballBrown}; font-weight: bold; text-align: center; font-size: 11px; line-height: 1.1;";
    }

    private string GetPlayerNameStyle()
    {
        return $"color: {ColorPalette.ChalkWhite}; font-weight: 500; font-size: 14px; font-weight: bold; line-height: 1.1; margin: 0; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;";
    }

    private string GetPositionTeamStyle()
    {
        return $"color: {ColorPalette.ChalkDust}; font-size: 9px; font-weight: bold; line-height: 1.1; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;";
    }
}