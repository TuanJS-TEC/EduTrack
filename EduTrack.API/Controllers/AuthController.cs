using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EduTrack.API.Authorization;
using EduTrack.API.Data;
using EduTrack.API.DTOs;
using EduTrack.API.Helpers;
using EduTrack.API.Models;
using EduTrack.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EduTrack.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService,
    EduTrackDbContext db,
    IOptions<JwtOptions> jwtOptions) : ControllerBase
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthLoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return ProblemResponses.Of(StatusCodes.Status400BadRequest, "Thiếu thông tin đăng nhập", ApiErrorCodes.Validation);

        var user = await userManager.FindByNameAsync(request.Username.Trim());
        if (user is null)
            return ProblemResponses.Of(StatusCodes.Status401Unauthorized, "Sai tài khoản hoặc mật khẩu", ApiErrorCodes.InvalidCredentials);

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
            return ProblemResponses.Of(StatusCodes.Status423Locked, "Tài khoản đang bị khóa tạm thời");

        if (!result.Succeeded)
            return ProblemResponses.Of(StatusCodes.Status401Unauthorized, "Sai tài khoản hoặc mật khẩu", ApiErrorCodes.InvalidCredentials);

        var roles = await userManager.GetRolesAsync(user);
        var permissions = RolePermissionSeeder.PermissionsForRoles(roles).ToList();
        var access = tokenService.CreateAccessToken(BuildClaims(user, roles, permissions));
        var refresh = tokenService.CreateRefreshToken();
        await StoreRefreshTokenAsync(user.Id, refresh, ct);

        return Ok(new AuthLoginResponse
        {
            UserId = user.Id,
            Username = user.UserName ?? "",
            MaGV = user.MaGV,
            HoTen = user.HoTen,
            Roles = roles.OrderBy(r => r).ToList(),
            Permissions = permissions.OrderBy(p => p).ToList(),
            AccessToken = access,
            RefreshToken = refresh,
            AccessTokenExpiresInSeconds = _jwt.AccessTokenMinutes * 60
        });
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] AuthRefreshRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return ProblemResponses.Of(StatusCodes.Status400BadRequest, "Thiếu refresh token", ApiErrorCodes.Validation);

        var hash = HashToken(request.RefreshToken);
        var existing = await db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, ct);
        if (existing is null || existing.RevokedAtUtc is not null || existing.ExpiresAtUtc < DateTime.UtcNow)
            return ProblemResponses.Of(StatusCodes.Status401Unauthorized, "Refresh token không hợp lệ", ApiErrorCodes.InvalidRefreshToken);

        var user = await userManager.FindByIdAsync(existing.UserId);
        if (user is null)
            return ProblemResponses.Of(StatusCodes.Status401Unauthorized, "Refresh token không hợp lệ", ApiErrorCodes.InvalidRefreshToken);

        var newRefresh = tokenService.CreateRefreshToken();
        existing.RevokedAtUtc = DateTime.UtcNow;
        existing.ReplacedByTokenHash = HashToken(newRefresh);
        await db.SaveChangesAsync(ct);
        await StoreRefreshTokenAsync(user.Id, newRefresh, ct);

        var roles = await userManager.GetRolesAsync(user);
        var permissions = RolePermissionSeeder.PermissionsForRoles(roles).ToList();
        var access = tokenService.CreateAccessToken(BuildClaims(user, roles, permissions));

        return Ok(new AuthTokenPairResponse
        {
            AccessToken = access,
            RefreshToken = newRefresh,
            AccessTokenExpiresInSeconds = _jwt.AccessTokenMinutes * 60
        });
    }

    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var active = await db.RefreshTokens.Where(x => x.UserId == userId && x.RevokedAtUtc == null).ToListAsync(ct);
        var now = DateTime.UtcNow;
        foreach (var t in active) t.RevokedAtUtc = now;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task StoreRefreshTokenAsync(string userId, string rawToken, CancellationToken ct)
    {
        var entity = new RefreshToken
        {
            UserId = userId,
            TokenHash = HashToken(rawToken),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays)
        };
        db.RefreshTokens.Add(entity);
        await db.SaveChangesAsync(ct);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    private static IEnumerable<Claim> BuildClaims(ApplicationUser user, IList<string> roles, IReadOnlyList<string> permissions)
    {
        yield return new Claim(ClaimTypes.NameIdentifier, user.Id);
        if (!string.IsNullOrEmpty(user.UserName)) yield return new Claim(ClaimTypes.Name, user.UserName);
        if (!string.IsNullOrEmpty(user.MaGV)) yield return new Claim("ma_gv", user.MaGV);
        foreach (var r in roles) yield return new Claim(ClaimTypes.Role, r);
        foreach (var p in permissions) yield return new Claim("permission", p);
    }
}
