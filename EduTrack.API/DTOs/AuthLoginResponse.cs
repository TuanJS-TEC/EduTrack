namespace EduTrack.API.DTOs;

public sealed class AuthLoginResponse
{
    public string UserId { get; set; } = "";
    public string Username { get; set; } = "";
    public List<string> Roles { get; set; } = [];
    public List<string> Permissions { get; set; } = [];
    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public int AccessTokenExpiresInSeconds { get; set; }
}

