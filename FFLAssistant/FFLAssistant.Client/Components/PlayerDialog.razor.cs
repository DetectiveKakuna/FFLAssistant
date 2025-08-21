using FFLAssistant.Models;
using FFLAssistant.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FFLAssistant.Client.Components;

public partial class PlayerDialog
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public Player Player { get; set; } = new();
    [Parameter] public bool IsDraft { get; set; } = false;

    [Inject] private IFantasyProsService FantasyProsService { get; set; } = default!;

    private PlayerNotes PlayerNotes { get; set; } = new();
    private Dictionary<string, string> Projections { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private string? HeadshotUrl => !string.IsNullOrEmpty(PlayerNotes.PlayerId)
        ? $"https://images.fantasypros.com/images/players/nfl/{PlayerNotes.PlayerId}/headshot/160x160.png"
        : null;


    protected override async Task OnInitializedAsync()
    {
        var notesTask = FantasyProsService.GetPlayerNotesAsync(Player.FirstName, Player.LastName);
        var projectionsTask = FantasyProsService.GetDraftProjectionsAsync(Player.FirstName, Player.LastName);

        await Task.WhenAll(notesTask, projectionsTask);

        PlayerNotes = await notesTask;
        Projections = await projectionsTask;

        IsLoading = false;
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