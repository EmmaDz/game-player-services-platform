namespace GamePlayer.Api.Dtos;

public class PlayerProfileResponse
{
    public Guid Id { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string? Bio { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}