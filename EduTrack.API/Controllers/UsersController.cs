using EduTrack.API.Authorization;
using EduTrack.API.Data;
using EduTrack.API.DTOs;
using EduTrack.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = AppPolicies.CanManageUsers)]
public sealed class UsersController(UserManager<ApplicationUser> userManager, EduTrackDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AdminUserResponse>>> List(CancellationToken ct)
    {
        var users = await userManager.Users.AsNoTracking().OrderBy(u => u.UserName).ToListAsync(ct);
        var result = new List<AdminUserResponse>();
        foreach (var u in users)
        {
            var roles = await userManager.GetRolesAsync(u);
            result.Add(new AdminUserResponse
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                HoTen = u.HoTen,
                MaGV = u.MaGV,
                Roles = roles.OrderBy(r => r).ToList()
            });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AdminUserResponse>> Create([FromBody] AdminUserCreateRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Username/Password bắt buộc");

        var user = new ApplicationUser
        {
            UserName = req.Username.Trim(),
            Email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim(),
            HoTen = req.HoTen,
            MaGV = string.IsNullOrWhiteSpace(req.MaGV) ? null : req.MaGV.Trim()
        };

        var res = await userManager.CreateAsync(user, req.Password);
        if (!res.Succeeded)
            return BadRequest(res.Errors.Select(e => e.Description));

        foreach (var r in req.Roles.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (await userManager.IsInRoleAsync(user, r)) continue;
            await userManager.AddToRoleAsync(user, r);
        }

        var roles = await userManager.GetRolesAsync(user);
        var dto = new AdminUserResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            HoTen = user.HoTen,
            MaGV = user.MaGV,
            Roles = roles.OrderBy(x => x).ToList()
        };
        return Created($"/api/users/{user.Id}", dto);
    }

    [HttpPost("{userId}/roles")]
    public async Task<ActionResult> SetRoles([FromRoute] string userId, [FromBody] List<string> roles, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        var current = await userManager.GetRolesAsync(user);
        await userManager.RemoveFromRolesAsync(user, current);
        foreach (var r in roles.Distinct(StringComparer.OrdinalIgnoreCase))
            await userManager.AddToRoleAsync(user, r);

        return NoContent();
    }

    [HttpPost("{userId}/parent-link")]
    public async Task<ActionResult> LinkParentStudent([FromRoute] string userId, [FromBody] ParentLinkRequest body, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();
        if (!await userManager.IsInRoleAsync(user, RolePermissionSeeder.Parent))
            return BadRequest("User không phải phụ huynh");

        var maHS = body.MaHS.Trim();
        if (!await db.HocSinhs.AnyAsync(h => h.MaHS == maHS, ct))
            return BadRequest("MaHS không tồn tại");

        if (await db.ParentStudentLinks.AnyAsync(x => x.UserId == userId && x.MaHS == maHS, ct))
            return NoContent();

        db.ParentStudentLinks.Add(new ParentStudentLink { UserId = userId, MaHS = maHS });
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
