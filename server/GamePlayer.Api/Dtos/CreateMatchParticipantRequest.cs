using System.ComponentModel.DataAnnotations;

namespace GamePlayer.Api.Dtos;

public class CreateMatchParticipantRequest
{
    public Guid PlayerProfileId { get; set; }

    [Range(0, int.MaxValue)]
    public int Score { get; set; }

    [Range(0, int.MaxValue)]
    public int Kills { get; set; }

    [Range(0, int.MaxValue)]
    public int Deaths { get; set; }

    [Required]
    [StringLength(10)]
    public string Result { get; set; } = string.Empty;
}