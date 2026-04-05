using System.Security.Claims;

namespace EduTrack.API.Services;

public sealed class CurrentUserService(IHttpContextAccessor http) : ICurrentUserService
{
    public string? UserId => http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName => http.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
}
