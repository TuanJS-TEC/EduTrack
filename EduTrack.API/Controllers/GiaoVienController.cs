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
    [HttpGet]
    public async Task<ActionResult<List<GiaoVien>>> GetAll()
    {
        return Ok(await db.GiaoViens.AsNoTracking().OrderBy(x => x.HoTen).ToListAsync());
    }

    [HttpGet("{maGV}")]
    public async Task<ActionResult<GiaoVien>> GetById([FromRoute] string maGV)
    {
        var item = await db.GiaoViens.AsNoTracking().FirstOrDefaultAsync(x => x.MaGV == maGV);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Create([FromBody] GiaoVien input)
    {
        if (await db.GiaoViens.AnyAsync(x => x.MaGV == input.MaGV))
            return Conflict(new { message = "MaGV đã tồn tại" });

        db.GiaoViens.Add(input);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { maGV = input.MaGV }, input);
    }

    [HttpPut("{maGV}")]
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete([FromRoute] string maGV)
    {
        var item = await db.GiaoViens.FirstOrDefaultAsync(x => x.MaGV == maGV);
        if (item is null) return NotFound();

        db.GiaoViens.Remove(item);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

