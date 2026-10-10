using GamePlayer.Api.Dtos;
using GamePlayer.Api.Models;
using GamePlayer.Api.Services;
using GamePlayer.Api.Services.Results;
using GamePlayer.Api.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GamePlayer.Api.Tests;

public class MatchServiceTests
{
    [Fact]
    public async Task CreateMatchAsync_ValidRequest_CreatesMatch()
    {
        await using var testDatabase =
            await SqliteTestDatabase.CreateAsync();

        var dbContext =
            testDatabase.DbContext;

        var now = DateTime.UtcNow;

        var player1 = new PlayerProfile
        {
            Id = Guid.NewGuid(),
            DisplayName = "PlayerOne",
            Region = "Canada",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var player2 = new PlayerProfile
        {
            Id = Guid.NewGuid(),
            DisplayName = "PlayerTwo",
            Region = "Canada",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        dbContext.PlayerProfiles.AddRange(
            player1,
            player2
        );

        await dbContext.SaveChangesAsync();

        var request =
            new CreateMatchRequest
            {
                ExternalMatchId =
                    "TEST-CREATE-001",

                GameMode =
                    "Ranked",

                PlayedAtUtc =
                    DateTimeOffset.UtcNow,

                Participants =
                    new List<
                        CreateMatchParticipantRequest
                    >
                    {
                        new()
                        {
                            PlayerProfileId =
                                player1.Id,
                            Score = 1200,
                            Kills = 8,
                            Deaths = 2,
                            Result = "Win"
                        },
                        new()
                        {
                            PlayerProfileId =
                                player2.Id,
                            Score = 900,
                            Kills = 4,
                            Deaths = 8,
                            Result = "Loss"
                        }
                    }
            };

        var service =
            new MatchService(dbContext);

        var result =
            await service.CreateMatchAsync(
                request
            );

        Assert.True(
            result.IsSuccess
        );

        Assert.NotNull(
            result.Match
        );

        Assert.Equal(
            "TEST-CREATE-001",
            result.Match.ExternalMatchId
        );

        Assert.Equal(
            2,
            result.Match.Participants.Count
        );

        Assert.Equal(
            1,
            await dbContext.Matches.CountAsync()
        );

        Assert.Equal(
            2,
            await dbContext.MatchParticipants.CountAsync()
        );
    }

    [Fact]
    public async Task CreateMatchAsync_DuplicateExternalMatchId_ReturnsDuplicateError()
    {
        await using var testDatabase =
            await SqliteTestDatabase.CreateAsync();

        var dbContext =
            testDatabase.DbContext;

        var now = DateTime.UtcNow;

        var existingMatch = new Match
        {
            Id = Guid.NewGuid(),
            ExternalMatchId =
                "TEST-DUPLICATE-001",
            GameMode = "Ranked",
            PlayedAtUtc = now,
            CreatedAtUtc = now
        };

        dbContext.Matches.Add(
            existingMatch
        );

        await dbContext.SaveChangesAsync();

        var request =
            new CreateMatchRequest
            {
                ExternalMatchId =
                    "TEST-DUPLICATE-001",

                GameMode =
                    "Ranked",

                PlayedAtUtc =
                    DateTimeOffset.UtcNow,

                Participants =
                    new List<
                        CreateMatchParticipantRequest
                    >()
            };

        var service =
            new MatchService(dbContext);

        var result =
            await service.CreateMatchAsync(
                request
            );

        Assert.False(
            result.IsSuccess
        );

        Assert.Equal(
            CreateMatchErrorCode
                .DuplicateExternalMatchId,
            result.ErrorCode
        );

        Assert.Equal(
            1,
            await dbContext.Matches.CountAsync()
        );
    }

    [Fact]
    public async Task CreateMatchAsync_MissingPlayer_ReturnsMissingPlayerIds()
    {
        await using var testDatabase =
            await SqliteTestDatabase.CreateAsync();

        var dbContext =
            testDatabase.DbContext;

        var now = DateTime.UtcNow;

        var existingPlayer =
            new PlayerProfile
            {
                Id = Guid.NewGuid(),
                DisplayName =
                    "ExistingPlayer",
                Region = "Canada",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

        dbContext.PlayerProfiles.Add(
            existingPlayer
        );

        await dbContext.SaveChangesAsync();

        var missingPlayerId =
            Guid.NewGuid();

        var request =
            new CreateMatchRequest
            {
                ExternalMatchId =
                    "TEST-MISSING-PLAYER",

                GameMode =
                    "Ranked",

                PlayedAtUtc =
                    DateTimeOffset.UtcNow,

                Participants =
                    new List<
                        CreateMatchParticipantRequest
                    >
                    {
                        new()
                        {
                            PlayerProfileId =
                                existingPlayer.Id,
                            Score = 1000,
                            Kills = 5,
                            Deaths = 2,
                            Result = "Win"
                        },
                        new()
                        {
                            PlayerProfileId =
                                missingPlayerId,
                            Score = 800,
                            Kills = 2,
                            Deaths = 5,
                            Result = "Loss"
                        }
                    }
            };

        var service =
            new MatchService(dbContext);

        var result =
            await service.CreateMatchAsync(
                request
            );

        Assert.False(
            result.IsSuccess
        );

        Assert.Equal(
            CreateMatchErrorCode.PlayerNotFound,
            result.ErrorCode
        );

        Assert.Single(
            result.MissingPlayerIds
        );

        Assert.Equal(
            missingPlayerId,
            result.MissingPlayerIds[0]
        );

        Assert.Equal(
            0,
            await dbContext.Matches.CountAsync()
        );
    }
}