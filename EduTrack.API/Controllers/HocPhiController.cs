using EduTrack.API.Data;
using EduTrack.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Controllers;

[ApiController]
[Route("api/hocphi")]
[Authorize]
public sealed class HocPhiController(EduTrackDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<HocPhi>>> GetAll([FromQuery] string? maHS, [FromQuery] byte? hocKy)
    {
        var q = db.HocPhis.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(maHS)) q = q.Where(x => x.MaHS == maHS);
        if (hocKy.HasValue) q = q.Where(x => x.HocKy == hocKy.Value);

        return Ok(await q.OrderByDescending(x => x.NgayDong).ToListAsync());
    }

    [HttpGet("{maHocPhi:int}")]
    public async Task<ActionResult<HocPhi>> GetById([FromRoute] int maHocPhi)
    {
        var item = await db.HocPhis.AsNoTracking().FirstOrDefaultAsync(x => x.MaHocPhi == maHocPhi);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Create([FromBody] HocPhi input)
    {
        db.HocPhis.Add(input);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { maHocPhi = input.MaHocPhi }, input);
    }

    [HttpPut("{maHocPhi:int}")]
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete([FromRoute] int maHocPhi)
    {
        var item = await db.HocPhis.FirstOrDefaultAsync(x => x.MaHocPhi == maHocPhi);
        if (item is null) return NotFound();

        db.HocPhis.Remove(item);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

