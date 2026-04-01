using EduTrack.API.Data;
using EduTrack.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Controllers;

[ApiController]
[Route("api/hocsinh")]
[Authorize]
public sealed class HocSinhController(EduTrackDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<HocSinh>>> GetAll([FromQuery] string? maLop)
    {
        var query = db.HocSinhs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(maLop))
            query = query.Where(x => x.MaLop == maLop);

        return Ok(await query.OrderBy(x => x.HoTen).ToListAsync());
    }

    [HttpGet("{maHS}")]
    public async Task<ActionResult<HocSinh>> GetById([FromRoute] string maHS)
    {
        var hs = await db.HocSinhs.AsNoTracking().FirstOrDefaultAsync(x => x.MaHS == maHS);
        return hs is null ? NotFound() : Ok(hs);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult> Create([FromBody] HocSinh hs)
    {
        if (await db.HocSinhs.AnyAsync(x => x.MaHS == hs.MaHS))
            return Conflict(new { message = "MaHS đã tồn tại" });

        db.HocSinhs.Add(hs);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { maHS = hs.MaHS }, hs);
    }

    [HttpPut("{maHS}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult> Update([FromRoute] string maHS, [FromBody] HocSinh input)
    {
        var hs = await db.HocSinhs.FirstOrDefaultAsync(x => x.MaHS == maHS);
        if (hs is null) return NotFound();

        hs.HoTen = input.HoTen;
        hs.NgaySinh = input.NgaySinh;
        hs.DiaChi = input.DiaChi;
        hs.MaLop = input.MaLop;
        hs.Email_PhuHuynh = input.Email_PhuHuynh;
        hs.SDT_PhuHuynh = input.SDT_PhuHuynh;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{maHS}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete([FromRoute] string maHS)
    {
        var hs = await db.HocSinhs.FirstOrDefaultAsync(x => x.MaHS == maHS);
        if (hs is null) return NotFound();

        db.HocSinhs.Remove(hs);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

