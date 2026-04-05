namespace EduTrack.API.DTOs;

public sealed class AdminUserCreateRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string? Email { get; set; }
    public string? HoTen { get; set; }
    public string? MaGV { get; set; }
    public List<string> Roles { get; set; } = [];
}

public sealed class ParentLinkRequest
{
    public string MaHS { get; set; } = "";
}

public sealed class AdminUserResponse
{
    public string Id { get; set; } = "";
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? HoTen { get; set; }
    public string? MaGV { get; set; }
    public List<string> Roles { get; set; } = [];
}
