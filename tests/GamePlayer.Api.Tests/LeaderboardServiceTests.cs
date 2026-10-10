using GamePlayer.Api.Models;
using GamePlayer.Api.Services;
using GamePlayer.Api.Tests.Infrastructure;

namespace GamePlayer.Api.Tests;

public class LeaderboardServiceTests
{
    [Fact]
    public async Task GetLeaderboardAsync_RankedMatches_AggregatesAndOrdersCorrectly()
    {
        await using var testDatabase =
            await SqliteTestDatabase.CreateAsync();

        var dbContext =
            testDatabase.DbContext;

        var now = DateTime.UtcNow;

        var snowFoxId = Guid.NewGuid();
        var mapleKnightId = Guid.NewGuid();

        var snowFox = new PlayerProfile
        {
            Id = snowFoxId,
            DisplayName = "SnowFox",
            Region = "Canada",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var mapleKnight = new PlayerProfile
        {
            Id = mapleKnightId,
            DisplayName = "MapleKnight",
            Region = "Canada",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var rankedMatch1 = new Match
        {
            Id = Guid.NewGuid(),
            ExternalMatchId = "TEST-RANKED-001",
            GameMode = "Ranked",
            PlayedAtUtc = now.AddMinutes(-30),
            CreatedAtUtc = now
        };

        var rankedMatch2 = new Match
        {
            Id = Guid.NewGuid(),
            ExternalMatchId = "TEST-RANKED-002",
            GameMode = "Ranked",
            PlayedAtUtc = now.AddMinutes(-20),
            CreatedAtUtc = now
        };

        var casualMatch = new Match
        {
            Id = Guid.NewGuid(),
            ExternalMatchId = "TEST-CASUAL-001",
            GameMode = "Casual",
            PlayedAtUtc = now.AddMinutes(-10),
            CreatedAtUtc = now
        };

        dbContext.PlayerProfiles.AddRange(
            snowFox,
            mapleKnight
        );

        dbContext.Matches.AddRange(
            rankedMatch1,
            rankedMatch2,
            casualMatch
        );

        dbContext.MatchParticipants.AddRange(
            new MatchParticipant
            {
                Id = Guid.NewGuid(),
                MatchId = rankedMatch1.Id,
                PlayerProfileId = snowFoxId,
                Score = 1200,
                Kills = 8,
                Deaths = 3,
                Result = "Win"
            },
            new MatchParticipant
            {
                Id = Guid.NewGuid(),
                MatchId = rankedMatch1.Id,
                PlayerProfileId = mapleKnightId,
                Score = 950,
                Kills = 5,
                Deaths = 6,
                Result = "Loss"
            },
            new MatchParticipant
            {
                Id = Guid.NewGuid(),
                MatchId = rankedMatch2.Id,
                PlayerProfileId = snowFoxId,
                Score = 800,
                Kills = 4,
                Deaths = 5,
                Result = "Loss"
            },
            new MatchParticipant
            {
                Id = Guid.NewGuid(),
                MatchId = rankedMatch2.Id,
                PlayerProfileId = mapleKnightId,
                Score = 1100,
                Kills = 7,
                Deaths = 4,
                Result = "Win"
            },
            new MatchParticipant
            {
                Id = Guid.NewGuid(),
                MatchId = casualMatch.Id,
                PlayerProfileId = snowFoxId,
                Score = 10000,
                Kills = 50,
                Deaths = 0,
                Result = "Win"
            },
            new MatchParticipant
            {
                Id = Guid.NewGuid(),
                MatchId = casualMatch.Id,
                PlayerProfileId = mapleKnightId,
                Score = 10000,
                Kills = 50,
                Deaths = 0,
                Result = "Loss"
            }
        );

        await dbContext.SaveChangesAsync();

        var service =
            new LeaderboardService(dbContext);

        var response =
            await service.GetLeaderboardAsync(
                page: 1,
                pageSize: 10
            );

        Assert.Equal(
            2,
            response.TotalCount
        );

        Assert.Equal(
            1,
            response.TotalPages
        );

        Assert.Equal(
            2,
            response.Items.Count
        );

        var first = response.Items[0];

        Assert.Equal(
            mapleKnightId,
            first.PlayerId
        );

        Assert.Equal(
            "MapleKnight",
            first.DisplayName
        );

        Assert.Equal(
            2,
            first.MatchesPlayed
        );

        Assert.Equal(
            1,
            first.Wins
        );

        Assert.Equal(
            2050,
            first.TotalScore
        );

        Assert.Equal(
            12,
            first.TotalKills
        );

        Assert.Equal(
            10,
            first.TotalDeaths
        );

        var second = response.Items[1];

        Assert.Equal(
            snowFoxId,
            second.PlayerId
        );

        Assert.Equal(
            "SnowFox",
            second.DisplayName
        );

        Assert.Equal(
            2,
            second.MatchesPlayed
        );

        Assert.Equal(
            1,
            second.Wins
        );

        Assert.Equal(
            2000,
            second.TotalScore
        );

        Assert.Equal(
            12,
            second.TotalKills
        );

        Assert.Equal(
            8,
            second.TotalDeaths
        );
    }

    [Fact]
    public async Task GetLeaderboardAsync_OnlyCasualMatches_ReturnsEmptyLeaderboard()
    {
        await using var testDatabase =
            await SqliteTestDatabase.CreateAsync();

        var dbContext =
            testDatabase.DbContext;

        var now = DateTime.UtcNow;

        var player1 = new PlayerProfile
        {
            Id = Guid.NewGuid(),
            DisplayName = "CasualPlayerOne",
            Region = "Canada",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var player2 = new PlayerProfile
        {
            Id = Guid.NewGuid(),
            DisplayName = "CasualPlayerTwo",
            Region = "Canada",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var casualMatch = new Match
        {
            Id = Guid.NewGuid(),
            ExternalMatchId = "TEST-CASUAL-ONLY",
            GameMode = "Casual",
            PlayedAtUtc = now,
            CreatedAtUtc = now
        };

        dbContext.PlayerProfiles.AddRange(
            player1,
            player2
        );

        dbContext.Matches.Add(casualMatch);

        dbContext.MatchParticipants.AddRange(
            new MatchParticipant
            {
                Id = Guid.NewGuid(),
                MatchId = casualMatch.Id,
                PlayerProfileId = player1.Id,
                Score = 5000,
                Kills = 20,
                Deaths = 1,
                Result = "Win"
            },
            new MatchParticipant
            {
                Id = Guid.NewGuid(),
                MatchId = casualMatch.Id,
                PlayerProfileId = player2.Id,
                Score = 4000,
                Kills = 10,
                Deaths = 5,
                Result = "Loss"
            }
        );

        await dbContext.SaveChangesAsync();

        var service =
            new LeaderboardService(dbContext);

        var response =
            await service.GetLeaderboardAsync(
                page: 1,
                pageSize: 10
            );

        Assert.Empty(
            response.Items
        );

        Assert.Equal(
            0,
            response.TotalCount
        );

        Assert.Equal(
            0,
            response.TotalPages
        );
    }

    [Fact]
    public async Task GetLeaderboardAsync_SecondPage_ReturnsCorrectPlayer()
    {
        await using var testDatabase =
            await SqliteTestDatabase.CreateAsync();

        var dbContext =
            testDatabase.DbContext;

        var now = DateTime.UtcNow;

        var alpha = new PlayerProfile
        {
            Id = Guid.NewGuid(),
            DisplayName = "AlphaPlayer",
            Region = "Canada",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var bravo = new PlayerProfile
        {
            Id = Guid.NewGuid(),
            DisplayName = "BravoPlayer",
            Region = "Canada",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var charlie = new PlayerProfile
        {
            Id = Guid.NewGuid(),
            DisplayName = "CharliePlayer",
            Region = "Canada",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var rankedMatch = new Match
        {
            Id = Guid.NewGuid(),
            ExternalMatchId = "TEST-PAGINATION-001",
            GameMode = "Ranked",
            PlayedAtUtc = now,
            CreatedAtUtc = now
        };

        dbContext.PlayerProfiles.AddRange(
            alpha,
            bravo,
            charlie
        );

        dbContext.Matches.Add(
            rankedMatch
        );

        dbContext.MatchParticipants.AddRange(
            new MatchParticipant
            {
                Id = Guid.NewGuid(),
                MatchId = rankedMatch.Id,
                PlayerProfileId = alpha.Id,
                Score = 3000,
                Kills = 10,
                Deaths = 1,
                Result = "Win"
            },
            new MatchParticipant
            {
                Id = Guid.NewGuid(),
                MatchId = rankedMatch.Id,
                PlayerProfileId = bravo.Id,
                Score = 2000,
                Kills = 7,
                Deaths = 3,
                Result = "Loss"
            },
            new MatchParticipant
            {
                Id = Guid.NewGuid(),
                MatchId = rankedMatch.Id,
                PlayerProfileId = charlie.Id,
                Score = 1000,
                Kills = 3,
                Deaths = 7,
                Result = "Loss"
            }
        );

        await dbContext.SaveChangesAsync();

        var service =
            new LeaderboardService(dbContext);

        var response =
            await service.GetLeaderboardAsync(
                page: 2,
                pageSize: 1
            );

        Assert.Equal(
            3,
            response.TotalCount
        );

        Assert.Equal(
            3,
            response.TotalPages
        );

        Assert.Single(
            response.Items
        );

        var item =
            response.Items[0];

        Assert.Equal(
            bravo.Id,
            item.PlayerId
        );

        Assert.Equal(
            "BravoPlayer",
            item.DisplayName
        );

        Assert.Equal(
            2000,
            item.TotalScore
        );

        Assert.Equal(
            2,
            response.Page
        );

        Assert.Equal(
            1,
            response.PageSize
        );
    }

    [Fact]
    public async Task GetLeaderboardAsync_TiedScores_UsesWinsThenDisplayName()
    {
        await using var testDatabase =
            await SqliteTestDatabase.CreateAsync();

        var dbContext =
            testDatabase.DbContext;

        var now = DateTime.UtcNow;

        var alpha = new PlayerProfile
        {
            Id = Guid.NewGuid(),
            DisplayName = "AlphaPlayer",
            Region = "Canada",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var bravo = new PlayerProfile
        {
            Id = Guid.NewGuid(),
            DisplayName = "BravoPlayer",
            Region = "Canada",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var charlie = new PlayerProfile
        {
            Id = Guid.NewGuid(),
            DisplayName = "CharliePlayer",
            Region = "Canada",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var trainingBot = new PlayerProfile
        {
            Id = Guid.NewGuid(),
            DisplayName = "TrainingBot",
            Region = "Canada",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        dbContext.PlayerProfiles.AddRange(
            alpha,
            bravo,
            charlie,
            trainingBot
        );

        var alphaMatch = new Match
        {
            Id = Guid.NewGuid(),
            ExternalMatchId = "TEST-TIE-ALPHA",
            GameMode = "Ranked",
            PlayedAtUtc = now.AddMinutes(-40),
            CreatedAtUtc = now
        };

        var bravoMatch1 = new Match
        {
            Id = Guid.NewGuid(),
            ExternalMatchId = "TEST-TIE-BRAVO-001",
            GameMode = "Ranked",
            PlayedAtUtc = now.AddMinutes(-30),
            CreatedAtUtc = now
        };

        var bravoMatch2 = new Match
        {
            Id = Guid.NewGuid(),
            ExternalMatchId = "TEST-TIE-BRAVO-002",
            GameMode = "Ranked",
            PlayedAtUtc = now.AddMinutes(-20),
            CreatedAtUtc = now
        };

        var charlieMatch = new Match
        {
            Id = Guid.NewGuid(),
            ExternalMatchId = "TEST-TIE-CHARLIE",
            GameMode = "Ranked",
            PlayedAtUtc = now.AddMinutes(-10),
            CreatedAtUtc = now
        };

        dbContext.Matches.AddRange(
            alphaMatch,
            bravoMatch1,
            bravoMatch2,
            charlieMatch
        );

        dbContext.MatchParticipants.AddRange(
            new MatchParticipant
            {
                Id = Guid.NewGuid(),
                MatchId = alphaMatch.Id,
                PlayerProfileId = alpha.Id,
                Score = 2000,
                Kills = 8,
                Deaths = 2,
                Result = "Win"
            },
            new MatchParticipant
            {
                Id = Guid.NewGuid(),
                MatchId = alphaMatch.Id,
                PlayerProfileId = trainingBot.Id,
                Score = 10,
                Kills = 0,
                Deaths = 8,
                Result = "Loss"
            },

            new MatchParticipant
            {
                Id = Guid.NewGuid(),
                MatchId = bravoMatch1.Id,
                PlayerProfileId = bravo.Id,
                Score = 1000,
                Kills = 5,
                Deaths = 2,
                Result = "Win"
            },
            new MatchParticipant
            {
                Id = Guid.NewGuid(),
                MatchId = bravoMatch1.Id,
                PlayerProfileId = trainingBot.Id,
                Score = 10,
                Kills = 0,
                Deaths = 5,
                Result = "Loss"
            },

            new MatchParticipant
            {
                Id = Guid.NewGuid(),
                MatchId = bravoMatch2.Id,
                PlayerProfileId = bravo.Id,
                Score = 1000,
                Kills = 6,
                Deaths = 1,
                Result = "Win"
            },
            new MatchParticipant
            {
                Id = Guid.NewGuid(),
                MatchId = bravoMatch2.Id,
                PlayerProfileId = trainingBot.Id,
                Score = 10,
                Kills = 0,
                Deaths = 6,
                Result = "Loss"
            },

            new MatchParticipant
            {
                Id = Guid.NewGuid(),
                MatchId = charlieMatch.Id,
                PlayerProfileId = charlie.Id,
                Score = 2000,
                Kills = 7,
                Deaths = 3,
                Result = "Win"
            },
            new MatchParticipant
            {
                Id = Guid.NewGuid(),
                MatchId = charlieMatch.Id,
                PlayerProfileId = trainingBot.Id,
                Score = 10,
                Kills = 0,
                Deaths = 7,
                Result = "Loss"
            }
        );

        await dbContext.SaveChangesAsync();

        var service =
            new LeaderboardService(dbContext);

        var response =
            await service.GetLeaderboardAsync(
                page: 1,
                pageSize: 10
            );

        Assert.Equal(
            4,
            response.Items.Count
        );

        Assert.Equal(
            "BravoPlayer",
            response.Items[0].DisplayName
        );

        Assert.Equal(
            2,
            response.Items[0].Wins
        );

        Assert.Equal(
            "AlphaPlayer",
            response.Items[1].DisplayName
        );

        Assert.Equal(
            1,
            response.Items[1].Wins
        );

        Assert.Equal(
            "CharliePlayer",
            response.Items[2].DisplayName
        );

        Assert.Equal(
            1,
            response.Items[2].Wins
        );

        Assert.Equal(
            "TrainingBot",
            response.Items[3].DisplayName
        );
    }
}