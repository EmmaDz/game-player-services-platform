namespace GamePlayer.Api.Services.Results;

public enum CreateMatchErrorCode
{
    InvalidExternalMatchId,
    InvalidGameMode,
    DuplicateExternalMatchId,
    InvalidPlayerId,
    DuplicateParticipant,
    InvalidResult,
    PlayerNotFound
}