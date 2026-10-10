using GamePlayer.Api.Controllers;
using GamePlayer.Api.Dtos;
using GamePlayer.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GamePlayer.Api.Tests;

public class FakeLeaderboardService : ILeaderboardService
{
    public LeaderboardResponse ResponseToReturn { get; set; }
        = new LeaderboardResponse();

    public int? ReceivedPage { get; private set; }

    public int? ReceivedPageSize { get; private set; }

    public Task<LeaderboardResponse> GetLeaderboardAsync(
        int page,
        int pageSize
    )
    {
        ReceivedPage = page;
        ReceivedPageSize = pageSize;

        return Task.FromResult(ResponseToReturn);
    }
}

public class LeaderboardControllerTests
{
    [Fact]
    public async Task GetLeaderboard_ValidRequest_ReturnsOk()
    {
        var fakeService =
            new FakeLeaderboardService();

        fakeService.ResponseToReturn =
            new LeaderboardResponse
            {
                Items =
                    new List<LeaderboardEntryResponse>(),

                Page = 1,
                PageSize = 10,
                TotalCount = 2,
                TotalPages = 1
            };

        var controller =
            new LeaderboardController(fakeService);

        var result =
            await controller.GetLeaderboard(
                page: 1,
                pageSize: 10
            );

        var okResult =
            Assert.IsType<OkObjectResult>(
                result.Result
            );

        var response =
            Assert.IsType<LeaderboardResponse>(
                okResult.Value
            );

        Assert.Equal(
            1,
            response.Page
        );

        Assert.Equal(
            10,
            response.PageSize
        );

        Assert.Equal(
            2,
            response.TotalCount
        );

        Assert.Equal(
            1,
            fakeService.ReceivedPage
        );

        Assert.Equal(
            10,
            fakeService.ReceivedPageSize
        );
    }

    [Fact]
    public async Task GetLeaderboard_PageZero_ReturnsBadRequest()
    {
        var fakeService =
            new FakeLeaderboardService();

        var controller =
            new LeaderboardController(fakeService);

        var result =
            await controller.GetLeaderboard(
                page: 0,
                pageSize: 10
            );

        Assert.IsType<BadRequestObjectResult>(
            result.Result
        );

        Assert.Null(
            fakeService.ReceivedPage
        );

        Assert.Null(
            fakeService.ReceivedPageSize
        );
    }

    [Fact]
    public async Task GetLeaderboard_PageSizeTooLarge_ReturnsBadRequest()
    {
        var fakeService =
            new FakeLeaderboardService();

        var controller =
            new LeaderboardController(fakeService);

        var result =
            await controller.GetLeaderboard(
                page: 1,
                pageSize: 101
            );

        Assert.IsType<BadRequestObjectResult>(
            result.Result
        );

        Assert.Null(
            fakeService.ReceivedPage
        );
    }
}