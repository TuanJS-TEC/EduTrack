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
    [HttpGet]
    [Authorize(Policy = AppPolicies.CanViewScores)]
    public async Task<ActionResult<List<DiemSo>>> GetAll(
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
        return Ok(list);
    }

    [HttpGet("{maDiem:int}")]
    [Authorize(Policy = AppPolicies.CanViewScores)]
    public async Task<ActionResult<DiemSo>> GetById([FromRoute] int maDiem, CancellationToken ct)
    {
        var userId = current.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var item = await db.DiemSos.AsNoTracking().Include(x => x.ThanhPhans).FirstOrDefaultAsync(x => x.MaDiem == maDiem, ct);
        if (item is null) return NotFound();
        if (!await access.CanViewStudentAsync(userId, item.MaHS, ct)) return Forbid();
        return Ok(item);
    }

    [HttpPost("upsert")]
    [Authorize(Policy = AppPolicies.CanEditScores)]
    public async Task<ActionResult<DiemSo>> Upsert([FromBody] DiemSoUpsertRequest req, CancellationToken ct)
    {
        var userId = current.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (string.IsNullOrWhiteSpace(req.MaHS) || string.IsNullOrWhiteSpace(req.MaMon))
            return ProblemResponses.Of(400, "MaHS/MaMon không hợp lệ", ApiErrorCodes.Validation);

        if (!EduCodeFormats.IsValidSchoolYear(req.NamHoc))
            return ProblemResponses.Of(400, "NamHoc không đúng định dạng YYYY-YYYY", ApiErrorCodes.Validation);

        if (!await access.CanEditScoreAsync(userId, req.MaHS, req.MaMon, req.NamHoc, req.HocKy, ct))
            return Forbid();

        var err = ScoreInputValidator.Validate(req.DiemGiuaKy)
                  ?? ScoreInputValidator.Validate(req.DiemCuoiKy)
                  ?? ScoreInputValidator.ValidateMany(req.DiemMiengs)
                  ?? ScoreInputValidator.ValidateMany(req.Diem15ps)
                  ?? ScoreInputValidator.Validate(req.DiemMieng)
                  ?? ScoreInputValidator.Validate(req.Diem15p);
        if (err is not null)
            return ProblemResponses.Of(400, err, ApiErrorCodes.ScoreOutOfRange);

        var ky = await db.KyHocs.AsNoTracking().FirstOrDefaultAsync(k => k.NamHoc == req.NamHoc && k.HocKy == req.HocKy, ct);
        if (ky?.Locked == true)
            return ProblemResponses.Of(409, "Học kỳ đã chốt, không được sửa điểm", ApiErrorCodes.SemesterLocked);

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

        return Ok(tracked);
    }

    [HttpPost("bulk-upsert")]
    [Authorize(Policy = AppPolicies.CanEditScores)]
    public async Task<ActionResult<int>> BulkUpsert([FromBody] DiemSoBulkUpsertRequest req, CancellationToken ct)
    {
        var userId = current.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (string.IsNullOrWhiteSpace(req.MaLop) || string.IsNullOrWhiteSpace(req.MaMon))
            return ProblemResponses.Of(400, "Thiếu MaLop/MaMon");

        var ky = await db.KyHocs.AsNoTracking().FirstOrDefaultAsync(k => k.NamHoc == req.NamHoc && k.HocKy == req.HocKy, ct);
        if (ky?.Locked == true)
            return ProblemResponses.Of(409, "Học kỳ đã chốt, không được sửa điểm", ApiErrorCodes.SemesterLocked);

        var count = 0;
        foreach (var row in req.Rows)
        {
            row.NamHoc = req.NamHoc;
            row.HocKy = req.HocKy;
            row.MaMon = req.MaMon;

            var inLop = await db.HocSinhs.AnyAsync(h => h.MaHS == row.MaHS && h.MaLop == req.MaLop, ct);
            if (!inLop) continue;

            var err = ScoreInputValidator.Validate(row.DiemGiuaKy)
                      ?? ScoreInputValidator.Validate(row.DiemCuoiKy)
                      ?? ScoreInputValidator.ValidateMany(row.DiemMiengs)
                      ?? ScoreInputValidator.ValidateMany(row.Diem15ps)
                      ?? ScoreInputValidator.Validate(row.DiemMieng)
                      ?? ScoreInputValidator.Validate(row.Diem15p);
            if (err is not null)
                return ProblemResponses.Of(400, err, ApiErrorCodes.ScoreOutOfRange);

            if (!await access.CanEditScoreAsync(userId, row.MaHS, row.MaMon, row.NamHoc, row.HocKy, ct))
                return Forbid();

            var item = await db.DiemSos.Include(x => x.ThanhPhans).FirstOrDefaultAsync(
                x => x.MaHS == row.MaHS && x.MaMon == row.MaMon && x.NamHoc == row.NamHoc && x.HocKy == row.HocKy, ct);

            var oldSnapshot = item is null ? null : SnapshotDiem(item);
            if (item is null)
            {
                item = new DiemSo { MaHS = row.MaHS, MaMon = row.MaMon, NamHoc = row.NamHoc, HocKy = row.HocKy };
                db.DiemSos.Add(item);
            }

            ApplyScores(item, row);
            item.DiemTBMon = DiemSoScoreReader.RecalculateTbm(item);
            await db.SaveChangesAsync(ct);

            var fresh = await db.DiemSos.Include(x => x.ThanhPhans).AsNoTracking().FirstAsync(x => x.MaDiem == item.MaDiem, ct);
            await audit.LogAsync("DiemSo.BulkUpsert", nameof(DiemSo), item.MaDiem.ToString(), oldSnapshot, SnapshotDiem(fresh), ct);
            count++;
        }

        return Ok(count);
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
            return ProblemResponses.Of(409, "Học kỳ đã chốt", ApiErrorCodes.SemesterLocked);

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

        var data = await BuildBangDiemAsync(maLop, maMon, namHoc, hocKy, ct);
        return Ok(data);
    }

    [HttpGet("bangdiem/excel")]
    [Authorize(Policy = AppPolicies.CanViewScores)]
    public async Task<IActionResult> BangDiemExcel([FromQuery] string maLop, [FromQuery] string maMon, [FromQuery] string namHoc, [FromQuery] byte hocKy, CancellationToken ct)
    {
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
            r++;
        }

        await using var ms = new MemoryStream();
        wb.SaveAs(ms);
        var bytes = ms.ToArray();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"bang-diem-{maLop}-{maMon}-hk{hocKy}.xlsx");
    }

    [HttpGet("thong-ke")]
    [Authorize(Policy = AppPolicies.CanViewScores)]
    public async Task<ActionResult<DiemThongKeResponse>> ThongKe([FromQuery] string maLop, [FromQuery] string maMon, [FromQuery] string namHoc, [FromQuery] byte hocKy, CancellationToken ct)
    {
        var data = await BuildBangDiemAsync(maLop, maMon, namHoc, hocKy, ct);

        var scores = data.Where(x => x.DiemTBMon.HasValue).Select(x => x.DiemTBMon!.Value).OrderBy(x => x).ToList();
        if (scores.Count == 0)
            return Ok(new DiemThongKeResponse { SiSo = data.Count, TbLop = null, Top = [], Bottom = [] });

        var tbLop = GradeCalculator.RoundOneDecimal(scores.Average());
        var top = data.Where(x => x.DiemTBMon.HasValue).OrderByDescending(x => x.DiemTBMon).ThenBy(x => x.HoTen).Take(5).ToList();
        var bottom = data.Where(x => x.DiemTBMon.HasValue).OrderBy(x => x.DiemTBMon).ThenBy(x => x.HoTen).Take(5).ToList();

        var hist = scores.GroupBy(s => (int)Math.Floor((double)s)).ToDictionary(g => g.Key, g => g.Count());
        return Ok(new DiemThongKeResponse
        {
            SiSo = data.Count,
            TbLop = tbLop,
            Top = top,
            Bottom = bottom,
            Histogram = hist
        });
    }

    private async Task<List<BangDiemItemResponse>> BuildBangDiemAsync(string maLop, string maMon, string namHoc, byte hocKy, CancellationToken ct)
    {
        var hsList = await db.HocSinhs.AsNoTracking()
            .Where(h => h.MaLop == maLop)
            .OrderBy(h => h.HoTen)
            .ToListAsync(ct);

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
                XepLoai = GradeCalculator.XepLoaiMon(tbm, ds?.DiemCuoiKy),
                QuaMon = GradeCalculator.PassedMon(tbm, ds?.DiemCuoiKy),
                Liet = GradeCalculator.IsLiet(ds?.DiemCuoiKy)
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
            d.DiemTBMon
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
}
