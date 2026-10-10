using System.ComponentModel.DataAnnotations;

namespace GamePlayer.Api.Dtos;

public class CreateMatchRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string ExternalMatchId { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string GameMode { get; set; } = string.Empty;

    public DateTimeOffset PlayedAtUtc { get; set; }

    [Required]
    [MinLength(2)]
    public List<CreateMatchParticipantRequest> Participants { get; set; }
        = new List<CreateMatchParticipantRequest>();
}