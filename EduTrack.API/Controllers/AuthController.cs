using System.Security.Claims;
using EduTrack.API.DTOs;
using EduTrack.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ITokenService tokenService) : ControllerBase
{
    // Skeleton login: thay bằng truy vấn SQL Server + hash mật khẩu sau
    [HttpPost("login")]
    public ActionResult<AuthLoginResponse> Login([FromBody] AuthLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return Unauthorized();

        var role = request.Username.Equals("admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Teacher";

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, request.Username),
            new Claim(ClaimTypes.Role, role),
        };

        return Ok(new AuthLoginResponse
        {
            Username = request.Username,
            Role = role,
            AccessToken = tokenService.CreateAccessToken(claims),
            RefreshToken = tokenService.CreateRefreshToken(),
        });
    }
}

