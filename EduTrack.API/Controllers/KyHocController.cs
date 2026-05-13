using EduTrack.API.Authorization;
using EduTrack.API.Data;
using EduTrack.API.DTOs;
using EduTrack.API.Models;
using EduTrack.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Controllers;

[ApiController]
[Route("api/kyhoc")]
[Authorize(Policy = AppPolicies.CanConfigureSystem)]
public sealed class KyHocController(
    EduTrackDbContext db,
    ICurrentUserService current,
    IAuditLogService audit) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<KyHoc>>> GetAll(CancellationToken ct) =>
        Ok(await db.KyHocs.AsNoTracking().OrderBy(x => x.NamHoc).ThenBy(x => x.HocKy).ToListAsync(ct));

    [HttpGet("workflow-log")]
    public async Task<ActionResult<List<KyHocWorkflowLogDto>>> WorkflowLogs(
        [FromQuery] string namHoc,
        [FromQuery] byte hocKy,
        CancellationToken ct)
    {
        var logs = await db.KyHocWorkflowLogs.AsNoTracking()
            .Where(x => x.NamHoc == namHoc && x.HocKy == hocKy)
            .OrderByDescending(x => x.AtUtc)
            .Select(x => new KyHocWorkflowLogDto
            {
                Id = x.Id,
                NamHoc = x.NamHoc,
                HocKy = x.HocKy,
                FromStatus = x.FromStatus,
                ToStatus = x.ToStatus,
                PerformedByUserId = x.PerformedByUserId,
                PerformedByUserName = x.PerformedByUserName,
                BienBan = x.BienBan,
                AtUtc = x.AtUtc
            })
            .ToListAsync(ct);
        return Ok(logs);
    }

    [HttpPut]
    public async Task<ActionResult> Upsert([FromBody] KyHoc input, CancellationToken ct)
    {
        var row = await db.KyHocs.FirstOrDefaultAsync(k => k.NamHoc == input.NamHoc && k.HocKy == input.HocKy, ct);
        if (row is null)
        {
            db.KyHocs.Add(new KyHoc
            {
                NamHoc = input.NamHoc,
                HocKy = input.HocKy,
                Locked = input.Locked,
                TrangThai = input.Locked ? "DaChot" : "Mo",
                ApprovedAtUtc = input.Locked ? DateTime.UtcNow : null
            });
        }
        else
        {
            var from = row.TrangThai;
            row.Locked = input.Locked;
            row.TrangThai = input.Locked ? "DaChot" : "Mo";
            row.ApprovedAtUtc = input.Locked ? DateTime.UtcNow : row.ApprovedAtUtc;
            row.ReopenedAtUtc = input.Locked ? row.ReopenedAtUtc : DateTime.UtcNow;
            await AddWorkflowLogAsync(row.NamHoc, row.HocKy, from, row.TrangThai, "Cập nhật trực tiếp trạng thái khóa kỳ.", ct);
        }

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("submit-review")]
    public async Task<ActionResult> SubmitReview([FromBody] KyHocWorkflowActionRequest req, CancellationToken ct)
    {
        var row = await EnsureSemesterAsync(req.NamHoc, req.HocKy, ct);
        if (row.TrangThai == "DaChot") return Conflict(new { message = "Kỳ đã chốt, không thể gửi duyệt." });

        var from = row.TrangThai;
        row.TrangThai = "ChoDuyet";
        row.Locked = false;
        row.SubmittedAtUtc = DateTime.UtcNow;
        await AddWorkflowLogAsync(row.NamHoc, row.HocKy, from, row.TrangThai, req.BienBan ?? "Gửi kỳ chờ duyệt.", ct);
        await audit.LogAsync("KyHoc.SubmitReview", nameof(KyHoc), $"{row.NamHoc}/HK{row.HocKy}", from, row.TrangThai, ct);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("approve")]
    public async Task<ActionResult> Approve([FromBody] KyHocWorkflowActionRequest req, CancellationToken ct)
    {
        var row = await EnsureSemesterAsync(req.NamHoc, req.HocKy, ct);
        if (row.TrangThai != "ChoDuyet" && row.TrangThai != "Mo")
            return Conflict(new { message = "Trạng thái hiện tại không thể chốt kỳ." });

        var from = row.TrangThai;
        row.TrangThai = "DaChot";
        row.Locked = true;
        row.ApprovedAtUtc = DateTime.UtcNow;
        await AddWorkflowLogAsync(row.NamHoc, row.HocKy, from, row.TrangThai, req.BienBan ?? "Duyệt chốt kỳ.", ct);
        await audit.LogAsync("KyHoc.Approve", nameof(KyHoc), $"{row.NamHoc}/HK{row.HocKy}", from, row.TrangThai, ct);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("reopen")]
    public async Task<ActionResult> Reopen([FromBody] KyHocWorkflowActionRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.BienBan))
            return BadRequest(new { message = "Mở lại kỳ bắt buộc có biên bản." });

        var row = await EnsureSemesterAsync(req.NamHoc, req.HocKy, ct);
        if (row.TrangThai != "DaChot") return Conflict(new { message = "Chỉ kỳ đã chốt mới được mở lại." });

        var from = row.TrangThai;
        row.TrangThai = "MoLai";
        row.Locked = false;
        row.ReopenedAtUtc = DateTime.UtcNow;
        await AddWorkflowLogAsync(row.NamHoc, row.HocKy, from, row.TrangThai, req.BienBan, ct);
        await audit.LogAsync("KyHoc.Reopen", nameof(KyHoc), $"{row.NamHoc}/HK{row.HocKy}", from, row.TrangThai, ct);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task<KyHoc> EnsureSemesterAsync(string namHoc, byte hocKy, CancellationToken ct)
    {
        var row = await db.KyHocs.FirstOrDefaultAsync(k => k.NamHoc == namHoc && k.HocKy == hocKy, ct);
        if (row is not null) return row;

        row = new KyHoc
        {
            NamHoc = namHoc,
            HocKy = hocKy,
            Locked = false,
            TrangThai = "Mo"
        };
        db.KyHocs.Add(row);
        return row;
    }

    private async Task AddWorkflowLogAsync(
        string namHoc,
        byte hocKy,
        string fromStatus,
        string toStatus,
        string? bienBan,
        CancellationToken ct)
    {
        db.KyHocWorkflowLogs.Add(new KyHocWorkflowLog
        {
            NamHoc = namHoc,
            HocKy = hocKy,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            PerformedByUserId = current.UserId,
            PerformedByUserName = current.UserName,
            BienBan = bienBan,
            AtUtc = DateTime.UtcNow
        });

        await Task.CompletedTask;
    }
}
