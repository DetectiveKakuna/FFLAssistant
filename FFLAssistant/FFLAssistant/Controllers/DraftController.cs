using FFLAssistant.Models;
using FFLAssistant.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FFLAssistant.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DraftController(IDraftRankingsService draftRankingsService, ISleeperLiveDraftService sleeperLiveDraftService) : ControllerBase
{
    private readonly IDraftRankingsService _draftRankingsService = draftRankingsService;
    private readonly ISleeperLiveDraftService _sleeperLiveDraftService = sleeperLiveDraftService;

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