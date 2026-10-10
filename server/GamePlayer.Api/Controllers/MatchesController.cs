using GamePlayer.Api.Dtos;
using GamePlayer.Api.Services;
using GamePlayer.Api.Services.Results;
using Microsoft.AspNetCore.Mvc;

namespace GamePlayer.Api.Controllers;

[ApiController]
[Route("api/matches")]
public class MatchesController : ControllerBase
{
    private readonly IMatchService _matchService;

    public MatchesController(
        IMatchService matchService
    )
    {
        _matchService = matchService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MatchResponse>>
        GetMatchById(Guid id)
    {
        var match =
            await _matchService
                .GetMatchByIdAsync(id);

        if (match is null)
        {
            return NotFound(new
            {
                message =
                    $"Match with ID '{id}' was not found."
            });
        }

        return Ok(match);
    }

    [HttpPost]
    public async Task<ActionResult<MatchResponse>>
        CreateMatch(
            [FromBody]
            CreateMatchRequest request
        )
    {
        var result =
            await _matchService
                .CreateMatchAsync(request);

        if (
            result.IsSuccess
            && result.Match is not null
        )
        {
            return CreatedAtAction(
                nameof(GetMatchById),
                new
                {
                    id = result.Match.Id
                },
                result.Match
            );
        }

        switch (result.ErrorCode)
        {
            case CreateMatchErrorCode
                .DuplicateExternalMatchId:

                return Conflict(new
                {
                    message =
                        result.ErrorMessage
                });

            case CreateMatchErrorCode
                .PlayerNotFound:

                return BadRequest(new
                {
                    message =
                        result.ErrorMessage,

                    missingPlayerIds =
                        result.MissingPlayerIds
                });

            default:

                return BadRequest(new
                {
                    message =
                        result.ErrorMessage
                });
        }
    }
}