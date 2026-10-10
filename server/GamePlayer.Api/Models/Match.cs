namespace GamePlayer.Api.Models;

public class Match
{
    public Guid Id { get; set; }

    public string ExternalMatchId { get; set; } = string.Empty;

    public string GameMode { get; set; } = string.Empty;

    public DateTime PlayedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public List<MatchParticipant> Participants { get; set; }
        = new List<MatchParticipant>();
}