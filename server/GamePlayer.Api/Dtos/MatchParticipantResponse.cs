namespace GamePlayer.Api.Dtos;

public class MatchParticipantResponse
{
    public Guid Id { get; set; }

    public Guid PlayerProfileId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public int Score { get; set; }

    public int Kills { get; set; }

    public int Deaths { get; set; }

    public string Result { get; set; } = string.Empty;
}