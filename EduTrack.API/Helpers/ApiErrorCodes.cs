namespace EduTrack.API.Helpers;

public static class ApiErrorCodes
{
    public const string Validation = "VALIDATION_ERROR";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string NotFound = "NOT_FOUND";
    public const string Conflict = "CONFLICT";

    public const string ScoreOutOfRange = "SCORE_OUT_OF_RANGE";
    public const string SemesterLocked = "SEMESTER_LOCKED";
    public const string InvalidRefreshToken = "INVALID_REFRESH_TOKEN";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
}
