using System.Security.Claims;
using ClosedXML.Excel;
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
[Route("api/reports")]
[Authorize]
public sealed class ReportsController(
    EduTrackDbContext db,
    ICurrentUserService current) : ControllerBase
{
    [HttpGet("dashboard/bgh")]
    [Authorize(Policy = AppPolicies.CanViewReports)]
    public async Task<ActionResult<BghDashboardDto>> DashboardBgh(
        [FromQuery] string namHoc,
        [FromQuery] byte hocKy,
        [FromQuery] string? maLop,
        [FromQuery] string? khoiLop,
        CancellationToken ct = default)
    {
        var filter = BuildFilter(namHoc, hocKy, maLop, khoiLop);
        var hsBase = BuildHocSinhScope(filter);
        var hs = await hsBase.Select(x => new { x.MaHS, x.HoTen }).ToListAsync(ct);
        var maHsSet = hs.Select(x => x.MaHS).ToHashSet();

        var lopCount = await BuildLopScope(filter).CountAsync(ct);
        var diemRows = await db.DiemSos.AsNoTracking()
            .Include(x => x.ThanhPhans)
            .Where(x => x.NamHoc == filter.NamHoc && x.HocKy == filter.HocKy && maHsSet.Contains(x.MaHS))
            .ToListAsync(ct);

        var byHs = diemRows.GroupBy(x => x.MaHS).ToDictionary(g => g.Key, g => g.ToList());
        var tbHocSinh = new List<decimal>();
        var phanBoHocLuc = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var student in hs)
        {
            if (!byHs.TryGetValue(student.MaHS, out var monList) || monList.Count == 0)
                continue;

            var tbms = monList.Select(DiemSoScoreReader.RecalculateTbm).Where(x => x.HasValue).Select(x => x!.Value).ToList();
            if (tbms.Count == 0)
                continue;

            tbHocSinh.Add(GradeCalculator.RoundOneDecimal(tbms.Average()));

            var hocLuc = GradeCalculator.CalcHocLuc(monList.Select(m => (DiemSoScoreReader.RecalculateTbm(m), m.DiemCuoiKy)).ToList()) ?? "Khac";
            phanBoHocLuc[hocLuc] = phanBoHocLuc.GetValueOrDefault(hocLuc) + 1;
        }

        var hocPhiRows = await BuildHocPhiScope(filter).ToListAsync(ct);
        var tongPhaiThu = hocPhiRows.Sum(x => x.SoTien);
        var tongDaThu = hocPhiRows.Where(x => IsPaidStatus(x.TrangThai)).Sum(x => x.SoTien);

        return Ok(new BghDashboardDto
        {
            Filter = filter,
            TongLop = lopCount,
            TongHocSinh = hs.Count,
            SoHocSinhCoDiemTongKet = tbHocSinh.Count,
            DiemTrungBinhToanTruong = tbHocSinh.Count == 0 ? null : GradeCalculator.RoundOneDecimal(tbHocSinh.Average()),
            PhanBoHocLuc = phanBoHocLuc,
            TongHocPhiPhaiThu = tongPhaiThu,
            TongHocPhiDaThu = tongDaThu,
            TyLeThuHocPhi = tongPhaiThu <= 0 ? 0 : Math.Round((tongDaThu / tongPhaiThu) * 100m, 2),
        });
    }

    [HttpGet("dashboard/ketoan")]
    [Authorize(Policy = AppPolicies.CanViewFinance)]
    public async Task<ActionResult<KeToanDashboardDto>> DashboardKeToan(
        [FromQuery] string namHoc,
        [FromQuery] byte hocKy,
        [FromQuery] string? maLop,
        [FromQuery] string? khoiLop,
        CancellationToken ct = default)
    {
        if (!(User.IsInRole(RolePermissionSeeder.Admin) || User.IsInRole(RolePermissionSeeder.Accountant) || User.IsInRole(RolePermissionSeeder.Bgh)))
            return Forbid();

        var filter = BuildFilter(namHoc, hocKy, maLop, khoiLop);
        var hocPhiRows = await BuildHocPhiScope(filter)
            .Select(x => new { x.MaHocPhi, x.SoTien, x.TrangThai, MaLop = x.HocSinh!.MaLop })
            .ToListAsync(ct);

        var tongPhaiThu = hocPhiRows.Sum(x => x.SoTien);
        var tongDaThu = hocPhiRows.Where(x => IsPaidStatus(x.TrangThai)).Sum(x => x.SoTien);
        var thuTheoLop = hocPhiRows
            .Where(x => IsPaidStatus(x.TrangThai))
            .GroupBy(x => x.MaLop)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.SoTien));

        return Ok(new KeToanDashboardDto
        {
            Filter = filter,
            TongHoSoHocPhi = hocPhiRows.Count,
            SoHoSoDaThu = hocPhiRows.Count(x => IsPaidStatus(x.TrangThai)),
            SoHoSoChuaThu = hocPhiRows.Count(x => !IsPaidStatus(x.TrangThai)),
            TongPhaiThu = tongPhaiThu,
            TongDaThu = tongDaThu,
            TongConNo = tongPhaiThu - tongDaThu,
            ThuTheoLop = thuTheoLop
        });
    }

    [HttpGet("dashboard/gvcn")]
    public async Task<ActionResult<GvcnDashboardDto>> DashboardGvcn(
        [FromQuery] string namHoc,
        [FromQuery] byte hocKy,
        [FromQuery] string? maLop,
        CancellationToken ct = default)
    {
        var userId = current.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var maGv = User.FindFirstValue("ma_gv");
        if (string.IsNullOrWhiteSpace(maGv))
            return Forbid();

        var targetClass = await ResolveGvcnClassAsync(maGv, namHoc, maLop, ct);
        if (targetClass is null)
            return Forbid();

        var filter = BuildFilter(namHoc, hocKy, targetClass.MaLop, targetClass.KhoiLop);
        var hs = await db.HocSinhs.AsNoTracking()
            .Where(x => x.MaLop == targetClass.MaLop)
            .Select(x => new { x.MaHS, x.HoTen })
            .ToListAsync(ct);

        var maHsSet = hs.Select(x => x.MaHS).ToHashSet();
        var diemRows = await db.DiemSos.AsNoTracking()
            .Include(x => x.ThanhPhans)
            .Where(x => x.NamHoc == filter.NamHoc && x.HocKy == filter.HocKy && maHsSet.Contains(x.MaHS))
            .ToListAsync(ct);

        var byHs = diemRows.GroupBy(x => x.MaHS).ToDictionary(g => g.Key, g => g.ToList());
        var tbHocSinh = new List<decimal>();
        var canhBao = new List<GvcnCanhBaoItemDto>();

        foreach (var student in hs)
        {
            if (!byHs.TryGetValue(student.MaHS, out var monList) || monList.Count == 0)
                continue;

            var tbmList = monList.Select(DiemSoScoreReader.RecalculateTbm).Where(x => x.HasValue).Select(x => x!.Value).ToList();
            if (tbmList.Count == 0)
                continue;

            var tb = GradeCalculator.RoundOneDecimal(tbmList.Average());
            tbHocSinh.Add(tb);
            var soMonDuoiTb = tbmList.Count(x => x < 5m);

            if (tb < 5m || soMonDuoiTb >= 2)
            {
                canhBao.Add(new GvcnCanhBaoItemDto
                {
                    MaHS = student.MaHS,
                    HoTen = student.HoTen,
                    DiemTrungBinh = tb,
                    SoMonDuoiTrungBinh = soMonDuoiTb
                });
            }
        }

        return Ok(new GvcnDashboardDto
        {
            Filter = filter,
            MaLop = targetClass.MaLop,
            TenLop = targetClass.TenLop,
            SiSo = hs.Count,
            SoHocSinhCoTbHocKy = tbHocSinh.Count,
            DiemTrungBinhLop = tbHocSinh.Count == 0 ? null : GradeCalculator.RoundOneDecimal(tbHocSinh.Average()),
            SoHocSinhNguyCo = canhBao.Count,
            CanhBao = canhBao.OrderBy(x => x.DiemTrungBinh).ThenBy(x => x.HoTen).ToList()
        });
    }

    [HttpGet("download")]
    public async Task<IActionResult> DownloadOneClick(
        [FromQuery] string vaiTro,
        [FromQuery] string namHoc,
        [FromQuery] byte hocKy,
        [FromQuery] string? maLop,
        [FromQuery] string? khoiLop,
        CancellationToken ct = default)
    {
        vaiTro = vaiTro.Trim().ToUpperInvariant();
        using var wb = new XLWorkbook();

        if (vaiTro == "BGH")
        {
            var data = (await DashboardBgh(namHoc, hocKy, maLop, khoiLop, ct)).Value;
            if (data is null) return Forbid();
            var ws = wb.AddWorksheet("BGH");
            ws.Cell(1, 1).Value = "NamHoc"; ws.Cell(1, 2).Value = data.Filter.NamHoc;
            ws.Cell(2, 1).Value = "HocKy"; ws.Cell(2, 2).Value = data.Filter.HocKy;
            ws.Cell(3, 1).Value = "TongLop"; ws.Cell(3, 2).Value = data.TongLop;
            ws.Cell(4, 1).Value = "TongHocSinh"; ws.Cell(4, 2).Value = data.TongHocSinh;
            ws.Cell(5, 1).Value = "DiemTrungBinhToanTruong"; ws.Cell(5, 2).Value = data.DiemTrungBinhToanTruong?.ToString() ?? "";
            ws.Cell(6, 1).Value = "TyLeThuHocPhi"; ws.Cell(6, 2).Value = data.TyLeThuHocPhi;
        }
        else if (vaiTro is "KETOAN" or "KE_TOAN")
        {
            var result = await DashboardKeToan(namHoc, hocKy, maLop, khoiLop, ct);
            if (result.Result is ForbidResult) return Forbid();
            var data = result.Value;
            if (data is null) return Forbid();
            var ws = wb.AddWorksheet("KeToan");
            ws.Cell(1, 1).Value = "NamHoc"; ws.Cell(1, 2).Value = data.Filter.NamHoc;
            ws.Cell(2, 1).Value = "HocKy"; ws.Cell(2, 2).Value = data.Filter.HocKy;
            ws.Cell(3, 1).Value = "TongPhaiThu"; ws.Cell(3, 2).Value = data.TongPhaiThu;
            ws.Cell(4, 1).Value = "TongDaThu"; ws.Cell(4, 2).Value = data.TongDaThu;
            ws.Cell(5, 1).Value = "TongConNo"; ws.Cell(5, 2).Value = data.TongConNo;
        }
        else if (vaiTro == "GVCN")
        {
            var result = await DashboardGvcn(namHoc, hocKy, maLop, ct);
            if (result.Result is ForbidResult) return Forbid();
            var data = result.Value;
            if (data is null) return Forbid();
            var ws = wb.AddWorksheet("GVCN");
            ws.Cell(1, 1).Value = "MaLop"; ws.Cell(1, 2).Value = data.MaLop;
            ws.Cell(2, 1).Value = "TenLop"; ws.Cell(2, 2).Value = data.TenLop ?? "";
            ws.Cell(3, 1).Value = "SiSo"; ws.Cell(3, 2).Value = data.SiSo;
            ws.Cell(4, 1).Value = "SoHocSinhNguyCo"; ws.Cell(4, 2).Value = data.SoHocSinhNguyCo;

            var start = 7;
            ws.Cell(start, 1).Value = "MaHS";
            ws.Cell(start, 2).Value = "HoTen";
            ws.Cell(start, 3).Value = "DiemTrungBinh";
            ws.Cell(start, 4).Value = "SoMonDuoiTrungBinh";
            var row = start + 1;
            foreach (var c in data.CanhBao)
            {
                ws.Cell(row, 1).Value = c.MaHS;
                ws.Cell(row, 2).Value = c.HoTen;
                ws.Cell(row, 3).Value = c.DiemTrungBinh?.ToString() ?? "";
                ws.Cell(row, 4).Value = c.SoMonDuoiTrungBinh;
                row++;
            }
        }
        else
        {
            return Problem("vaiTro chỉ hỗ trợ BGH, KeToan, GVCN", statusCode: 400);
        }

        await using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(
            ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"bao-cao-{vaiTro.ToLowerInvariant()}-{namHoc}-hk{hocKy}.xlsx");
    }

    private OperationalReportFilterDto BuildFilter(string namHoc, byte hocKy, string? maLop, string? khoiLop) =>
        new()
        {
            NamHoc = string.IsNullOrWhiteSpace(namHoc) ? "2025-2026" : namHoc.Trim(),
            HocKy = hocKy == 0 ? (byte)1 : hocKy,
            MaLop = string.IsNullOrWhiteSpace(maLop) ? null : maLop.Trim(),
            KhoiLop = string.IsNullOrWhiteSpace(khoiLop) ? null : khoiLop.Trim()
        };

    private IQueryable<LopHoc> BuildLopScope(OperationalReportFilterDto filter)
    {
        var q = db.LopHocs.AsNoTracking().AsQueryable();
        q = q.Where(x => x.NamHoc == filter.NamHoc);
        if (!string.IsNullOrWhiteSpace(filter.MaLop)) q = q.Where(x => x.MaLop == filter.MaLop);
        if (!string.IsNullOrWhiteSpace(filter.KhoiLop)) q = q.Where(x => x.KhoiLop == filter.KhoiLop);
        return q;
    }

    private IQueryable<HocSinh> BuildHocSinhScope(OperationalReportFilterDto filter)
    {
        var q = from hs in db.HocSinhs.AsNoTracking()
                join lop in db.LopHocs.AsNoTracking() on hs.MaLop equals lop.MaLop
                where lop.NamHoc == filter.NamHoc
                select hs;

        if (!string.IsNullOrWhiteSpace(filter.MaLop))
            q = q.Where(x => x.MaLop == filter.MaLop);
        if (!string.IsNullOrWhiteSpace(filter.KhoiLop))
            q = q.Where(x => x.LopHoc != null && x.LopHoc.KhoiLop == filter.KhoiLop);
        return q;
    }

    private IQueryable<HocPhi> BuildHocPhiScope(OperationalReportFilterDto filter)
    {
        var q = db.HocPhis.AsNoTracking().Include(x => x.HocSinh).ThenInclude(h => h!.LopHoc).AsQueryable();
        q = q.Where(x => x.HocKy == filter.HocKy && x.HocSinh != null && x.HocSinh.LopHoc != null && x.HocSinh.LopHoc.NamHoc == filter.NamHoc);
        if (!string.IsNullOrWhiteSpace(filter.MaLop)) q = q.Where(x => x.HocSinh!.MaLop == filter.MaLop);
        if (!string.IsNullOrWhiteSpace(filter.KhoiLop)) q = q.Where(x => x.HocSinh!.LopHoc!.KhoiLop == filter.KhoiLop);
        return q;
    }

    private static bool IsPaidStatus(string? status)
        => !string.IsNullOrWhiteSpace(status) &&
           (status.Equals("DaDong", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Paid", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Đã đóng", StringComparison.OrdinalIgnoreCase));

    private async Task<LopHoc?> ResolveGvcnClassAsync(string maGv, string namHoc, string? maLop, CancellationToken ct)
    {
        var q = db.LopHocs.AsNoTracking()
            .Where(x => x.MaGVChuNhiem == maGv && x.NamHoc == namHoc);

        if (!string.IsNullOrWhiteSpace(maLop))
            q = q.Where(x => x.MaLop == maLop);

        return await q.OrderBy(x => x.MaLop).FirstOrDefaultAsync(ct);
    }
}
