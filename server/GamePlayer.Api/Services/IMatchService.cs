using GamePlayer.Api.Dtos;
using GamePlayer.Api.Services.Results;

namespace GamePlayer.Api.Services;

public interface IMatchService
{
    Task<MatchResponse?> GetMatchByIdAsync(
        Guid id
    );

    Task<CreateMatchResult> CreateMatchAsync(
        CreateMatchRequest request
    );
}