using System.Globalization;
using System.Text.Json;
using ClosedXML.Excel;
using EduTrack.API.Authorization;
using EduTrack.API.Data;
using EduTrack.API.DTOs;
using EduTrack.API.Helpers;
using EduTrack.API.Models;
using EduTrack.API.Services;
using EduTrack.API.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Controllers;

[ApiController]
[Route("api/diemso")]
[Authorize]
public sealed class DiemSoController(
    EduTrackDbContext db,
    IAccessControlService access,
    IAuditLogService audit,
    ICurrentUserService current) : ControllerBase
{
    private static DiemSoResponse ToResponse(DiemSo d)
    {
        var (m, p) = DiemSoScoreReader.GetComponentLists(d);
        var tbm = DiemSoScoreReader.RecalculateTbm(d);
        return new DiemSoResponse
        {
            MaDiem = d.MaDiem,
            MaHS = d.MaHS,
            MaMon = d.MaMon,
            NamHoc = d.NamHoc,
            HocKy = d.HocKy,
            DiemMiengs = m,
            Diem15ps = p,
            DiemMieng = d.DiemMieng,
            Diem15p = d.Diem15p,
            DiemGiuaKy = d.DiemGiuaKy,
            DiemCuoiKy = d.DiemCuoiKy,
            DiemTBMon = tbm,
            TrangThaiNhapDiem = DiemNhapTrangThai.Compute(d),
        };
    }

    [HttpGet]
    [Authorize(Policy = AppPolicies.CanViewScores)]
    public async Task<ActionResult<List<DiemSoResponse>>> GetAll(
        [FromQuery] string? maHS,
        [FromQuery] string? maMon,
        [FromQuery] string? namHoc,
        [FromQuery] byte? hocKy,
        CancellationToken ct)
    {
        var userId = current.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (!string.IsNullOrEmpty(maHS) && !await access.CanViewStudentAsync(userId, maHS, ct))
            return Forbid();

        var q = db.DiemSos.AsNoTracking().Include(x => x.ThanhPhans).AsQueryable();
        if (!string.IsNullOrWhiteSpace(maHS)) q = q.Where(x => x.MaHS == maHS);
        if (!string.IsNullOrWhiteSpace(maMon)) q = q.Where(x => x.MaMon == maMon);
        if (!string.IsNullOrWhiteSpace(namHoc)) q = q.Where(x => x.NamHoc == namHoc);
        if (hocKy.HasValue) q = q.Where(x => x.HocKy == hocKy.Value);

        if (User.IsInRole(RolePermissionSeeder.Parent))
        {
            var codes = await access.GetParentStudentCodesAsync(userId, ct);
            q = q.Where(x => codes.Contains(x.MaHS));
        }

        var list = await q.OrderBy(x => x.MaHS).ThenBy(x => x.MaMon).ToListAsync(ct);
        return Ok(list.Select(ToResponse).ToList());
    }

    [HttpGet("{maDiem:int}")]
    [Authorize(Policy = AppPolicies.CanViewScores)]
    public async Task<ActionResult<DiemSoResponse>> GetById([FromRoute] int maDiem, CancellationToken ct)
    {
        var userId = current.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var item = await db.DiemSos.AsNoTracking().Include(x => x.ThanhPhans).FirstOrDefaultAsync(x => x.MaDiem == maDiem, ct);
        if (item is null) return NotFound();
        if (!await access.CanViewStudentAsync(userId, item.MaHS, ct)) return Forbid();
        return Ok(ToResponse(item));
    }

    [HttpGet("{maDiem:int}/audit-trail")]
    [Authorize(Policy = AppPolicies.CanViewReports)]
    public async Task<ActionResult<List<AuditLogEntryDto>>> GetAuditTrail([FromRoute] int maDiem, CancellationToken ct)
    {
        var logs = await db.AuditLogEntries.AsNoTracking()
            .Where(x => x.EntityType == nameof(DiemSo) && x.EntityKey == maDiem.ToString())
            .OrderByDescending(x => x.AtUtc)
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

        return Ok(logs);
    }

    [HttpPost("upsert")]
    [Authorize(Policy = AppPolicies.CanEditScores)]
    public async Task<ActionResult<DiemSoResponse>> Upsert([FromBody] DiemSoUpsertRequest req, CancellationToken ct)
    {
        var userId = current.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (string.IsNullOrWhiteSpace(req.MaHS) || string.IsNullOrWhiteSpace(req.MaMon))
            return ProblemResponses.Of(400, "MaHS/MaMon không hợp lệ", ApiErrorCodes.Validation);

        if (!EduCodeFormats.IsValidSchoolYear(req.NamHoc))
            return ProblemResponses.Of(400, "NamHoc không đúng định dạng YYYY-YYYY", ApiErrorCodes.Validation);

        if (!await access.CanEditScoreAsync(userId, req.MaHS, req.MaMon, req.NamHoc, req.HocKy, ct))
            return Forbid();

        var err = ScoreInputValidator.ValidateRequest(req);
        if (err is not null)
        {
            await LogRuleViolationAsync(
                "DiemSo.RuleViolation",
                req.MaHS,
                req.MaMon,
                req.NamHoc,
                req.HocKy,
                "SCORE_OUT_OF_RANGE",
                "High",
                err,
                ct);
            return ProblemResponses.Of(400, err, ApiErrorCodes.ScoreOutOfRange);
        }

        var ky = await db.KyHocs.AsNoTracking().FirstOrDefaultAsync(k => k.NamHoc == req.NamHoc && k.HocKy == req.HocKy, ct);
        if (ky?.Locked == true)
        {
            await LogRuleViolationAsync(
                "DiemSo.RuleViolation",
                req.MaHS,
                req.MaMon,
                req.NamHoc,
                req.HocKy,
                "SEMESTER_LOCKED",
                "Critical",
                "Học kỳ đã chốt, không được sửa điểm",
                ct);
            return ProblemResponses.Of(409, "Học kỳ đã chốt, không được sửa điểm", ApiErrorCodes.SemesterLocked);
        }

        var existsHS = await db.HocSinhs.AnyAsync(x => x.MaHS == req.MaHS, ct);
        if (!existsHS) return ProblemResponses.Of(400, "Học sinh không tồn tại");

        var existsMon = await db.MonHocs.AnyAsync(x => x.MaMon == req.MaMon, ct);
        if (!existsMon) return ProblemResponses.Of(400, "Môn học không tồn tại");

        var item = await db.DiemSos.Include(x => x.ThanhPhans).FirstOrDefaultAsync(
            x => x.MaHS == req.MaHS && x.MaMon == req.MaMon && x.NamHoc == req.NamHoc && x.HocKy == req.HocKy, ct);

        var oldSnapshot = item is null ? null : SnapshotDiem(item);

        if (item is null)
        {
            item = new DiemSo { MaHS = req.MaHS, MaMon = req.MaMon, NamHoc = req.NamHoc, HocKy = req.HocKy };
            db.DiemSos.Add(item);
        }

        ApplyScores(item, req);
        item.DiemTBMon = DiemSoScoreReader.RecalculateTbm(item);

        await db.SaveChangesAsync(ct);

        var tracked = await db.DiemSos.Include(x => x.ThanhPhans).AsNoTracking().FirstAsync(x => x.MaDiem == item.MaDiem, ct);
        await audit.LogAsync("DiemSo.Upsert", nameof(DiemSo), item.MaDiem.ToString(), oldSnapshot, SnapshotDiem(tracked), ct);

        return Ok(ToResponse(tracked));
    }

    [HttpPost("bulk-upsert")]
    [Authorize(Policy = AppPolicies.CanEditScores)]
    public async Task<ActionResult<DiemSoBulkUpsertResultDto>> BulkUpsert([FromBody] DiemSoBulkUpsertRequest req, CancellationToken ct)
    {
        var userId = current.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (string.IsNullOrWhiteSpace(req.MaLop) || string.IsNullOrWhiteSpace(req.MaMon))
            return ProblemResponses.Of(400, "Thiếu MaLop/MaMon");

        if (!EduCodeFormats.IsValidSchoolYear(req.NamHoc))
            return ProblemResponses.Of(400, "NamHoc không đúng định dạng YYYY-YYYY", ApiErrorCodes.Validation);

        var ky = await db.KyHocs.AsNoTracking().FirstOrDefaultAsync(k => k.NamHoc == req.NamHoc && k.HocKy == req.HocKy, ct);
        if (ky?.Locked == true)
        {
            await LogRuleViolationAsync(
                "DiemSo.BulkRuleViolation",
                null,
                req.MaMon,
                req.NamHoc,
                req.HocKy,
                "SEMESTER_LOCKED",
                "Critical",
                "Bulk upsert bị chặn vì học kỳ đã chốt",
                ct);
            return ProblemResponses.Of(409, "Học kỳ đã chốt, không được sửa điểm", ApiErrorCodes.SemesterLocked);
        }

        if (!EduCodeFormats.IsValidClassCode(req.MaLop) || !EduCodeFormats.IsValidSubjectCode(req.MaMon))
            return ProblemResponses.Of(400, "MaLop/MaMon không đúng định dạng");

        var maHsInLop = await db.HocSinhs.AsNoTracking()
            .Where(h => h.MaLop == req.MaLop)
            .Select(h => h.MaHS)
            .ToListAsync(ct);
        var inLop = maHsInLop.ToHashSet();

        var toApply = new List<DiemSoUpsertRequest>();
        var result = new DiemSoBulkUpsertResultDto();

        foreach (var row in req.Rows)
        {
            row.NamHoc = req.NamHoc;
            row.HocKy = req.HocKy;
            row.MaMon = req.MaMon;

            if (!inLop.Contains(row.MaHS))
            {
                result.Errors.Add($"{row.MaHS}: không thuộc lớp {req.MaLop}");
                continue;
            }

            var err = ScoreInputValidator.ValidateRequest(row);
            if (err is not null)
            {
                result.Errors.Add($"{row.MaHS}: {err}");
                continue;
            }

            toApply.Add(row);
        }

        foreach (var row in toApply)
        {
            if (!await access.CanEditScoreAsync(userId, row.MaHS, row.MaMon, row.NamHoc, row.HocKy, ct))
                return Forbid();
        }

        var existing = await db.DiemSos
            .Include(x => x.ThanhPhans)
            .Where(d => d.MaMon == req.MaMon && d.NamHoc == req.NamHoc && d.HocKy == req.HocKy && inLop.Contains(d.MaHS))
            .ToDictionaryAsync(d => d.MaHS, ct);

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            foreach (var row in toApply)
            {
                if (!existing.TryGetValue(row.MaHS, out var item))
                {
                    item = new DiemSo { MaHS = row.MaHS, MaMon = row.MaMon, NamHoc = row.NamHoc, HocKy = row.HocKy };
                    db.DiemSos.Add(item);
                    existing[row.MaHS] = item;
                }

                ApplyScores(item, row);
                item.DiemTBMon = DiemSoScoreReader.RecalculateTbm(item);
                result.Updated++;
            }

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        await audit.LogAsync(
            "DiemSo.BulkUpsert",
            nameof(DiemSo),
            $"{req.MaLop}/{req.MaMon}/{req.NamHoc}/HK{req.HocKy}",
            null,
            JsonSerializer.Serialize(new { result.Updated, Errors = result.Errors }),
            ct);

        return Ok(result);
    }

    [HttpPost("import/excel")]
    [Authorize(Policy = AppPolicies.CanEditScores)]
    public async Task<ActionResult<DiemSoImportResultDto>> ImportExcel(
        [FromForm] IFormFile file,
        [FromQuery] string maLop,
        [FromQuery] string maMon,
        [FromQuery] string namHoc,
        [FromQuery] byte hocKy,
        CancellationToken ct)
    {
        var userId = current.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (file.Length == 0) return BadRequest("File rỗng");
        if (string.IsNullOrWhiteSpace(maLop) || string.IsNullOrWhiteSpace(maMon))
            return ProblemResponses.Of(400, "Thiếu maLop/maMon");

        if (!EduCodeFormats.IsValidSchoolYear(namHoc))
            return ProblemResponses.Of(400, "NamHoc không đúng định dạng", ApiErrorCodes.Validation);

        var ky = await db.KyHocs.AsNoTracking().FirstOrDefaultAsync(k => k.NamHoc == namHoc && k.HocKy == hocKy, ct);
        if (ky?.Locked == true)
        {
            await LogRuleViolationAsync(
                "DiemSo.ImportRuleViolation",
                null,
                maMon,
                namHoc,
                hocKy,
                "SEMESTER_LOCKED",
                "Critical",
                "Import bị chặn vì học kỳ đã chốt",
                ct);
            return ProblemResponses.Of(409, "Học kỳ đã chốt", ApiErrorCodes.SemesterLocked);
        }

        if (!await db.MonHocs.AnyAsync(m => m.MaMon == maMon, ct))
            return ProblemResponses.Of(400, "Môn học không tồn tại");

        var inLop = (await db.HocSinhs.AsNoTracking()
            .Where(h => h.MaLop == maLop)
            .Select(h => h.MaHS)
            .ToListAsync(ct)).ToHashSet();

        var importResult = new DiemSoImportResultDto();
        var rowsToUpsert = new List<DiemSoUpsertRequest>();

        await using var stream = file.OpenReadStream();
        using var wb = new XLWorkbook(stream);
        var ws = wb.Worksheet(1);
        var excelRows = ws.RangeUsed()?.RowsUsed().Skip(1) ?? Enumerable.Empty<IXLRangeRow>();

        foreach (var row in excelRows)
        {
            var maHs = row.Cell(1).GetString().Trim();
            if (string.IsNullOrWhiteSpace(maHs))
            {
                importResult.Skipped++;
                continue;
            }

            if (!inLop.Contains(maHs))
            {
                importResult.Skipped++;
                importResult.Warnings.Add($"Dòng {row.RowNumber()}: {maHs} không thuộc lớp {maLop}.");
                continue;
            }

            var miengs = ParseDecimalList(row.Cell(3).GetString());
            var ps = ParseDecimalList(row.Cell(4).GetString());
            var gk = ParseDecimalCell(row.Cell(5));
            var ck = ParseDecimalCell(row.Cell(6));

            var req = new DiemSoUpsertRequest
            {
                MaHS = maHs,
                MaMon = maMon,
                NamHoc = namHoc,
                HocKy = hocKy,
                DiemMiengs = miengs,
                Diem15ps = ps,
                DiemGiuaKy = gk,
                DiemCuoiKy = ck,
            };

            var err = ScoreInputValidator.ValidateRequest(req);
            if (err is not null)
            {
                importResult.Skipped++;
                importResult.Warnings.Add($"Dòng {row.RowNumber()}: {err}");
                continue;
            }

            if (!await access.CanEditScoreAsync(userId, maHs, maMon, namHoc, hocKy, ct))
                return Forbid();

            rowsToUpsert.Add(req);
        }

        var existing = await db.DiemSos
            .Include(x => x.ThanhPhans)
            .Where(d => d.MaMon == maMon && d.NamHoc == namHoc && d.HocKy == hocKy && inLop.Contains(d.MaHS))
            .ToDictionaryAsync(d => d.MaHS, ct);

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            foreach (var row in rowsToUpsert)
            {
                if (!existing.TryGetValue(row.MaHS, out var item))
                {
                    item = new DiemSo { MaHS = row.MaHS, MaMon = row.MaMon, NamHoc = row.NamHoc, HocKy = row.HocKy };
                    db.DiemSos.Add(item);
                    existing[row.MaHS] = item;
                }

                ApplyScores(item, row);
                item.DiemTBMon = DiemSoScoreReader.RecalculateTbm(item);
                importResult.Imported++;
            }

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        await audit.LogAsync(
            "DiemSo.ImportExcel",
            nameof(DiemSo),
            $"{maLop}/{maMon}/{namHoc}/HK{hocKy}",
            null,
            JsonSerializer.Serialize(new { importResult.Imported, importResult.Skipped }),
            ct);

        return Ok(importResult);
    }

    [HttpDelete("{maDiem:int}")]
    [Authorize(Policy = AppPolicies.CanEditScores)]
    public async Task<ActionResult> Delete([FromRoute] int maDiem, CancellationToken ct)
    {
        var userId = current.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var item = await db.DiemSos.Include(x => x.ThanhPhans).FirstOrDefaultAsync(x => x.MaDiem == maDiem, ct);
        if (item is null) return NotFound();

        var ky = await db.KyHocs.AsNoTracking().FirstOrDefaultAsync(k => k.NamHoc == item.NamHoc && k.HocKy == item.HocKy, ct);
        if (ky?.Locked == true)
        {
            await LogRuleViolationAsync(
                "DiemSo.DeleteRuleViolation",
                item.MaHS,
                item.MaMon,
                item.NamHoc,
                item.HocKy,
                "SEMESTER_LOCKED",
                "Critical",
                "Xóa điểm bị chặn vì học kỳ đã chốt",
                ct);
            return ProblemResponses.Of(409, "Học kỳ đã chốt", ApiErrorCodes.SemesterLocked);
        }

        if (!await access.CanEditScoreAsync(userId, item.MaHS, item.MaMon, item.NamHoc, item.HocKy, ct))
            return Forbid();

        var oldSnapshot = SnapshotDiem(item);
        db.DiemSos.Remove(item);
        await db.SaveChangesAsync(ct);
        await audit.LogAsync("DiemSo.Delete", nameof(DiemSo), maDiem.ToString(), oldSnapshot, null, ct);
        return NoContent();
    }

    [HttpGet("bangdiem")]
    [Authorize(Policy = AppPolicies.CanViewScores)]
    public async Task<ActionResult<List<BangDiemItemResponse>>> BangDiem(
        [FromQuery] string maLop,
        [FromQuery] string maMon,
        [FromQuery] string namHoc,
        [FromQuery] byte hocKy,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(maLop) || string.IsNullOrWhiteSpace(maMon))
            return ProblemResponses.Of(400, "Thiếu maLop/maMon");

        if (!await db.MonHocs.AnyAsync(m => m.MaMon == maMon, ct))
            return ProblemResponses.Of(400, "Môn học không tồn tại");

        var data = await BuildBangDiemAsync(maLop, maMon, namHoc, hocKy, ct);
        return Ok(data);
    }

    [HttpGet("bangdiem/excel")]
    [Authorize(Policy = AppPolicies.CanViewScores)]
    public async Task<IActionResult> BangDiemExcel([FromQuery] string maLop, [FromQuery] string maMon, [FromQuery] string namHoc, [FromQuery] byte hocKy, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(maLop) || string.IsNullOrWhiteSpace(maMon))
            return ProblemResponses.Of(400, "Thiếu maLop/maMon");
        if (!await db.MonHocs.AnyAsync(m => m.MaMon == maMon, ct))
            return ProblemResponses.Of(400, "Môn học không tồn tại");

        var data = await BuildBangDiemAsync(maLop, maMon, namHoc, hocKy, ct);

        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Bang diem");
        ws.Cell(1, 1).Value = "MaHS";
        ws.Cell(1, 2).Value = "HoTen";
        ws.Cell(1, 3).Value = "Diem mieng";
        ws.Cell(1, 4).Value = "Diem 15p";
        ws.Cell(1, 5).Value = "GK";
        ws.Cell(1, 6).Value = "CK";
        ws.Cell(1, 7).Value = "TBM";
        ws.Cell(1, 8).Value = "Xep loai";
        ws.Cell(1, 9).Value = "Trang thai nhap";
        var r = 2;
        foreach (var row in data)
        {
            ws.Cell(r, 1).Value = row.MaHS;
            ws.Cell(r, 2).Value = row.HoTen;
            ws.Cell(r, 3).Value = string.Join(", ", row.DiemMiengs);
            ws.Cell(r, 4).Value = string.Join(", ", row.Diem15ps);
            ws.Cell(r, 5).Value = row.DiemGiuaKy?.ToString() ?? "";
            ws.Cell(r, 6).Value = row.DiemCuoiKy?.ToString() ?? "";
            ws.Cell(r, 7).Value = row.DiemTBMon?.ToString() ?? "";
            ws.Cell(r, 8).Value = row.XepLoai ?? "";
            ws.Cell(r, 9).Value = row.TrangThaiNhapDiem;
            r++;
        }

        await using var ms = new MemoryStream();
        wb.SaveAs(ms);
        var bytes = ms.ToArray();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"bang-diem-{maLop}-{maMon}-hk{hocKy}.xlsx");
    }

    [HttpGet("bangdiem/pdf")]
    [Authorize(Policy = AppPolicies.CanViewScores)]
    public async Task<IActionResult> BangDiemPdf([FromQuery] string maLop, [FromQuery] string maMon, [FromQuery] string namHoc, [FromQuery] byte hocKy, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(maLop) || string.IsNullOrWhiteSpace(maMon))
            return ProblemResponses.Of(400, "Thiếu maLop/maMon");
        if (!await db.MonHocs.AnyAsync(m => m.MaMon == maMon, ct))
            return ProblemResponses.Of(400, "Môn học không tồn tại");

        var data = await BuildBangDiemAsync(maLop, maMon, namHoc, hocKy, ct);
        var tenMon = data.FirstOrDefault()?.TenMon ?? maMon;
        var pdf = BangDiemPdfExporter.Build(data, maLop, maMon, tenMon, namHoc, hocKy);
        return File(pdf, "application/pdf", $"bang-diem-{maLop}-{maMon}-hk{hocKy}.pdf");
    }

    [HttpGet("thong-ke")]
    [Authorize(Policy = AppPolicies.CanViewScores)]
    public async Task<ActionResult<DiemThongKeResponse>> ThongKe(
        [FromQuery] string maLop,
        [FromQuery] string maMon,
        [FromQuery] string namHoc,
        [FromQuery] byte hocKy,
        [FromQuery] int top = 5,
        [FromQuery] int bottom = 5,
        CancellationToken ct = default)
    {
        top = Math.Clamp(top, 1, 50);
        bottom = Math.Clamp(bottom, 1, 50);

        if (string.IsNullOrWhiteSpace(maLop) || string.IsNullOrWhiteSpace(maMon))
            return ProblemResponses.Of(400, "Thiếu maLop/maMon");
        if (!await db.MonHocs.AnyAsync(m => m.MaMon == maMon, ct))
            return ProblemResponses.Of(400, "Môn học không tồn tại");

        var data = await BuildBangDiemAsync(maLop, maMon, namHoc, hocKy, ct);
        return Ok(BuildThongKeResponse(data, top, bottom));
    }

    [HttpGet("thong-ke/khoi")]
    [Authorize(Policy = AppPolicies.CanViewScores)]
    public async Task<ActionResult<DiemThongKeResponse>> ThongKeKhoi(
        [FromQuery] string khoiLop,
        [FromQuery] string maMon,
        [FromQuery] string namHoc,
        [FromQuery] byte hocKy,
        [FromQuery] int top = 5,
        [FromQuery] int bottom = 5,
        CancellationToken ct = default)
    {
        if (User.IsInRole(RolePermissionSeeder.Parent))
            return Forbid();

        top = Math.Clamp(top, 1, 50);
        bottom = Math.Clamp(bottom, 1, 50);

        if (string.IsNullOrWhiteSpace(khoiLop) || string.IsNullOrWhiteSpace(maMon))
            return ProblemResponses.Of(400, "Thiếu khoiLop/maMon");
        if (!await db.MonHocs.AnyAsync(m => m.MaMon == maMon, ct))
            return ProblemResponses.Of(400, "Môn học không tồn tại");

        var data = await BuildBangDiemKhoiAsync(khoiLop, namHoc, maMon, namHoc, hocKy, ct);
        return Ok(BuildThongKeResponse(data, top, bottom));
    }

    [HttpGet("tong-hop/hoc-sinh")]
    [Authorize(Policy = AppPolicies.CanViewScores)]
    public async Task<ActionResult<DiemTongHopHocSinhResponse>> TongHopHocSinh(
        [FromQuery] string maHS,
        [FromQuery] string namHoc,
        [FromQuery] byte hocKy,
        CancellationToken ct)
    {
        var userId = current.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        if (!await access.CanViewStudentAsync(userId, maHS, ct)) return Forbid();

        var hs = await db.HocSinhs.AsNoTracking().FirstOrDefaultAsync(h => h.MaHS == maHS, ct);
        if (hs is null) return NotFound();

        var dsList = await db.DiemSos.AsNoTracking()
            .Include(d => d.ThanhPhans)
            .Include(d => d.MonHoc)
            .Where(d => d.MaHS == maHS && d.NamHoc == namHoc && d.HocKy == hocKy)
            .OrderBy(d => d.MaMon)
            .ToListAsync(ct);

        var theoMon = new List<DiemMonKyItemDto>();
        foreach (var d in dsList)
        {
            var tbm = DiemSoScoreReader.RecalculateTbm(d);
            theoMon.Add(new DiemMonKyItemDto
            {
                MaMon = d.MaMon,
                TenMon = d.MonHoc?.TenMon ?? "",
                DiemTBMon = tbm,
                TrangThaiNhapDiem = DiemNhapTrangThai.Compute(d),
            });
        }

        var tbms = theoMon.Where(x => x.DiemTBMon.HasValue).Select(x => x.DiemTBMon!.Value).ToList();
        return Ok(new DiemTongHopHocSinhResponse
        {
            MaHS = hs.MaHS,
            HoTen = hs.HoTen,
            MaLop = hs.MaLop,
            NamHoc = namHoc,
            HocKy = hocKy,
            TheoMon = theoMon,
            TbChungKy = tbms.Count == 0 ? null : GradeCalculator.RoundOneDecimal(tbms.Average()),
            SoMonCoTbm = tbms.Count,
        });
    }

    [HttpGet("tong-hop/lop")]
    [Authorize(Policy = AppPolicies.CanViewScores)]
    public async Task<ActionResult<DiemTongHopLopResponse>> TongHopLop(
        [FromQuery] string maLop,
        [FromQuery] string namHoc,
        [FromQuery] byte hocKy,
        CancellationToken ct)
    {
        if (User.IsInRole(RolePermissionSeeder.Parent))
            return Forbid();

        var lop = await db.LopHocs.AsNoTracking().FirstOrDefaultAsync(l => l.MaLop == maLop, ct);
        var (siSo, coTb, tbLop) = await ComputeLopAggregateAsync(maLop, namHoc, hocKy, ct);

        return Ok(new DiemTongHopLopResponse
        {
            MaLop = maLop,
            TenLop = lop?.TenLop,
            NamHoc = namHoc,
            HocKy = hocKy,
            SiSo = siSo,
            SoHocSinhCoTbChung = coTb,
            TbChungLop = tbLop,
        });
    }

    [HttpGet("tong-hop/khoi")]
    [Authorize(Policy = AppPolicies.CanViewScores)]
    public async Task<ActionResult<DiemTongHopKhoiResponse>> TongHopKhoi(
        [FromQuery] string khoiLop,
        [FromQuery] string namHoc,
        [FromQuery] byte hocKy,
        CancellationToken ct)
    {
        if (User.IsInRole(RolePermissionSeeder.Parent))
            return Forbid();

        var maHsList = await (from h in db.HocSinhs.AsNoTracking()
                              join l in db.LopHocs.AsNoTracking() on h.MaLop equals l.MaLop
                              where l.KhoiLop == khoiLop && l.NamHoc == namHoc
                              select h.MaHS).Distinct().ToListAsync(ct);

        var agg = await ComputeTbChungFromMaHsAsync(maHsList, namHoc, hocKy, ct);
        return Ok(new DiemTongHopKhoiResponse
        {
            KhoiLop = khoiLop,
            NamHoc = namHoc,
            HocKy = hocKy,
            TongSoHocSinh = maHsList.Count,
            SoHocSinhCoTbChung = agg.coTb,
            TbChungKhoi = agg.tb,
        });
    }

    [HttpGet("tong-hop/truong")]
    [Authorize(Policy = AppPolicies.CanViewScores)]
    public async Task<ActionResult<DiemTongHopTruongResponse>> TongHopTruong(
        [FromQuery] string namHoc,
        [FromQuery] byte hocKy,
        CancellationToken ct)
    {
        if (User.IsInRole(RolePermissionSeeder.Parent))
            return Forbid();

        var maHsList = await (from h in db.HocSinhs.AsNoTracking()
                              join l in db.LopHocs.AsNoTracking() on h.MaLop equals l.MaLop
                              where l.NamHoc == namHoc
                              select h.MaHS).Distinct().ToListAsync(ct);

        var agg = await ComputeTbChungFromMaHsAsync(maHsList, namHoc, hocKy, ct);
        return Ok(new DiemTongHopTruongResponse
        {
            NamHoc = namHoc,
            HocKy = hocKy,
            TongSoHocSinh = maHsList.Count,
            SoHocSinhCoTbChung = agg.coTb,
            TbChungTruong = agg.tb,
        });
    }

    private static List<decimal>? ParseDecimalList(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var list = new List<decimal>();
        foreach (var part in text.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (decimal.TryParse(part, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                list.Add(v);
        }

        return list.Count == 0 ? null : list;
    }

    private static decimal? ParseDecimalCell(IXLCell cell)
    {
        if (cell.IsEmpty()) return null;
        if (cell.DataType == XLDataType.Number)
            return (decimal)cell.GetDouble();

        var s = cell.GetString().Trim();
        return string.IsNullOrEmpty(s)
            ? null
            : decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;
    }

    private async Task<(int siSo, int coTb, decimal? tbLop)> ComputeLopAggregateAsync(string maLop, string namHoc, byte hocKy, CancellationToken ct)
    {
        var siSo = await db.HocSinhs.AsNoTracking().CountAsync(h => h.MaLop == maLop, ct);
        var maHs = await db.HocSinhs.AsNoTracking().Where(h => h.MaLop == maLop).Select(h => h.MaHS).ToListAsync(ct);
        var agg = await ComputeTbChungFromMaHsAsync(maHs, namHoc, hocKy, ct);
        return (siSo, agg.coTb, agg.tb);
    }

    private async Task<(int coTb, decimal? tb)> ComputeTbChungFromMaHsAsync(List<string> maHsList, string namHoc, byte hocKy, CancellationToken ct)
    {
        if (maHsList.Count == 0) return (0, null);

        var allDiem = await db.DiemSos.AsNoTracking()
            .Include(d => d.ThanhPhans)
            .Where(d => maHsList.Contains(d.MaHS) && d.NamHoc == namHoc && d.HocKy == hocKy)
            .ToListAsync(ct);

        var byHs = allDiem.GroupBy(d => d.MaHS).ToDictionary(g => g.Key, g => g.ToList());
        var tbChungList = new List<decimal>();
        foreach (var ma in maHsList)
        {
            if (!byHs.TryGetValue(ma, out var list)) continue;
            var tbms = list.Select(DiemSoScoreReader.RecalculateTbm).Where(x => x.HasValue).Select(x => x!.Value).ToList();
            if (tbms.Count > 0) tbChungList.Add(GradeCalculator.RoundOneDecimal(tbms.Average()));
        }

        return (
            tbChungList.Count,
            tbChungList.Count == 0 ? null : GradeCalculator.RoundOneDecimal(tbChungList.Average()));
    }

    private static DiemThongKeResponse BuildThongKeResponse(List<BangDiemItemResponse> data, int top, int bottom)
    {
        var soChuaCoDiem = data.Count(x => x.TrangThaiNhapDiem == DiemNhapTrangThai.ChuaCoDiem);
        var withTbm = data.Where(x => x.DiemTBMon.HasValue).ToList();

        if (withTbm.Count == 0)
        {
            return new DiemThongKeResponse
            {
                SiSo = data.Count,
                SoHocSinhCoTbm = 0,
                SoHocSinhChuaCoDiem = soChuaCoDiem,
                TbLop = null,
                Top = [],
                Bottom = [],
                Histogram = new Dictionary<int, int>(),
                PhanBoMucDiem = MucDiemBucketsEmpty(),
                PhanBoXepLoai = new Dictionary<string, int>(),
            };
        }

        var scores = withTbm.Select(x => x.DiemTBMon!.Value).OrderBy(x => x).ToList();
        var tbLop = GradeCalculator.RoundOneDecimal(scores.Average());
        var topList = withTbm.OrderByDescending(x => x.DiemTBMon).ThenBy(x => x.HoTen).Take(top).ToList();
        var bottomList = withTbm.OrderBy(x => x.DiemTBMon).ThenBy(x => x.HoTen).Take(bottom).ToList();

        var hist = scores.GroupBy(s => (int)Math.Floor((double)s)).ToDictionary(g => g.Key, g => g.Count());
        var muc = MucDiemBucketsEmpty();
        foreach (var s in scores)
        {
            var key = BucketTbm(s);
            muc[key]++;
        }

        var xepLoai = new Dictionary<string, int>();
        foreach (var row in withTbm)
        {
            var k = string.IsNullOrEmpty(row.XepLoai) ? "Khac" : row.XepLoai;
            xepLoai[k] = xepLoai.GetValueOrDefault(k) + 1;
        }

        return new DiemThongKeResponse
        {
            SiSo = data.Count,
            SoHocSinhCoTbm = withTbm.Count,
            SoHocSinhChuaCoDiem = soChuaCoDiem,
            TbLop = tbLop,
            Top = topList,
            Bottom = bottomList,
            Histogram = hist,
            PhanBoMucDiem = muc,
            PhanBoXepLoai = xepLoai,
        };
    }

    private static Dictionary<string, int> MucDiemBucketsEmpty() =>
        new()
        {
            ["0-2"] = 0,
            ["2-4"] = 0,
            ["4-5"] = 0,
            ["5-6.5"] = 0,
            ["6.5-8"] = 0,
            ["8-10"] = 0,
        };

    private static string BucketTbm(decimal tbm) =>
        tbm < 2 ? "0-2" :
        tbm < 4 ? "2-4" :
        tbm < 5 ? "4-5" :
        tbm < 6.5m ? "5-6.5" :
        tbm < 8 ? "6.5-8" : "8-10";

    private async Task<List<BangDiemItemResponse>> BuildBangDiemAsync(string maLop, string maMon, string namHoc, byte hocKy, CancellationToken ct)
    {
        var hsList = await db.HocSinhs.AsNoTracking()
            .Where(h => h.MaLop == maLop)
            .OrderBy(h => h.HoTen)
            .ToListAsync(ct);
        return await BuildBangDiemForHocSinhsAsync(hsList, maMon, namHoc, hocKy, ct);
    }

    private async Task<List<BangDiemItemResponse>> BuildBangDiemKhoiAsync(
        string khoiLop,
        string namHocLop,
        string maMon,
        string namHocKy,
        byte hocKy,
        CancellationToken ct)
    {
        var hsList = await (from h in db.HocSinhs.AsNoTracking()
                            join l in db.LopHocs.AsNoTracking() on h.MaLop equals l.MaLop
                            where l.KhoiLop == khoiLop && l.NamHoc == namHocLop
                            orderby h.MaLop, h.HoTen
                            select h).ToListAsync(ct);
        return await BuildBangDiemForHocSinhsAsync(hsList, maMon, namHocKy, hocKy, ct);
    }

    private async Task<List<BangDiemItemResponse>> BuildBangDiemForHocSinhsAsync(
        List<HocSinh> hsList,
        string maMon,
        string namHoc,
        byte hocKy,
        CancellationToken ct)
    {
        var maHs = hsList.Select(h => h.MaHS).ToList();
        var dsList = await db.DiemSos.AsNoTracking()
            .Include(d => d.ThanhPhans)
            .Where(d => d.MaMon == maMon && d.HocKy == hocKy && d.NamHoc == namHoc && maHs.Contains(d.MaHS))
            .ToListAsync(ct);
        var byHs = dsList.ToDictionary(d => d.MaHS);

        var mh = await db.MonHocs.AsNoTracking().FirstAsync(m => m.MaMon == maMon, ct);

        var result = new List<BangDiemItemResponse>();
        foreach (var hs in hsList)
        {
            byHs.TryGetValue(hs.MaHS, out var ds);
            var (mList, pList) = ds is null ? ([], []) : DiemSoScoreReader.GetComponentLists(ds);
            var tbm = ds is null ? null : DiemSoScoreReader.RecalculateTbm(ds);
            var tt = DiemNhapTrangThai.Compute(ds);
            result.Add(new BangDiemItemResponse
            {
                MaHS = hs.MaHS,
                HoTen = hs.HoTen,
                MaLop = hs.MaLop,
                MaMon = mh.MaMon,
                TenMon = mh.TenMon,
                NamHoc = namHoc,
                HocKy = hocKy,
                DiemMiengs = mList,
                Diem15ps = pList,
                DiemMieng = ds?.DiemMieng,
                Diem15p = ds?.Diem15p,
                DiemGiuaKy = ds?.DiemGiuaKy,
                DiemCuoiKy = ds?.DiemCuoiKy,
                DiemTBMon = tbm,
                TrangThaiNhapDiem = tt,
                XepLoai = GradeCalculator.XepLoaiMon(tbm, ds?.DiemCuoiKy),
                QuaMon = GradeCalculator.PassedMon(tbm, ds?.DiemCuoiKy),
                Liet = GradeCalculator.IsLiet(ds?.DiemCuoiKy),
            });
        }

        return result;
    }

    private static string SnapshotDiem(DiemSo d)
    {
        var (m, p) = DiemSoScoreReader.GetComponentLists(d);
        return JsonSerializer.Serialize(new
        {
            d.MaDiem,
            d.MaHS,
            d.MaMon,
            d.NamHoc,
            d.HocKy,
            Mieng = m,
            Fifteen = p,
            d.DiemGiuaKy,
            d.DiemCuoiKy,
            d.DiemTBMon,
        });
    }

    private static void ApplyScores(DiemSo item, DiemSoUpsertRequest req)
    {
        item.DiemGiuaKy = req.DiemGiuaKy;
        item.DiemCuoiKy = req.DiemCuoiKy;

        var hasLists = req.DiemMiengs is { Count: > 0 } || req.Diem15ps is { Count: > 0 };
        if (hasLists)
        {
            item.DiemMieng = null;
            item.Diem15p = null;
            item.ThanhPhans.Clear();
            var order = 0;
            if (req.DiemMiengs is not null)
            {
                foreach (var d in req.DiemMiengs)
                    item.ThanhPhans.Add(new DiemThanhPhan { Loai = 1, Diem = d, ThuTu = order++ });
            }

            order = 0;
            if (req.Diem15ps is not null)
            {
                foreach (var d in req.Diem15ps)
                    item.ThanhPhans.Add(new DiemThanhPhan { Loai = 2, Diem = d, ThuTu = order++ });
            }
        }
        else
        {
            item.DiemMieng = req.DiemMieng;
            item.Diem15p = req.Diem15p;
            item.ThanhPhans.Clear();
            if (req.DiemMieng.HasValue)
                item.ThanhPhans.Add(new DiemThanhPhan { Loai = 1, Diem = req.DiemMieng.Value, ThuTu = 0 });
            if (req.Diem15p.HasValue)
                item.ThanhPhans.Add(new DiemThanhPhan { Loai = 2, Diem = req.Diem15p.Value, ThuTu = 0 });
        }
    }

    private Task LogRuleViolationAsync(
        string action,
        string? maHS,
        string? maMon,
        string namHoc,
        byte hocKy,
        string violationCode,
        string severity,
        string message,
        CancellationToken ct)
    {
        var metadata = JsonSerializer.Serialize(new
        {
            MaHS = maHS,
            MaMon = maMon,
            NamHoc = namHoc,
            HocKy = hocKy,
            Message = message
        });

        return audit.LogViolationAsync(
            action,
            nameof(DiemSo),
            maHS is null || maMon is null ? $"{maMon}/{namHoc}/HK{hocKy}" : $"{maHS}/{maMon}/{namHoc}/HK{hocKy}",
            violationCode,
            severity,
            metadata,
            ct);
    }
}
