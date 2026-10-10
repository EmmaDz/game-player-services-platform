using GamePlayer.Api.Dtos;

namespace GamePlayer.Api.Services;

public interface ILeaderboardService
{
    Task<LeaderboardResponse> GetLeaderboardAsync(
        int page,
        int pageSize
    );
}