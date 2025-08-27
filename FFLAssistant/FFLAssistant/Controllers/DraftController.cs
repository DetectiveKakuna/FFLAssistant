using FFLAssistant.Models;
using FFLAssistant.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FFLAssistant.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DraftController(IDraftRankingsService draftRankingsService, IFantasyProsService fantasyProsService, ISleeperLiveDraftService sleeperLiveDraftService) : ControllerBase
{
    private readonly IDraftRankingsService _draftRankingsService = draftRankingsService;
    private readonly IFantasyProsService _fantasyProsService = fantasyProsService;
    private readonly ISleeperLiveDraftService _sleeperLiveDraftService = sleeperLiveDraftService;

    [HttpGet("projections")]
    public async Task<ActionResult<Dictionary<string, string>>> GetDraftProjectionsAsync(string firstName, string lastName)
    {
        try
        {
            var result = await _fantasyProsService.GetDraftProjectionsAsync(firstName, lastName);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error getting draft projectons: {ex.Message}");
        }
    }

    [HttpGet("{draftId}")]
    public async Task<ActionResult<DraftState?>> GetDraftState(string draftId)
    {
        try
        {
            var draftState = await _sleeperLiveDraftService.GetDraftStateAsync(draftId);
            return Ok(draftState);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error getting draft state: {ex.Message}");
        }
    }

    [HttpGet("rankings")]
    public async Task<ActionResult<IList<DraftRanking>?>> GetDraftRankings()
    {
        try
        {
            var draftState = await _draftRankingsService.GetDraftRankingsAsync();
            return Ok(draftState);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error getting draft state: {ex.Message}");
        }
    }
}