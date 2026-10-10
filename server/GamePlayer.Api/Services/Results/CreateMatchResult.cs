using GamePlayer.Api.Dtos;

namespace GamePlayer.Api.Services.Results;

public sealed class CreateMatchResult
{
    public bool IsSuccess { get; }

    public MatchResponse? Match { get; }

    public CreateMatchErrorCode? ErrorCode { get; }

    public string? ErrorMessage { get; }

    public List<Guid> MissingPlayerIds { get; }

    private CreateMatchResult(
        bool isSuccess,
        MatchResponse? match,
        CreateMatchErrorCode? errorCode,
        string? errorMessage,
        List<Guid>? missingPlayerIds
    )
    {
        IsSuccess = isSuccess;
        Match = match;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;

        MissingPlayerIds =
            missingPlayerIds ?? new List<Guid>();
    }

    public static CreateMatchResult Success(
        MatchResponse match
    )
    {
        return new CreateMatchResult(
            isSuccess: true,
            match: match,
            errorCode: null,
            errorMessage: null,
            missingPlayerIds: null
        );
    }

    public static CreateMatchResult Failure(
        CreateMatchErrorCode errorCode,
        string errorMessage,
        List<Guid>? missingPlayerIds = null
    )
    {
        return new CreateMatchResult(
            isSuccess: false,
            match: null,
            errorCode: errorCode,
            errorMessage: errorMessage,
            missingPlayerIds: missingPlayerIds
        );
    }
}