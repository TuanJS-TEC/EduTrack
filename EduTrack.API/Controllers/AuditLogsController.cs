using EduTrack.API.Authorization;
using EduTrack.API.Data;
using EduTrack.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Policy = AppPolicies.CanViewReports)]
public sealed class AuditLogsController(EduTrackDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AuditLogEntryDto>>> GetLogs(
        [FromQuery] string? entityType,
        [FromQuery] string? entityKey,
        [FromQuery] string? action,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int take = 200,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var q = db.AuditLogEntries.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(entityType)) q = q.Where(x => x.EntityType == entityType);
        if (!string.IsNullOrWhiteSpace(entityKey)) q = q.Where(x => x.EntityKey == entityKey);
        if (!string.IsNullOrWhiteSpace(action)) q = q.Where(x => x.Action == action);
        if (fromUtc.HasValue) q = q.Where(x => x.AtUtc >= fromUtc.Value);
        if (toUtc.HasValue) q = q.Where(x => x.AtUtc <= toUtc.Value);

        var rows = await q.OrderByDescending(x => x.AtUtc)
            .Take(take)
            .Select(x => new AuditLogEntryDto
            {
                Id = x.Id,
                UserId = x.UserId,
                UserName = x.UserName,
                Action = x.Action,
                EntityType = x.EntityType,
                EntityKey = x.EntityKey,
                OldSnapshot = x.OldSnapshot,
                NewSnapshot = x.NewSnapshot,
                ViolationCode = x.ViolationCode,
                Severity = x.Severity,
                MetadataJson = x.MetadataJson,
                AtUtc = x.AtUtc
            })
            .ToListAsync(ct);

        return Ok(rows);
    }
}
