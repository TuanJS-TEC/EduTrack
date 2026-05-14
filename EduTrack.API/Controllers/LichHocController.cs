using System.Security.Claims;
using EduTrack.API.Authorization;
using EduTrack.API.Data;
using EduTrack.API.DTOs;
using EduTrack.API.Models;
using EduTrack.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Controllers;

[ApiController]
[Route("api/lichhoc")]
[Authorize]
public sealed class LichHocController(
    EduTrackDbContext db,
    ICurrentUserService current,
    IAccessControlService access) : ControllerBase
{
    private bool IsElevatedScheduleViewer() =>
        User.IsInRole(RolePermissionSeeder.Admin) ||
        User.IsInRole(RolePermissionSeeder.Bgh) ||
        User.IsInRole(RolePermissionSeeder.Accountant) ||
        User.HasClaim("permission", AppPermissions.TeachersView);

    [HttpGet]
    [Authorize(Policy = AppPolicies.CanViewStudents)]
    public async Task<ActionResult<List<LichHocListItemDto>>> GetAll(
        [FromQuery] string? maLop,
        [FromQuery] string? maGV,
        CancellationToken ct)
    {
        IQueryable<LichHoc> q = db.LichHocs.AsNoTracking();

        if (IsElevatedScheduleViewer())
        {
            if (!string.IsNullOrWhiteSpace(maLop)) q = q.Where(x => x.MaLop == maLop);
            if (!string.IsNullOrWhiteSpace(maGV)) q = q.Where(x => x.MaGV == maGV);
        }
        else if (User.IsInRole(RolePermissionSeeder.Teacher))
        {
            var my = User.FindFirstValue("ma_gv");
            if (string.IsNullOrEmpty(my))
                return Ok(new List<LichHocListItemDto>());

            if (!string.IsNullOrWhiteSpace(maGV) &&
                !string.Equals(maGV.Trim(), my, StringComparison.Ordinal))
                return Forbid();

            if (!string.IsNullOrWhiteSpace(maLop))
            {
                var ml = maLop.Trim();
                var mayViewLop = await db.LopHocs.AsNoTracking()
                        .AnyAsync(l => l.MaLop == ml && l.MaGVChuNhiem == my, ct)
                    || await db.LichHocs.AsNoTracking()
                        .AnyAsync(l => l.MaLop == ml && l.MaGV == my, ct);
                if (!mayViewLop)
                    return Forbid();
                q = q.Where(x => x.MaLop == ml);
            }
            else if (!string.IsNullOrWhiteSpace(maGV))
                q = q.Where(x => x.MaGV == maGV!.Trim());
            else
                q = q.Where(x => x.MaGV == my);
        }
        else if (User.IsInRole(RolePermissionSeeder.Parent))
        {
            if (!string.IsNullOrWhiteSpace(maGV))
                return Forbid();

            var userId = current.UserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var codes = await access.GetParentStudentCodesAsync(userId, ct);
            var allowedLops = await db.HocSinhs.AsNoTracking()
                .Where(h => codes.Contains(h.MaHS))
                .Select(h => h.MaLop)
                .Distinct()
                .ToListAsync(ct);

            if (allowedLops.Count == 0)
                return Ok(new List<LichHocListItemDto>());

            q = q.Where(x => allowedLops.Contains(x.MaLop));
            if (!string.IsNullOrWhiteSpace(maLop))
            {
                var ml = maLop.Trim();
                if (!allowedLops.Contains(ml))
                    return Forbid();
                q = q.Where(x => x.MaLop == ml);
            }
        }
        else
            return Forbid();

        var list = await q
            .OrderBy(x => x.Thu)
            .ThenBy(x => x.TietBD)
            .Select(lh => new LichHocListItemDto
            {
                MaLich = lh.MaLich,
                MaMon = lh.MaMon,
                MaLop = lh.MaLop,
                MaGV = lh.MaGV,
                Thu = lh.Thu,
                TietBD = lh.TietBD,
                TietKT = lh.TietKT,
                Phong = lh.Phong,
                TenMon = lh.MonHoc != null ? lh.MonHoc.TenMon : null,
                TenLop = lh.LopHoc != null ? lh.LopHoc.TenLop : null,
                TenGV = lh.GiaoVien != null ? lh.GiaoVien.HoTen : null,
            })
            .ToListAsync(ct);

        return Ok(list);
    }

    [HttpGet("{maLich:int}")]
    [Authorize(Policy = AppPolicies.CanViewStudents)]
    public async Task<ActionResult<LichHoc>> GetById([FromRoute] int maLich, CancellationToken ct)
    {
        var item = await db.LichHocs.AsNoTracking().FirstOrDefaultAsync(x => x.MaLich == maLich, ct);
        if (item is null) return NotFound();

        if (!IsElevatedScheduleViewer())
        {
            if (User.IsInRole(RolePermissionSeeder.Parent))
            {
                var userId = current.UserId;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();
                var codes = await access.GetParentStudentCodesAsync(userId, ct);
                var ok = await db.HocSinhs.AsNoTracking()
                    .AnyAsync(h => codes.Contains(h.MaHS) && h.MaLop == item.MaLop, ct);
                if (!ok) return NotFound();
            }
            else if (User.IsInRole(RolePermissionSeeder.Teacher))
            {
                var my = User.FindFirstValue("ma_gv");
                if (string.IsNullOrEmpty(my)) return NotFound();
                var may = item.MaGV == my
                    || await db.LopHocs.AsNoTracking()
                        .AnyAsync(l => l.MaLop == item.MaLop && l.MaGVChuNhiem == my, ct);
                if (!may) return NotFound();
            }
            else
                return NotFound();
        }

        return Ok(item);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.CanConfigureSystem)]
    public async Task<ActionResult> Create([FromBody] LichHoc input)
    {
        db.LichHocs.Add(input);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { maLich = input.MaLich }, input);
    }

    [HttpPut("{maLich:int}")]
    [Authorize(Policy = AppPolicies.CanConfigureSystem)]
    public async Task<ActionResult> Update([FromRoute] int maLich, [FromBody] LichHoc input)
    {
        var item = await db.LichHocs.FirstOrDefaultAsync(x => x.MaLich == maLich);
        if (item is null) return NotFound();

        item.MaMon = input.MaMon;
        item.MaLop = input.MaLop;
        item.MaGV = input.MaGV;
        item.Thu = input.Thu;
        item.TietBD = input.TietBD;
        item.TietKT = input.TietKT;
        item.Phong = input.Phong;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{maLich:int}")]
    [Authorize(Policy = AppPolicies.CanConfigureSystem)]
    public async Task<ActionResult> Delete([FromRoute] int maLich)
    {
        var item = await db.LichHocs.FirstOrDefaultAsync(x => x.MaLich == maLich);
        if (item is null) return NotFound();

        db.LichHocs.Remove(item);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
