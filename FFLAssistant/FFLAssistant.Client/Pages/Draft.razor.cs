using FFLAssistant.Models;
using FFLAssistant.Models.Components;
using FFLAssistant.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace FFLAssistant.Client.Pages;

public partial class Draft : ComponentBase
{
    [Inject] private ISleeperLiveDraftService SleeperService { get; set; } = default!;
    [Inject] private ISleeperPlayersService SleeperPlayerService { get; set; } = default!;

    private List<DraftPickCardModel>? draftPickCards;
    private DraftState? currentDraftState;
    private IList<Player>? allPlayers;
    private const int totalRounds = 16; // This could also be dynamic if needed
    private const string draftId = "1260412747493933056"; // Your draft ID

    protected override async Task OnInitializedAsync()
    {
        await LoadDraftBoardAsync();
    }

    private async Task LoadDraftBoardAsync()
    {
        try
        {
            // Load players first
            allPlayers = await SleeperPlayerService.GetPlayersAsync();

            currentDraftState = await SleeperService.GetDraftStateAsync(draftId);
            if (currentDraftState != null)
            {
                await BuildDraftPickCards(currentDraftState);
                StateHasChanged(); // Force re-render after data is loaded
            }
        }
        catch (Exception ex)
        {
            // Handle error
            Console.WriteLine($"Error loading draft: {ex.Message}");
        }
    }

    private async Task BuildDraftPickCards(DraftState draftState)
    {
        draftPickCards = [];

        // Use the dynamic team count from draft state
        var totalPicks = draftState.TotalTeams * totalRounds;

        // Create all possible picks (snake draft order)
        for (int pick = 1; pick <= totalPicks; pick++)
        {
            var draftPick = draftState.Picks.FirstOrDefault(p => p.PickNumber == pick);
            Player? player = null;

            if (draftPick != null && !string.IsNullOrEmpty(draftPick.PlayerId) && allPlayers != null)
            {
                player = allPlayers.FirstOrDefault(p => p.Id == draftPick.PlayerId);
            }

            var card = new DraftPickCardModel(draftState.TotalTeams)
            {
                PickNumber = pick,
                Player = player,
                IsActivePick = pick == draftState.CurrentPick,
            };

            draftPickCards.Add(card);
        }
    }

    private List<DraftPickCardModel> GetPicksForRound(int round)
    {
        if (draftPickCards == null || currentDraftState == null)
            return [];

        var startPick = (round - 1) * currentDraftState.TotalTeams + 1;
        var endPick = round * currentDraftState.TotalTeams;

        var roundPicks = draftPickCards
            .Where(p => p.PickNumber >= startPick && p.PickNumber <= endPick)
            .ToList();

        // Handle snake draft order (reverse even rounds)
        if (round % 2 == 0)
        {
            roundPicks.Reverse();
        }

        return roundPicks;
    }
}