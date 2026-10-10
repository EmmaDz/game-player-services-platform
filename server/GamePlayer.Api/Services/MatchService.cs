using GamePlayer.Api.Data;
using GamePlayer.Api.Dtos;
using GamePlayer.Api.Models;
using GamePlayer.Api.Services.Results;
using Microsoft.EntityFrameworkCore;

namespace GamePlayer.Api.Services;

public class MatchService : IMatchService
{
    private readonly AppDbContext _dbContext;

    public MatchService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MatchResponse?> GetMatchByIdAsync(
        Guid id
    )
    {
        return await _dbContext.Matches
            .AsNoTracking()
            .Where(match => match.Id == id)
            .Select(match => new MatchResponse
            {
                Id = match.Id,
                ExternalMatchId = match.ExternalMatchId,
                GameMode = match.GameMode,
                PlayedAtUtc = match.PlayedAtUtc,
                CreatedAtUtc = match.CreatedAtUtc,

                Participants = match.Participants
                    .OrderByDescending(
                        participant =>
                            participant.Score
                    )
                    .Select(
                        participant =>
                            new MatchParticipantResponse
                            {
                                Id = participant.Id,

                                PlayerProfileId =
                                    participant.PlayerProfileId,

                                DisplayName =
                                    participant.PlayerProfile
                                        .DisplayName,

                                Score = participant.Score,
                                Kills = participant.Kills,
                                Deaths = participant.Deaths,
                                Result = participant.Result
                            }
                    )
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CreateMatchResult> CreateMatchAsync(
        CreateMatchRequest request
    )
    {
        var externalMatchId =
            request.ExternalMatchId.Trim();

        if (string.IsNullOrWhiteSpace(
            externalMatchId
        ))
        {
            return CreateMatchResult.Failure(
                CreateMatchErrorCode
                    .InvalidExternalMatchId,
                "External match ID cannot be empty."
            );
        }

        var gameMode =
            NormalizeGameMode(
                request.GameMode
            );

        if (gameMode is null)
        {
            return CreateMatchResult.Failure(
                CreateMatchErrorCode.InvalidGameMode,
                "Game mode must be either 'Ranked' or 'Casual'."
            );
        }

        var externalMatchAlreadyExists =
            await _dbContext.Matches.AnyAsync(
                match =>
                    match.ExternalMatchId ==
                    externalMatchId
            );

        if (externalMatchAlreadyExists)
        {
            return CreateMatchResult.Failure(
                CreateMatchErrorCode
                    .DuplicateExternalMatchId,
                $"Match '{externalMatchId}' already exists."
            );
        }

        var participantIds =
            request.Participants
                .Select(
                    participant =>
                        participant.PlayerProfileId
                )
                .ToList();

        if (
            participantIds.Any(
                id => id == Guid.Empty
            )
        )
        {
            return CreateMatchResult.Failure(
                CreateMatchErrorCode.InvalidPlayerId,
                "Every participant must have a valid player ID."
            );
        }

        if (
            participantIds.Distinct().Count()
            != participantIds.Count
        )
        {
            return CreateMatchResult.Failure(
                CreateMatchErrorCode
                    .DuplicateParticipant,
                "A player cannot appear more than once in the same match."
            );
        }

        var normalizedResults =
            new Dictionary<Guid, string>();

        foreach (
            var participant
            in request.Participants
        )
        {
            var result =
                NormalizeResult(
                    participant.Result
                );

            if (result is null)
            {
                return CreateMatchResult.Failure(
                    CreateMatchErrorCode
                        .InvalidResult,
                    "Participant result must be 'Win', 'Loss', or 'Draw'."
                );
            }

            normalizedResults[
                participant.PlayerProfileId
            ] = result;
        }

        var existingPlayers =
            await _dbContext.PlayerProfiles
                .AsNoTracking()
                .Where(
                    player =>
                        participantIds.Contains(
                            player.Id
                        )
                )
                .Select(
                    player => new
                    {
                        player.Id,
                        player.DisplayName
                    }
                )
                .ToListAsync();

        if (
            existingPlayers.Count
            != participantIds.Count
        )
        {
            var existingPlayerIds =
                existingPlayers
                    .Select(player => player.Id)
                    .ToHashSet();

            var missingPlayerIds =
                participantIds
                    .Where(
                        id =>
                            !existingPlayerIds
                                .Contains(id)
                    )
                    .ToList();

            return CreateMatchResult.Failure(
                CreateMatchErrorCode.PlayerNotFound,
                "One or more players do not exist.",
                missingPlayerIds
            );
        }

        var now = DateTime.UtcNow;

        var match = new Match
        {
            Id = Guid.NewGuid(),

            ExternalMatchId =
                externalMatchId,

            GameMode =
                gameMode,

            PlayedAtUtc =
                request.PlayedAtUtc.UtcDateTime,

            CreatedAtUtc =
                now
        };

        foreach (
            var participantRequest
            in request.Participants
        )
        {
            match.Participants.Add(
                new MatchParticipant
                {
                    Id = Guid.NewGuid(),

                    MatchId = match.Id,

                    PlayerProfileId =
                        participantRequest
                            .PlayerProfileId,

                    Score =
                        participantRequest.Score,

                    Kills =
                        participantRequest.Kills,

                    Deaths =
                        participantRequest.Deaths,

                    Result =
                        normalizedResults[
                            participantRequest
                                .PlayerProfileId
                        ]
                }
            );
        }

        _dbContext.Matches.Add(match);

        await _dbContext.SaveChangesAsync();

        var displayNames =
            existingPlayers.ToDictionary(
                player => player.Id,
                player => player.DisplayName
            );

        var response =
            new MatchResponse
            {
                Id = match.Id,

                ExternalMatchId =
                    match.ExternalMatchId,

                GameMode =
                    match.GameMode,

                PlayedAtUtc =
                    match.PlayedAtUtc,

                CreatedAtUtc =
                    match.CreatedAtUtc,

                Participants =
                    match.Participants
                        .OrderByDescending(
                            participant =>
                                participant.Score
                        )
                        .Select(
                            participant =>
                                new MatchParticipantResponse
                                {
                                    Id =
                                        participant.Id,

                                    PlayerProfileId =
                                        participant
                                            .PlayerProfileId,

                                    DisplayName =
                                        displayNames[
                                            participant
                                                .PlayerProfileId
                                        ],

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
                        .ToList()
            };

        return CreateMatchResult.Success(
            response
        );
    }

    private static string? NormalizeGameMode(
        string gameMode
    )
    {
        return gameMode
            .Trim()
            .ToLowerInvariant()
            switch
            {
                "ranked" => "Ranked",
                "casual" => "Casual",
                _ => null
            };
    }

    private static string? NormalizeResult(
        string result
    )
    {
        return result
            .Trim()
            .ToLowerInvariant()
            switch
            {
                "win" => "Win",
                "loss" => "Loss",
                "draw" => "Draw",
                _ => null
            };
    }
}