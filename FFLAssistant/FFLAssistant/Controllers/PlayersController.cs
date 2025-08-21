using FFLAssistant.Models;
using FFLAssistant.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FFLAssistant.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController(IFantasyProsService fantasyProsService, ISleeperPlayersService sleeperPlayersService) : ControllerBase
{
    private readonly IFantasyProsService _fantasyProsService = fantasyProsService;
    private readonly ISleeperPlayersService _sleeperPlayersService = sleeperPlayersService;

    [HttpGet]
    public async Task<ActionResult<IList<Player>>> GetPlayers()
    {
        try
        {
            var players = await _sleeperPlayersService.GetPlayersAsync();
            if (players == null)
            {
                return NotFound("No players found");
            }
            return Ok(players);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving players: {ex.Message}");
        }
    }

    [HttpGet("notes")]
    public async Task<ActionResult<PlayerNotes>> GetPlayerNotesAsync(string firstName, string lastName)
    {
        try
        {
            var result = await _fantasyProsService.GetPlayerNotesAsync(firstName, lastName);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error getting player notes: {ex.Message}");
        }
    }
}