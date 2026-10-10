using GamePlayer.Api.Dtos;
using GamePlayer.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GamePlayer.Api.Controllers;

[ApiController]
[Route("api/leaderboard")]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService;

    public LeaderboardController(
        ILeaderboardService leaderboardService
    )
    {
        _leaderboardService = leaderboardService;
    }

    [HttpGet]
    public async Task<ActionResult<LeaderboardResponse>>
        GetLeaderboard(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
    {
        if (page < 1)
        {
            return BadRequest(new
            {
                message = "Page must be at least 1."
            });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new
            {
                message =
                    "Page size must be between 1 and 100."
            });
        }

        var response =
            await _leaderboardService
                .GetLeaderboardAsync(
                    page,
                    pageSize
                );

        return Ok(response);
    }
}