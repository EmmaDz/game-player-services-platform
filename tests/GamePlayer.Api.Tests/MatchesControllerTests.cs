using GamePlayer.Api.Controllers;
using GamePlayer.Api.Dtos;
using GamePlayer.Api.Services;
using GamePlayer.Api.Services.Results;
using Microsoft.AspNetCore.Mvc;

namespace GamePlayer.Api.Tests;

public class FakeMatchService : IMatchService
{
    public MatchResponse? MatchToReturn { get; set; }

    public CreateMatchResult CreateResultToReturn { get; set; }
        = CreateMatchResult.Failure(
            CreateMatchErrorCode.InvalidGameMode,
            "Fake result was not configured."
        );

    public Guid? ReceivedGetMatchId { get; private set; }

    public CreateMatchRequest? ReceivedCreateRequest
    {
        get;
        private set;
    }

    public Task<MatchResponse?> GetMatchByIdAsync(
        Guid id
    )
    {
        ReceivedGetMatchId = id;

        return Task.FromResult(
            MatchToReturn
        );
    }

    public Task<CreateMatchResult> CreateMatchAsync(
        CreateMatchRequest request
    )
    {
        ReceivedCreateRequest = request;

        return Task.FromResult(
            CreateResultToReturn
        );
    }
}

public class MatchesControllerTests
{
    [Fact]
    public async Task CreateMatch_ServiceReturnsSuccess_ReturnsCreatedAtAction()
    {
        var fakeService =
            new FakeMatchService();

        var createdMatch =
            new MatchResponse
            {
                Id = Guid.NewGuid(),
                ExternalMatchId =
                    "TEST-CONTROLLER-001"
            };

        fakeService.CreateResultToReturn =
            CreateMatchResult.Success(
                createdMatch
            );

        var controller =
            new MatchesController(
                fakeService
            );

        var request =
            new CreateMatchRequest
            {
                ExternalMatchId =
                    "TEST-CONTROLLER-001",

                GameMode =
                    "Ranked"
            };

        var result =
            await controller.CreateMatch(
                request
            );

        var createdResult =
            Assert.IsType<CreatedAtActionResult>(
                result.Result
            );

        Assert.Same(
            createdMatch,
            createdResult.Value
        );
    }

    [Fact]
    public async Task CreateMatch_DuplicateExternalMatchId_ReturnsConflict()
    {
        var fakeService =
            new FakeMatchService();

        fakeService.CreateResultToReturn =
            CreateMatchResult.Failure(
                CreateMatchErrorCode
                    .DuplicateExternalMatchId,
                "Match already exists."
            );

        var controller =
            new MatchesController(
                fakeService
            );

        var request =
            new CreateMatchRequest();

        var result =
            await controller.CreateMatch(
                request
            );

        Assert.IsType<ConflictObjectResult>(
            result.Result
        );
    }

    [Fact]
    public async Task CreateMatch_PlayerNotFound_ReturnsBadRequest()
    {
        var fakeService =
            new FakeMatchService();

        var missingPlayerId =
            Guid.NewGuid();

        fakeService.CreateResultToReturn =
            CreateMatchResult.Failure(
                CreateMatchErrorCode
                    .PlayerNotFound,
                "One or more players do not exist.",
                new List<Guid>
                {
                    missingPlayerId
                }
            );

        var controller =
            new MatchesController(
                fakeService
            );

        var request =
            new CreateMatchRequest();

        var result =
            await controller.CreateMatch(
                request
            );

        Assert.IsType<BadRequestObjectResult>(
            result.Result
        );
    }
}