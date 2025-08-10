using FFLAssistant.Models.Players;
using FFLAssistant.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FFLAssistant.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController(ISleeperPlayersService sleeperPlayersService) : ControllerBase
{
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
}