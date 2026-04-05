namespace EduTrack.API.Services;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
}
