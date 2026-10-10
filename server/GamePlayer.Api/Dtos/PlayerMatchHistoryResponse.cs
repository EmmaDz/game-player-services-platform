namespace GamePlayer.Api.Dtos;

public class PlayerMatchHistoryResponse
{
    public Guid PlayerId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public List<PlayerMatchSummaryResponse> Items { get; set; }
        = new List<PlayerMatchSummaryResponse>();

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }
}