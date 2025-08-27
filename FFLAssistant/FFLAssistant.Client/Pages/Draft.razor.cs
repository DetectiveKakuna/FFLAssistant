using FFLAssistant.Models;
using FFLAssistant.Models.Components;
using FFLAssistant.Models.Enums;
using FFLAssistant.Models.Interfaces;
using Microsoft.AspNetCore.Components;

namespace FFLAssistant.Client.Pages;

public partial class Draft : ComponentBase
{
    [Inject] private IDraftRankingsService DraftRankingsService { get; set; } = default!;
    [Inject] private ISleeperLiveDraftService SleeperService { get; set; } = default!;
    [Inject] private ISleeperPlayersService SleeperPlayerService { get; set; } = default!;
    [Inject] private IFantasyProsService FantasyProsService { get; set; } = default!;

    private List<DraftPickCardModel>? draftPickCards;
    private IList<DraftRanking>? draftRankings;
    private DraftState? currentDraftState;
    private IList<Player>? allPlayers;
    private const string draftId = "1260412747493933056"; // Your draft ID
    private int? MyTeamIndex => currentDraftState?.Teams.FindIndex(t => t.IsMyTeam);

    protected override async Task OnInitializedAsync()
    {
        await LoadDataFiles();
        await GetDraftState();
    }

    private async Task USE_THIS_TO_CHECK_FOR_FANTASYPROS_NAME_MISMATCHES()
    {
        var players = draftRankings?.Select(dr => dr.Player);

        if (players is null)
            return;

        List<Player> unfound = [];

        foreach (var player in players)
        {
            var notes = await FantasyProsService.GetPlayerNotesAsync(player.FirstName, player.LastName);

            if (notes == null || notes.Notes.Count == 0)
                unfound.Add(player);
        }
    }

    private async Task LoadDataFiles()
    {
        try
        {
            // Load players first
            allPlayers = await SleeperPlayerService.GetPlayersAsync();

            // Load rankings second
            draftRankings = await DraftRankingsService.GetDraftRankingsAsync();
        }
        catch (Exception ex)
        {
            // Handle error
            Console.WriteLine($"Error loading draft: {ex.Message}");
        }
    }

    private async Task GetDraftState()
    {
        try
        {
            currentDraftState = await SleeperService.GetDraftStateAsync(draftId);

            if (currentDraftState != null)
            {
                BuildDraftPickCards(currentDraftState);

                // Set the draft state on the draft rankings
                if (draftRankings != null)
                {
                    var pickedIds = currentDraftState.Picks
                        .Where(p => !string.IsNullOrEmpty(p.PlayerId))
                        .Select(p => p.PlayerId);

                    draftRankings.Where(r => pickedIds.Contains(r.Player.Id)).ToList()
                                 .ForEach(r => r.IsDrafted = true);
                }
            }
        }
        catch (Exception ex)
        {
            // Handle error
            Console.WriteLine($"Error loading draft: {ex.Message}");
        }
    }

    private void BuildDraftPickCards(DraftState draftState)
    {
        draftPickCards = [];

        // Use the dynamic team count from draft state
        var totalPicks = draftState.TotalTeams * draftState.TotalRounds;

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

    private List<DraftRanking> GetRankingsByPosition(Position position)
    {
        if (draftRankings == null)
            return [];

        return draftRankings
            .Where(r => r.Player.Positions?.Contains(position) == true)
            .OrderBy(r => r.OverallRank)
            .ToList();
    }
}