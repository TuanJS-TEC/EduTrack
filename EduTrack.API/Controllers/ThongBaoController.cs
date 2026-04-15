using EduTrack.API.Authorization;
using EduTrack.API.Data;
using EduTrack.API.Models;
using EduTrack.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Controllers;

[ApiController]
[Route("api/thongbao")]
[Authorize]
public sealed class ThongBaoController(
    EduTrackDbContext db,
    INotificationRealtimeService realtime) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ThongBao>>> GetAll(
        [FromQuery] string? maHS,
        [FromQuery] string? loaiTB,
        [FromQuery] bool? daDoc,
        CancellationToken ct = default)
    {
        var q = db.ThongBaos.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(maHS)) q = q.Where(x => x.MaHS == maHS);
        if (!string.IsNullOrWhiteSpace(loaiTB)) q = q.Where(x => x.LoaiTB == loaiTB);
        if (daDoc.HasValue) q = q.Where(x => x.DaDoc == daDoc.Value);
        return Ok(await q.OrderByDescending(x => x.NgayGui).ToListAsync(ct));
    }

    [HttpGet("{maTB:int}")]
    public async Task<ActionResult<ThongBao>> GetById([FromRoute] int maTB, CancellationToken ct = default)
    {
        var item = await db.ThongBaos.AsNoTracking().FirstOrDefaultAsync(x => x.MaTB == maTB, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.CanSendNotifications)]
    public async Task<ActionResult> Create([FromBody] ThongBao input, CancellationToken ct = default)
    {
        if (input.NgayGui == default) input.NgayGui = DateTime.UtcNow;
        input.DaDoc = false;
        input.NgayDoc = null;

        db.ThongBaos.Add(input);
        await db.SaveChangesAsync(ct);
        await realtime.NotifyCreatedAsync(input, ct);
        return CreatedAtAction(nameof(GetById), new { maTB = input.MaTB }, input);
    }

    [HttpPut("{maTB:int}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult> Update([FromRoute] int maTB, [FromBody] ThongBao input, CancellationToken ct = default)
    {
        var item = await db.ThongBaos.FirstOrDefaultAsync(x => x.MaTB == maTB, ct);
        if (item is null) return NotFound();

        item.TieuDe = input.TieuDe;
        item.NoiDung = input.NoiDung;
        item.LoaiTB = input.LoaiTB;
        item.MaHS = input.MaHS;
        item.NgayGui = input.NgayGui == default ? item.NgayGui : input.NgayGui;
        item.DaDoc = input.DaDoc;
        item.NgayDoc = input.DaDoc ? input.NgayDoc ?? DateTime.UtcNow : null;

        await db.SaveChangesAsync(ct);
        await realtime.NotifyUpdatedAsync(item, ct);
        return NoContent();
    }

    [HttpPut("{maTB:int}/read")]
    public async Task<ActionResult> MarkAsRead([FromRoute] int maTB, CancellationToken ct = default)
    {
        var item = await db.ThongBaos.FirstOrDefaultAsync(x => x.MaTB == maTB, ct);
        if (item is null) return NotFound();

        if (!item.DaDoc)
        {
            item.DaDoc = true;
            item.NgayDoc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            await realtime.NotifyReadAsync(item, ct);
        }

        return NoContent();
    }

    [HttpDelete("{maTB:int}")]
    [Authorize(Policy = AppPolicies.CanSendNotifications)]
    public async Task<ActionResult> Delete([FromRoute] int maTB, CancellationToken ct = default)
    {
        var item = await db.ThongBaos.FirstOrDefaultAsync(x => x.MaTB == maTB, ct);
        if (item is null) return NotFound();

        var maHS = item.MaHS;
        db.ThongBaos.Remove(item);
        await db.SaveChangesAsync(ct);
        await realtime.NotifyDeletedAsync(maTB, maHS, ct);
        return NoContent();
    }
}

