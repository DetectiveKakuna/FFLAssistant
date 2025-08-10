using MudBlazor;

namespace FFLAssistant.Client.Layout;

public partial class MainLayout
{
    private MudTheme? _theme = null;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _theme = new()
        {
            PaletteDark = _darkPalette,
            LayoutProperties = new LayoutProperties(),
        };
    }

    #region Color Palette
    // Constants for repeating values
    private const string White = "#ffffff";
    private const string Gray929F = "#92929f";
    private const string Gray1A1A27 = "#1a1a27";

    // Football-themed color constants
    private const string Blackboard = "#1c1c1c";           // Deep blackboard black
    private const string ChalkDust = "#e8e8e8";            // Slightly dusty chalk color
    private const string ChalkWhite = "#f5f5dc";           // Off-white like field chalk lines
    private const string GrassyGreenDark = "#2d4a2b";      // Dark grassy green
    private const string GrassyGreenMedium = "#3d5a3b";    // Medium grassy green
    private const string FootballBrown = "#8b4513";        // Classic football brown

    private readonly PaletteDark _darkPalette = new()
    {
        // Primary Colors
        Primary = "#7e6fff",
        PrimaryContrastText = White,
        PrimaryDarken = "#6950e8",
        PrimaryLighten = "#9580ff",

        // Secondary Colors  
        Secondary = "#ff4081",
        SecondaryContrastText = White,
        SecondaryDarken = "#e91e63",
        SecondaryLighten = "#ff79b0",

        // Tertiary Colors
        Tertiary = "#1ec8a5",
        TertiaryContrastText = White,
        TertiaryDarken = "#17a085",
        TertiaryLighten = "#4dd0b7",

        // Status Colors
        Info = "#4a86ff",
        InfoContrastText = White,
        InfoDarken = "#2962ff",
        InfoLighten = "#7dabff",

        Success = "#3dcb6c",
        SuccessContrastText = White,
        SuccessDarken = "#2e7d32",
        SuccessLighten = "#66bb6a",

        Warning = "#ffb545",
        WarningContrastText = "#000000",
        WarningDarken = "#f57c00",
        WarningLighten = "#ffcc02",

        Error = "#ff3f5f",
        ErrorContrastText = White,
        ErrorDarken = "#d32f2f",
        ErrorLighten = "#ef5350",

        Dark = "#27272f",
        DarkContrastText = White,
        DarkDarken = Gray1A1A27,
        DarkLighten = "#32323a",

        // Background Colors
        Background = GrassyGreenDark,
        BackgroundGray = "#243423",
        Surface = GrassyGreenMedium,

        // Text Colors
        TextPrimary = ChalkWhite,
        TextSecondary = Gray929F,
        TextDisabled = "#ffffff33",

        // Action Colors
        ActionDefault = "#74718e",
        ActionDisabled = "#9999994d",
        ActionDisabledBackground = "#605f6d4d",

        // AppBar
        AppbarBackground = FootballBrown,
        AppbarText = ChalkWhite,

        // Drawer/Navigation
        DrawerBackground = Blackboard,
        DrawerText = ChalkWhite,
        DrawerIcon = ChalkDust,

        // Lines and Dividers
        LinesDefault = "#4a6348",
        LinesInputs = "#4a6348",
        TableLines = "#4a6348",
        TableStriped = "#00000005",
        TableHover = "#ffffff0a",
        Divider = "#333333",
        DividerLight = "#444444",

        // Gray Scale
        Black = "#000000",
        White = White,
        GrayDefault = "#95939f",
        GrayLight = "#4a6348",
        GrayLighter = GrassyGreenMedium,
        GrayDark = "#6c757d",
        GrayDarker = "#343a40",

        // Overlay
        OverlayDark = "#1c1c1ccc",
        OverlayLight = "#3d5a3b80",

        // Opacity Properties
        HoverOpacity = 0.06,
        RippleOpacity = 0.1,
        BorderOpacity = 0.12,
        RippleOpacitySecondary = 0.15,

        // Skeleton
        Skeleton = "#4a6348",
    };
    #endregion
}