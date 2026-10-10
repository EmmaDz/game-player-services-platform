namespace GamePlayer.Api.Dtos;

public class PlayerMatchSummaryResponse
{
    public Guid MatchId { get; set; }

    public string ExternalMatchId { get; set; } = string.Empty;

    public string GameMode { get; set; } = string.Empty;

    public DateTime PlayedAtUtc { get; set; }

    public int Score { get; set; }

    public int Kills { get; set; }

    public int Deaths { get; set; }

    public string Result { get; set; } = string.Empty;
}