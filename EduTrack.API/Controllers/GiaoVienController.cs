using System.Security.Claims;
using EduTrack.API.Authorization;
using EduTrack.API.Data;
using EduTrack.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Controllers;

[ApiController]
[Route("api/giaovien")]
[Authorize]
public sealed class GiaoVienController(EduTrackDbContext db) : ControllerBase
{
    /// <summary>Danh sách GV cho dropdown TKB: Admin/BGH/Kế toán hoặc có Teachers.View = toàn bộ; GV = chỉ bản thân; PH = rỗng.</summary>
    [HttpGet("for-schedule")]
    [Authorize(Policy = AppPolicies.CanViewStudents)]
    public async Task<ActionResult<List<GiaoVien>>> ForSchedulePicker(CancellationToken ct)
    {
        if (User.IsInRole(RolePermissionSeeder.Admin) ||
            User.IsInRole(RolePermissionSeeder.Bgh) ||
            User.IsInRole(RolePermissionSeeder.Accountant) ||
            User.HasClaim("permission", AppPermissions.TeachersView))
        {
            return Ok(await db.GiaoViens.AsNoTracking().OrderBy(x => x.HoTen).ToListAsync(ct));
        }

        if (User.IsInRole(RolePermissionSeeder.Parent))
            return Ok(new List<GiaoVien>());

        if (User.IsInRole(RolePermissionSeeder.Teacher))
        {
            var my = User.FindFirstValue("ma_gv");
            if (string.IsNullOrEmpty(my))
                return Ok(new List<GiaoVien>());
            var self = await db.GiaoViens.AsNoTracking().FirstOrDefaultAsync(x => x.MaGV == my, ct);
            return Ok(self is null ? new List<GiaoVien>() : new List<GiaoVien> { self });
        }

        return Ok(new List<GiaoVien>());
    }

    [HttpGet]
    [Authorize(Policy = AppPolicies.CanViewTeachers)]
    public async Task<ActionResult<List<GiaoVien>>> GetAll()
    {
        return Ok(await db.GiaoViens.AsNoTracking().OrderBy(x => x.HoTen).ToListAsync());
    }

    [HttpGet("{maGV}")]
    [Authorize(Policy = AppPolicies.CanViewTeachers)]
    public async Task<ActionResult<GiaoVien>> GetById([FromRoute] string maGV)
    {
        var item = await db.GiaoViens.AsNoTracking().FirstOrDefaultAsync(x => x.MaGV == maGV);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.CanManageTeachers)]
    public async Task<ActionResult> Create([FromBody] GiaoVien input)
    {
        if (await db.GiaoViens.AnyAsync(x => x.MaGV == input.MaGV))
            return Conflict(new { message = "MaGV đã tồn tại" });

        db.GiaoViens.Add(input);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { maGV = input.MaGV }, input);
    }

    [HttpPut("{maGV}")]
    [Authorize(Policy = AppPolicies.CanManageTeachers)]
    public async Task<ActionResult> Update([FromRoute] string maGV, [FromBody] GiaoVien input)
    {
        var item = await db.GiaoViens.FirstOrDefaultAsync(x => x.MaGV == maGV);
        if (item is null) return NotFound();

        item.HoTen = input.HoTen;
        item.ChuyenMon = input.ChuyenMon;
        item.Email = input.Email;
        item.LuongCoBan = input.LuongCoBan;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{maGV}")]
    [Authorize(Policy = AppPolicies.CanManageTeachers)]
    public async Task<ActionResult> Delete([FromRoute] string maGV)
    {
        var item = await db.GiaoViens.FirstOrDefaultAsync(x => x.MaGV == maGV);
        if (item is null) return NotFound();

        db.GiaoViens.Remove(item);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

