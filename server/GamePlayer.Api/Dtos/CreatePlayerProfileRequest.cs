using System.ComponentModel.DataAnnotations;

namespace GamePlayer.Api.Dtos;

public class CreatePlayerProfileRequest
{
    [Required]
    [StringLength(30, MinimumLength = 3)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [StringLength(30, MinimumLength = 2)]
    public string Region { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Bio { get; set; }
}