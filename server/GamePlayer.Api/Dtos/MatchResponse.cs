namespace GamePlayer.Api.Dtos;

public class MatchResponse
{
    public Guid Id { get; set; }

    public string ExternalMatchId { get; set; } = string.Empty;

    public string GameMode { get; set; } = string.Empty;

    public DateTime PlayedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public List<MatchParticipantResponse> Participants { get; set; }
        = new List<MatchParticipantResponse>();
}