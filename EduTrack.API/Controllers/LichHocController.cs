using EduTrack.API.Data;
using EduTrack.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Controllers;

[ApiController]
[Route("api/lichhoc")]
[Authorize]
public sealed class LichHocController(EduTrackDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<LichHoc>>> GetAll([FromQuery] string? maLop)
    {
        var q = db.LichHocs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(maLop)) q = q.Where(x => x.MaLop == maLop);
        return Ok(await q.OrderBy(x => x.Thu).ThenBy(x => x.TietBD).ToListAsync());
    }

    [HttpGet("{maLich:int}")]
    public async Task<ActionResult<LichHoc>> GetById([FromRoute] int maLich)
    {
        var item = await db.LichHocs.AsNoTracking().FirstOrDefaultAsync(x => x.MaLich == maLich);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult> Create([FromBody] LichHoc input)
    {
        db.LichHocs.Add(input);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { maLich = input.MaLich }, input);
    }

    [HttpPut("{maLich:int}")]
    [Authorize(Roles = "Admin,Teacher")]
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
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult> Delete([FromRoute] int maLich)
    {
        var item = await db.LichHocs.FirstOrDefaultAsync(x => x.MaLich == maLich);
        if (item is null) return NotFound();

        db.LichHocs.Remove(item);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

