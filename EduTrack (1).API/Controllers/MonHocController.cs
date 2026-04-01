using EduTrack.API.Data;
using EduTrack.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Controllers;

[ApiController]
[Route("api/monhoc")]
[Authorize]
public sealed class MonHocController(EduTrackDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MonHoc>>> GetAll([FromQuery] string? maGV)
    {
        var q = db.MonHocs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(maGV)) q = q.Where(x => x.MaGV == maGV);
        return Ok(await q.OrderBy(x => x.TenMon).ToListAsync());
    }

    [HttpGet("{maMon}")]
    public async Task<ActionResult<MonHoc>> GetById([FromRoute] string maMon)
    {
        var item = await db.MonHocs.AsNoTracking().FirstOrDefaultAsync(x => x.MaMon == maMon);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult> Create([FromBody] MonHoc input)
    {
        if (await db.MonHocs.AnyAsync(x => x.MaMon == input.MaMon))
            return Conflict(new { message = "MaMon đã tồn tại" });

        db.MonHocs.Add(input);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { maMon = input.MaMon }, input);
    }

    [HttpPut("{maMon}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult> Update([FromRoute] string maMon, [FromBody] MonHoc input)
    {
        var item = await db.MonHocs.FirstOrDefaultAsync(x => x.MaMon == maMon);
        if (item is null) return NotFound();

        item.TenMon = input.TenMon;
        item.SoTiet = input.SoTiet;
        item.HeSoThi = input.HeSoThi;
        item.MaGV = input.MaGV;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{maMon}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete([FromRoute] string maMon)
    {
        var item = await db.MonHocs.FirstOrDefaultAsync(x => x.MaMon == maMon);
        if (item is null) return NotFound();

        db.MonHocs.Remove(item);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

