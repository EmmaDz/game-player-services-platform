namespace GamePlayer.Api.Dtos;

public class PlayerSummaryResponse
{
    public Guid Id { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}