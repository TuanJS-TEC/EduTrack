using EduTrack.API.Authorization;
using EduTrack.API.Data;
using EduTrack.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Controllers;

[ApiController]
[Route("api/kyhoc")]
[Authorize(Policy = AppPolicies.CanConfigureSystem)]
public sealed class KyHocController(EduTrackDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<KyHoc>>> GetAll(CancellationToken ct) =>
        Ok(await db.KyHocs.AsNoTracking().OrderBy(x => x.NamHoc).ThenBy(x => x.HocKy).ToListAsync(ct));

    [HttpPut]
    public async Task<ActionResult> Upsert([FromBody] KyHoc input, CancellationToken ct)
    {
        var row = await db.KyHocs.FirstOrDefaultAsync(k => k.NamHoc == input.NamHoc && k.HocKy == input.HocKy, ct);
        if (row is null)
        {
            db.KyHocs.Add(new KyHoc { NamHoc = input.NamHoc, HocKy = input.HocKy, Locked = input.Locked });
        }
        else row.Locked = input.Locked;

        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
