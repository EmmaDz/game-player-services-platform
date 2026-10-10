namespace GamePlayer.Api.Dtos;

public class LeaderboardResponse
{
    public List<LeaderboardEntryResponse> Items { get; set; }
        = new List<LeaderboardEntryResponse>();

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }
}