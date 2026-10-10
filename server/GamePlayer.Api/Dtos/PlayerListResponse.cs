namespace GamePlayer.Api.Dtos;

public class PlayerListResponse
{
    public List<PlayerSummaryResponse> Items { get; set; }
        = new List<PlayerSummaryResponse>();

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }
}