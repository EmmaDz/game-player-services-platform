namespace GamePlayer.Api.Models;

public class PlayerProfile
{
    public Guid Id { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string? Bio { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public List<MatchParticipant> MatchParticipants { get; set; }
        = new List<MatchParticipant>();
}