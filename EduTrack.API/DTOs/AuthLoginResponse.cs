namespace EduTrack.API.DTOs;

public sealed class AuthLoginResponse
{
    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public string Username { get; set; } = "";
    public string Role { get; set; } = "";
}

