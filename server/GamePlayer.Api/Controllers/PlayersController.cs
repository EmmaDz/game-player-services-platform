using GamePlayer.Api.Data;
using GamePlayer.Api.Dtos;
using GamePlayer.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamePlayer.Api.Controllers;

[ApiController]
[Route("api/players")]
public class PlayersController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public PlayersController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<PlayerListResponse>> GetPlayers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string sortBy = "displayName",
        [FromQuery] string sortDirection = "asc"
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
                message = "Page size must be between 1 and 100."
            });
        }

        var normalizedSortDirection =
            sortDirection.Trim().ToLower();

        if (
            normalizedSortDirection != "asc"
            && normalizedSortDirection != "desc"
        )
        {
            return BadRequest(new
            {
                message =
                    "Sort direction must be either 'asc' or 'desc'."
            });
        }

        var normalizedSortBy =
            sortBy.Trim().ToLower();

        if (
            normalizedSortBy != "displayname"
            && normalizedSortBy != "createdat"
        )
        {
            return BadRequest(new
            {
                message =
                    "Sort by must be either 'displayName' or 'createdAt'."
            });
        }

        var query = _dbContext.PlayerProfiles
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();

            query = query.Where(
                player =>
                    EF.Functions.ILike(
                        player.DisplayName,
                        $"%{searchTerm}%"
                    )
            );
        }

        var totalCount = await query.CountAsync();

        query = normalizedSortBy switch
        {
            "createdat" when normalizedSortDirection == "desc"
                => query
                    .OrderByDescending(
                        player => player.CreatedAtUtc
                    )
                    .ThenBy(player => player.Id),

            "createdat"
                => query
                    .OrderBy(
                        player => player.CreatedAtUtc
                    )
                    .ThenBy(player => player.Id),

            "displayname" when normalizedSortDirection == "desc"
                => query.OrderByDescending(
                    player => player.DisplayName
                ),

            _ => query.OrderBy(
                player => player.DisplayName
            )
        };

        var players = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(player => new PlayerSummaryResponse
            {
                Id = player.Id,
                DisplayName = player.DisplayName,
                Region = player.Region,
                CreatedAtUtc = player.CreatedAtUtc
            })
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)pageSize
        );

        var response = new PlayerListResponse
        {
            Items = players,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlayerProfileResponse>> GetPlayerById(
        Guid id
    )
    {
        var player = await _dbContext.PlayerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(player => player.Id == id);

        if (player is null)
        {
            return NotFound(new
            {
                message = $"Player with ID '{id}' was not found."
            });
        }

        return Ok(ToResponse(player));
    }

    [HttpPost]
    public async Task<ActionResult<PlayerProfileResponse>> CreatePlayer(
        [FromBody] CreatePlayerProfileRequest request
    )
    {
        var displayName = request.DisplayName.Trim();
        var region = request.Region.Trim();

        string? bio = string.IsNullOrWhiteSpace(request.Bio)
            ? null
            : request.Bio.Trim();

        if (displayName.Length < 3)
        {
            return BadRequest(new
            {
                message =
                    "Display name must contain at least 3 non-space characters."
            });
        }

        if (region.Length < 2)
        {
            return BadRequest(new
            {
                message =
                    "Region must contain at least 2 non-space characters."
            });
        }

        var normalizedDisplayName = displayName.ToLower();

        var displayNameAlreadyExists =
            await _dbContext.PlayerProfiles.AnyAsync(
                player =>
                    player.DisplayName.ToLower()
                    == normalizedDisplayName
            );

        if (displayNameAlreadyExists)
        {
            return Conflict(new
            {
                message =
                    $"The display name '{displayName}' is already in use."
            });
        }

        var now = DateTime.UtcNow;

        var player = new PlayerProfile
        {
            Id = Guid.NewGuid(),
            DisplayName = displayName,
            Region = region,
            Bio = bio,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _dbContext.PlayerProfiles.Add(player);

        await _dbContext.SaveChangesAsync();

        var response = ToResponse(player);

        return CreatedAtAction(
            nameof(GetPlayerById),
            new { id = player.Id },
            response
        );
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PlayerProfileResponse>> UpdatePlayer(
        Guid id,
        [FromBody] UpdatePlayerProfileRequest request
    )
    {
        var player = await _dbContext.PlayerProfiles
            .FirstOrDefaultAsync(player => player.Id == id);

        if (player is null)
        {
            return NotFound(new
            {
                message = $"Player with ID '{id}' was not found."
            });
        }

        var displayName = request.DisplayName.Trim();
        var region = request.Region.Trim();

        string? bio = string.IsNullOrWhiteSpace(request.Bio)
            ? null
            : request.Bio.Trim();

        if (displayName.Length < 3)
        {
            return BadRequest(new
            {
                message =
                    "Display name must contain at least 3 non-space characters."
            });
        }

        if (region.Length < 2)
        {
            return BadRequest(new
            {
                message =
                    "Region must contain at least 2 non-space characters."
            });
        }

        var normalizedDisplayName = displayName.ToLower();

        var displayNameAlreadyExists =
            await _dbContext.PlayerProfiles.AnyAsync(
                existingPlayer =>
                    existingPlayer.Id != id
                    && existingPlayer.DisplayName.ToLower()
                        == normalizedDisplayName
            );

        if (displayNameAlreadyExists)
        {
            return Conflict(new
            {
                message =
                    $"The display name '{displayName}' is already in use."
            });
        }

        player.DisplayName = displayName;
        player.Region = region;
        player.Bio = bio;
        player.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(player));
    }


    [HttpGet("{id:guid}/matches")]
    public async Task<ActionResult<PlayerMatchHistoryResponse>>
        GetPlayerMatchHistory(
            Guid id,
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

        var player = await _dbContext.PlayerProfiles
            .AsNoTracking()
            .Where(player => player.Id == id)
            .Select(player => new
            {
                player.Id,
                player.DisplayName
            })
            .FirstOrDefaultAsync();

        if (player is null)
        {
            return NotFound(new
            {
                message =
                    $"Player with ID '{id}' was not found."
            });
        }

        var query = _dbContext.MatchParticipants
            .AsNoTracking()
            .Where(
                participant =>
                    participant.PlayerProfileId == id
            );

        var totalCount = await query.CountAsync();

        var matches = await query
            .OrderByDescending(
                participant =>
                    participant.Match.PlayedAtUtc
            )
            .ThenBy(participant => participant.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(
                participant =>
                    new PlayerMatchSummaryResponse
                    {
                        MatchId =
                            participant.MatchId,

                        ExternalMatchId =
                            participant.Match.ExternalMatchId,

                        GameMode =
                            participant.Match.GameMode,

                        PlayedAtUtc =
                            participant.Match.PlayedAtUtc,

                        Score =
                            participant.Score,

                        Kills =
                            participant.Kills,

                        Deaths =
                            participant.Deaths,

                        Result =
                            participant.Result
                    }
            )
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)pageSize
        );

        var response =
            new PlayerMatchHistoryResponse
            {
                PlayerId = player.Id,
                DisplayName = player.DisplayName,
                Items = matches,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

        return Ok(response);
    }


    private static PlayerProfileResponse ToResponse(
        PlayerProfile player
    )
    {
        return new PlayerProfileResponse
        {
            Id = player.Id,
            DisplayName = player.DisplayName,
            Region = player.Region,
            Bio = player.Bio,
            CreatedAtUtc = player.CreatedAtUtc
        };
    }
}