using EduTrack.API.Authorization;
using EduTrack.API.Data;
using EduTrack.API.Models;
using EduTrack.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Controllers;

[ApiController]
[Route("api/hocphi")]
[Authorize]
public sealed class HocPhiController(
    EduTrackDbContext db,
    ICurrentUserService current,
    IAccessControlService access) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AppPolicies.CanViewFinance)]
    public async Task<ActionResult<List<HocPhi>>> GetAll([FromQuery] string? maHS, [FromQuery] byte? hocKy, CancellationToken ct = default)
    {
        var q = db.HocPhis.AsNoTracking().AsQueryable();

        var userId = current.UserId;
        if (!string.IsNullOrEmpty(userId) && User.IsInRole(RolePermissionSeeder.Parent))
        {
            var codes = await access.GetParentStudentCodesAsync(userId, ct);
            if (codes.Count == 0)
                return Ok(new List<HocPhi>());
            q = q.Where(x => codes.Contains(x.MaHS));
        }

        if (!string.IsNullOrWhiteSpace(maHS)) q = q.Where(x => x.MaHS == maHS);
        if (hocKy.HasValue) q = q.Where(x => x.HocKy == hocKy.Value);

        return Ok(await q.OrderByDescending(x => x.NgayDong).ToListAsync(ct));
    }

    [HttpGet("{maHocPhi:int}")]
    [Authorize(Policy = AppPolicies.CanViewFinance)]
    public async Task<ActionResult<HocPhi>> GetById([FromRoute] int maHocPhi, CancellationToken ct = default)
    {
        var item = await db.HocPhis.AsNoTracking().FirstOrDefaultAsync(x => x.MaHocPhi == maHocPhi, ct);
        if (item is null) return NotFound();

        var userId = current.UserId;
        if (!string.IsNullOrEmpty(userId) && User.IsInRole(RolePermissionSeeder.Parent))
        {
            var codes = await access.GetParentStudentCodesAsync(userId, ct);
            if (!codes.Contains(item.MaHS)) return NotFound();
        }

        return Ok(item);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.CanManageFinance)]
    public async Task<ActionResult> Create([FromBody] HocPhi input)
    {
        db.HocPhis.Add(input);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { maHocPhi = input.MaHocPhi }, input);
    }

    [HttpPut("{maHocPhi:int}")]
    [Authorize(Policy = AppPolicies.CanManageFinance)]
    public async Task<ActionResult> Update([FromRoute] int maHocPhi, [FromBody] HocPhi input)
    {
        var item = await db.HocPhis.FirstOrDefaultAsync(x => x.MaHocPhi == maHocPhi);
        if (item is null) return NotFound();

        item.MaHS = input.MaHS;
        item.HocKy = input.HocKy;
        item.SoTien = input.SoTien;
        item.NgayDong = input.NgayDong;
        item.TrangThai = input.TrangThai;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{maHocPhi:int}")]
    [Authorize(Policy = AppPolicies.CanManageFinance)]
    public async Task<ActionResult> Delete([FromRoute] int maHocPhi)
    {
        var item = await db.HocPhis.FirstOrDefaultAsync(x => x.MaHocPhi == maHocPhi);
        if (item is null) return NotFound();

        db.HocPhis.Remove(item);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

