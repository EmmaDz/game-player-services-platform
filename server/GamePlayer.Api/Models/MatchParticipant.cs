namespace GamePlayer.Api.Models;

public class MatchParticipant
{
    public Guid Id { get; set; }

    public Guid MatchId { get; set; }

    public Guid PlayerProfileId { get; set; }

    public int Score { get; set; }

    public int Kills { get; set; }

    public int Deaths { get; set; }

    public string Result { get; set; } = string.Empty;

    public Match Match { get; set; } = null!;

    public PlayerProfile PlayerProfile { get; set; } = null!;
}