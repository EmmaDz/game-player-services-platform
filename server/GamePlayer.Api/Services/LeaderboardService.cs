using GamePlayer.Api.Data;
using GamePlayer.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace GamePlayer.Api.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly AppDbContext _dbContext;

    public LeaderboardService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LeaderboardResponse> GetLeaderboardAsync(
        int page,
        int pageSize
    )
    {
        var leaderboardQuery = _dbContext.MatchParticipants
            .AsNoTracking()
            .Where(
                participant =>
                    participant.Match.GameMode == "Ranked"
            )
            .GroupBy(
                participant => new
                {
                    participant.PlayerProfileId,
                    participant.PlayerProfile.DisplayName
                }
            )
            .Select(
                playerGroup => new LeaderboardEntryResponse
                {
                    PlayerId =
                        playerGroup.Key.PlayerProfileId,

                    DisplayName =
                        playerGroup.Key.DisplayName,

                    MatchesPlayed =
                        playerGroup.Count(),

                    Wins =
                        playerGroup.Count(
                            participant =>
                                participant.Result == "Win"
                        ),

                    TotalScore =
                        playerGroup.Sum(
                            participant =>
                                participant.Score
                        ),

                    TotalKills =
                        playerGroup.Sum(
                            participant =>
                                participant.Kills
                        ),

                    TotalDeaths =
                        playerGroup.Sum(
                            participant =>
                                participant.Deaths
                        )
                }
            );

        var totalCount =
            await leaderboardQuery.CountAsync();

        var items = await leaderboardQuery
            .OrderByDescending(
                entry => entry.TotalScore
            )
            .ThenByDescending(
                entry => entry.Wins
            )
            .ThenBy(
                entry => entry.DisplayName
            )
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)pageSize
        );

        return new LeaderboardResponse
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}