namespace GamePlayer.Api.Dtos;

public class LeaderboardEntryResponse
{
    public Guid PlayerId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public int MatchesPlayed { get; set; }

    public int Wins { get; set; }

    public int TotalScore { get; set; }

    public int TotalKills { get; set; }

    public int TotalDeaths { get; set; }
}