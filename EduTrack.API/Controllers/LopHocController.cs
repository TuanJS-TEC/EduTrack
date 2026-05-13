using EduTrack.API.Authorization;
using EduTrack.API.Data;
using EduTrack.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Controllers;

[ApiController]
[Route("api/lophoc")]
[Authorize]
public sealed class LopHocController(EduTrackDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AppPolicies.CanViewStudents)]
    public async Task<ActionResult<List<LopHoc>>> GetAll([FromQuery] string? namHoc, [FromQuery] string? khoiLop)
    {
        var q = db.LopHocs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(namHoc)) q = q.Where(x => x.NamHoc == namHoc);
        if (!string.IsNullOrWhiteSpace(khoiLop)) q = q.Where(x => x.KhoiLop == khoiLop);

        return Ok(await q.OrderBy(x => x.TenLop).ToListAsync());
    }

    [HttpGet("{maLop}")]
    [Authorize(Policy = AppPolicies.CanViewStudents)]
    public async Task<ActionResult<LopHoc>> GetById([FromRoute] string maLop)
    {
        var item = await db.LopHocs.AsNoTracking().FirstOrDefaultAsync(x => x.MaLop == maLop);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.CanConfigureSystem)]
    public async Task<ActionResult> Create([FromBody] LopHoc input)
    {
        if (await db.LopHocs.AnyAsync(x => x.MaLop == input.MaLop))
            return Conflict(new { message = "MaLop đã tồn tại" });

        db.LopHocs.Add(input);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { maLop = input.MaLop }, input);
    }

    [HttpPut("{maLop}")]
    [Authorize(Policy = AppPolicies.CanConfigureSystem)]
    public async Task<ActionResult> Update([FromRoute] string maLop, [FromBody] LopHoc input)
    {
        var item = await db.LopHocs.FirstOrDefaultAsync(x => x.MaLop == maLop);
        if (item is null) return NotFound();

        item.TenLop = input.TenLop;
        item.KhoiLop = input.KhoiLop;
        item.NamHoc = input.NamHoc;
        item.MaGVChuNhiem = input.MaGVChuNhiem;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{maLop}")]
    [Authorize(Policy = AppPolicies.CanConfigureSystem)]
    public async Task<ActionResult> Delete([FromRoute] string maLop)
    {
        var item = await db.LopHocs.FirstOrDefaultAsync(x => x.MaLop == maLop);
        if (item is null) return NotFound();

        db.LopHocs.Remove(item);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

