namespace EduTrack.API.DTOs;

public sealed class AuthTokenPairResponse
{
    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public int AccessTokenExpiresInSeconds { get; set; }
}
