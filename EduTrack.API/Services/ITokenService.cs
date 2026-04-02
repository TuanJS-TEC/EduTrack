using System.Security.Claims;

namespace EduTrack.API.Services;

public interface ITokenService
{
    string CreateAccessToken(IEnumerable<Claim> claims);
    string CreateRefreshToken();
}

