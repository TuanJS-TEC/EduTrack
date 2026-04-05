using EduTrack.API.Authorization;
using EduTrack.API.Data;
using EduTrack.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Controllers;

[ApiController]
[Route("api/thongbao")]
[Authorize]
public sealed class ThongBaoController(EduTrackDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ThongBao>>> GetAll([FromQuery] string? maHS)
    {
        var q = db.ThongBaos.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(maHS)) q = q.Where(x => x.MaHS == maHS);
        return Ok(await q.OrderByDescending(x => x.NgayGui).ToListAsync());
    }

    [HttpGet("{maTB:int}")]
    public async Task<ActionResult<ThongBao>> GetById([FromRoute] int maTB)
    {
        var item = await db.ThongBaos.AsNoTracking().FirstOrDefaultAsync(x => x.MaTB == maTB);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.CanSendNotifications)]
    public async Task<ActionResult> Create([FromBody] ThongBao input)
    {
        if (input.NgayGui == default) input.NgayGui = DateTime.UtcNow;
        db.ThongBaos.Add(input);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { maTB = input.MaTB }, input);
    }

    [HttpPut("{maTB:int}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult> Update([FromRoute] int maTB, [FromBody] ThongBao input)
    {
        var item = await db.ThongBaos.FirstOrDefaultAsync(x => x.MaTB == maTB);
        if (item is null) return NotFound();

        item.TieuDe = input.TieuDe;
        item.NoiDung = input.NoiDung;
        item.LoaiTB = input.LoaiTB;
        item.MaHS = input.MaHS;
        item.NgayGui = input.NgayGui == default ? item.NgayGui : input.NgayGui;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{maTB:int}")]
    [Authorize(Policy = AppPolicies.CanSendNotifications)]
    public async Task<ActionResult> Delete([FromRoute] int maTB)
    {
        var item = await db.ThongBaos.FirstOrDefaultAsync(x => x.MaTB == maTB);
        if (item is null) return NotFound();

        db.ThongBaos.Remove(item);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

