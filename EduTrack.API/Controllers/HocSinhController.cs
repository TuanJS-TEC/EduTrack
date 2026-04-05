using ClosedXML.Excel;
using EduTrack.API.Authorization;
using EduTrack.API.Data;
using EduTrack.API.DTOs;
using EduTrack.API.Models;
using EduTrack.API.Services;
using EduTrack.API.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Controllers;

[ApiController]
[Route("api/hocsinh")]
[Authorize]
public sealed class HocSinhController(
    EduTrackDbContext db,
    IAccessControlService access,
    ICurrentUserService current) : ControllerBase
{
    private static decimal? TinhDiemTbChung(IEnumerable<DiemSo> diemSos)
    {
        var tbms = diemSos.Select(DiemSoScoreReader.RecalculateTbm).Where(x => x.HasValue).Select(x => x!.Value).ToList();
        if (tbms.Count == 0) return null;
        return GradeCalculator.RoundOneDecimal(tbms.Average());
    }

    private static string? XepHanhKiem(decimal? tb)
    {
        if (tb is null) return null;
        if (tb >= 8.0m) return "Tốt";
        if (tb >= 6.5m) return "Khá";
        if (tb >= 5.0m) return "Trung bình";
        return "Yếu";
    }

    private static HocSinhResponse ToDto(HocSinh hs) =>
        new()
        {
            MaHS = hs.MaHS,
            HoTen = hs.HoTen,
            NgaySinh = hs.NgaySinh,
            DiaChi = hs.DiaChi,
            MaLop = hs.MaLop,
            Email_PhuHuynh = hs.Email_PhuHuynh,
            SDT_PhuHuynh = hs.SDT_PhuHuynh,
            TrangThai = hs.TrangThai,
            DiemTB = TinhDiemTbChung(hs.DiemSos),
            HanhKiem = XepHanhKiem(TinhDiemTbChung(hs.DiemSos)),
        };

    [HttpGet("paged")]
    [Authorize(Policy = AppPolicies.CanViewStudents)]
    public async Task<ActionResult<PagedResult<HocSinhResponse>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? maLop = null,
        [FromQuery] string? khoiLop = null,
        [FromQuery] string? namHoc = null,
        [FromQuery] string? sort = "HoTen",
        [FromQuery] bool sortDesc = false,
        CancellationToken ct = default)
    {
        var userId = current.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var q = db.HocSinhs.AsNoTracking().Include(h => h.LopHoc).Include(h => h.DiemSos).ThenInclude(d => d.ThanhPhans).AsQueryable();

        if (User.IsInRole(RolePermissionSeeder.Parent))
        {
            var codes = await access.GetParentStudentCodesAsync(userId, ct);
            q = q.Where(h => codes.Contains(h.MaHS));
        }

        if (!string.IsNullOrWhiteSpace(maLop)) q = q.Where(x => x.MaLop == maLop);
        if (!string.IsNullOrWhiteSpace(khoiLop)) q = q.Where(x => x.LopHoc != null && x.LopHoc.KhoiLop == khoiLop);
        if (!string.IsNullOrWhiteSpace(namHoc)) q = q.Where(x => x.LopHoc != null && x.LopHoc.NamHoc == namHoc);

        var total = await q.CountAsync(ct);
        q = (sort, sortDesc) switch
        {
            ("MaHS", true) => q.OrderByDescending(x => x.MaHS),
            ("MaHS", false) => q.OrderBy(x => x.MaHS),
            ("HoTen", true) => q.OrderByDescending(x => x.HoTen),
            _ => q.OrderBy(x => x.HoTen),
        };

        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return Ok(new PagedResult<HocSinhResponse>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Items = items.Select(ToDto).ToList()
        });
    }

    [HttpGet]
    [Authorize(Policy = AppPolicies.CanViewStudents)]
    public async Task<ActionResult<List<HocSinhResponse>>> GetAll([FromQuery] string? maLop, CancellationToken ct)
    {
        var userId = current.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var query = db.HocSinhs
            .AsNoTracking()
            .Include(h => h.DiemSos).ThenInclude(d => d.ThanhPhans)
            .AsQueryable();

        if (User.IsInRole(RolePermissionSeeder.Parent))
        {
            var codes = await access.GetParentStudentCodesAsync(userId, ct);
            query = query.Where(x => codes.Contains(x.MaHS));
        }

        if (!string.IsNullOrWhiteSpace(maLop))
            query = query.Where(x => x.MaLop == maLop);

        var list = await query.OrderBy(x => x.HoTen).ToListAsync(ct);
        return Ok(list.Select(ToDto));
    }

    [HttpGet("{maHS}")]
    [Authorize(Policy = AppPolicies.CanViewStudents)]
    public async Task<ActionResult<HocSinhResponse>> GetById([FromRoute] string maHS, CancellationToken ct)
    {
        var userId = current.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        if (!await access.CanViewStudentAsync(userId, maHS, ct)) return Forbid();

        var hs = await db.HocSinhs
            .AsNoTracking()
            .Include(h => h.DiemSos).ThenInclude(d => d.ThanhPhans)
            .FirstOrDefaultAsync(x => x.MaHS == maHS, ct);
        return hs is null ? NotFound() : Ok(ToDto(hs));
    }

    [HttpGet("{maHS}/ho-so")]
    [Authorize(Policy = AppPolicies.CanViewStudents)]
    public async Task<ActionResult<HocSinhFullProfileResponse>> GetHoSoDayDu(
        [FromRoute] string maHS,
        [FromQuery] string? namHoc = null,
        [FromQuery] byte? hocKy = null,
        CancellationToken ct = default)
    {
        var userId = current.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        if (!await access.CanViewStudentAsync(userId, maHS, ct)) return Forbid();

        var hs = await db.HocSinhs
            .AsNoTracking()
            .Include(h => h.LopHoc)
            .Include(h => h.DiemSos).ThenInclude(d => d.ThanhPhans)
            .Include(h => h.DiemSos).ThenInclude(d => d.MonHoc)
            .FirstOrDefaultAsync(x => x.MaHS == maHS, ct);
        if (hs is null) return NotFound();

        var diemList = hs.DiemSos.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(namHoc)) diemList = diemList.Where(d => d.NamHoc == namHoc);
        if (hocKy.HasValue) diemList = diemList.Where(d => d.HocKy == hocKy.Value);

        var diemTheoKy = diemList
            .GroupBy(d => new { d.NamHoc, d.HocKy })
            .Select(g => new DiemKyTomTatDto
            {
                NamHoc = g.Key.NamHoc,
                HocKy = g.Key.HocKy,
                Mon = g.OrderBy(m => m.MaMon).Select(m =>
                {
                    var tbm = DiemSoScoreReader.RecalculateTbm(m);
                    var lists = DiemSoScoreReader.GetComponentLists(m);
                    return new BangDiemItemResponse
                    {
                        MaHS = maHS,
                        HoTen = hs.HoTen,
                        MaLop = hs.MaLop,
                        MaMon = m.MaMon,
                        TenMon = m.MonHoc != null ? m.MonHoc.TenMon : "",
                        NamHoc = m.NamHoc,
                        HocKy = m.HocKy,
                        DiemMiengs = lists.Mieng,
                        Diem15ps = lists.Fifteen,
                        DiemMieng = m.DiemMieng,
                        Diem15p = m.Diem15p,
                        DiemGiuaKy = m.DiemGiuaKy,
                        DiemCuoiKy = m.DiemCuoiKy,
                        DiemTBMon = tbm,
                        XepLoai = GradeCalculator.XepLoaiMon(tbm, m.DiemCuoiKy),
                        QuaMon = GradeCalculator.PassedMon(tbm, m.DiemCuoiKy),
                        Liet = GradeCalculator.IsLiet(m.DiemCuoiKy)
                    };
                }).ToList(),
                HocLuc = null
            })
            .OrderBy(x => x.NamHoc).ThenBy(x => x.HocKy)
            .ToList();

        foreach (var ky in diemTheoKy)
        {
            var pairs = ky.Mon.Select(m => (m.DiemTBMon, m.DiemCuoiKy)).ToList();
            ky.HocLuc = GradeCalculator.CalcHocLuc(pairs.Select(p => (p.DiemTBMon, p.DiemCuoiKy)).ToList());
        }

        var hocPhis = await db.HocPhis.AsNoTracking().Where(x => x.MaHS == maHS).OrderByDescending(x => x.HocKy).ToListAsync(ct);

        var thongBaos = await db.ThongBaos.AsNoTracking()
            .Where(t => t.MaHS == null || t.MaHS == maHS)
            .OrderByDescending(t => t.NgayGui)
            .Take(50)
            .Select(t => new ThongBaoTomTatDto { MaTB = t.MaTB, TieuDe = t.TieuDe ?? "", LoaiTB = t.LoaiTB, NgayGui = t.NgayGui })
            .ToListAsync(ct);

        return Ok(new HocSinhFullProfileResponse
        {
            HocSinh = ToDto(hs),
            TenLop = hs.LopHoc?.TenLop,
            KhoiLop = hs.LopHoc?.KhoiLop,
            NamHocLop = hs.LopHoc?.NamHoc,
            DiemTheoKy = diemTheoKy,
            HocPhis = await CanViewPhiAsync(userId, ct) ? hocPhis : [],
            ThongBaos = thongBaos
        });
    }

    [HttpGet("{maHS}/lich-su-hoc-tap")]
    [Authorize(Policy = AppPolicies.CanViewStudents)]
    public async Task<ActionResult<List<LichSuHocTapResponse>>> GetLichSuHocTap([FromRoute] string maHS, CancellationToken ct)
    {
        var userId = current.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        if (!await access.CanViewStudentAsync(userId, maHS, ct)) return Forbid();

        var ds = await db.DiemSos.AsNoTracking()
            .Include(x => x.ThanhPhans)
            .Where(x => x.MaHS == maHS)
            .ToListAsync(ct);

        var result = ds
            .GroupBy(x => new { x.NamHoc, x.HocKy })
            .Select(g =>
            {
                var tbms = g.Select(DiemSoScoreReader.RecalculateTbm).Where(x => x.HasValue).Select(x => x!.Value).ToList();
                var tbc = tbms.Count == 0 ? (decimal?)null : GradeCalculator.RoundOneDecimal(tbms.Average());
                var pairs = g.Select(m => (DiemSoScoreReader.RecalculateTbm(m), m.DiemCuoiKy)).ToList();
                var hl = GradeCalculator.CalcHocLuc(pairs.Select(p => (p.Item1, p.Item2)).ToList());
                return new LichSuHocTapResponse
                {
                    NamHoc = g.Key.NamHoc,
                    HocKy = g.Key.HocKy,
                    Tbc = tbc,
                    HocLuc = hl,
                    SoMonCoDiem = tbms.Count
                };
            })
            .OrderBy(x => x.NamHoc).ThenBy(x => x.HocKy)
            .ToList();

        return Ok(result);
    }

    [HttpGet("export/excel")]
    [Authorize(Policy = AppPolicies.CanViewStudents)]
    public async Task<IActionResult> ExportExcel([FromQuery] string? maLop, [FromQuery] string? namHoc, CancellationToken ct)
    {
        var userId = current.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var q = db.HocSinhs.AsNoTracking().Include(h => h.LopHoc).AsQueryable();
        if (User.IsInRole(RolePermissionSeeder.Parent))
        {
            var codes = await access.GetParentStudentCodesAsync(userId, ct);
            q = q.Where(h => codes.Contains(h.MaHS));
        }

        if (!string.IsNullOrWhiteSpace(maLop)) q = q.Where(x => x.MaLop == maLop);
        if (!string.IsNullOrWhiteSpace(namHoc)) q = q.Where(x => x.LopHoc != null && x.LopHoc.NamHoc == namHoc);

        var list = await q.OrderBy(x => x.MaLop).ThenBy(x => x.HoTen).ToListAsync(ct);

        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Hoc sinh");
        ws.Cell(1, 1).Value = "MaHS";
        ws.Cell(1, 2).Value = "HoTen";
        ws.Cell(1, 3).Value = "NgaySinh";
        ws.Cell(1, 4).Value = "MaLop";
        ws.Cell(1, 5).Value = "TrangThai";
        ws.Cell(1, 6).Value = "Email PH";
        ws.Cell(1, 7).Value = "SDT PH";
        var r = 2;
        foreach (var x in list)
        {
            ws.Cell(r, 1).Value = x.MaHS;
            ws.Cell(r, 2).Value = x.HoTen;
            ws.Cell(r, 3).Value = x.NgaySinh?.ToString("yyyy-MM-dd") ?? "";
            ws.Cell(r, 4).Value = x.MaLop;
            ws.Cell(r, 5).Value = x.TrangThai;
            ws.Cell(r, 6).Value = x.Email_PhuHuynh ?? "";
            ws.Cell(r, 7).Value = x.SDT_PhuHuynh ?? "";
            r++;
        }

        await using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "hoc-sinh.xlsx");
    }

    [HttpPost("import/excel")]
    [Authorize(Policy = AppPolicies.CanEditStudents)]
    public async Task<ActionResult<int>> ImportExcel([FromForm] IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0) return BadRequest("File rỗng");

        await using var stream = file.OpenReadStream();
        using var wb = new XLWorkbook(stream);
        var ws = wb.Worksheet(1);
        var rows = ws.RangeUsed()?.RowsUsed().Skip(1) ?? Enumerable.Empty<IXLRangeRow>();
        var n = 0;
        foreach (var row in rows)
        {
            var maHs = row.Cell(1).GetString().Trim();
            var hoTen = row.Cell(2).GetString().Trim();
            var maLop = row.Cell(4).GetString().Trim();
            if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(maLop)) continue;

            if (string.IsNullOrWhiteSpace(maHs))
                maHs = await StudentCodeGenerator.NextAsync(db, DateTime.UtcNow.Year, ct);

            if (!EduCodeFormats.IsValidStudentCode(maHs) || !EduCodeFormats.IsValidClassCode(maLop))
                continue;

            if (await db.HocSinhs.AnyAsync(x => x.MaHS == maHs, ct))
            {
                var exist = await db.HocSinhs.FirstAsync(x => x.MaHS == maHs, ct);
                exist.HoTen = hoTen;
                exist.MaLop = maLop;
            }
            else
            {
                db.HocSinhs.Add(new HocSinh
                {
                    MaHS = maHs,
                    HoTen = hoTen,
                    MaLop = maLop,
                    NgaySinh = null,
                    TrangThai = "Đang học"
                });
            }

            n++;
        }

        await db.SaveChangesAsync(ct);
        return Ok(n);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.CanEditStudents)]
    public async Task<ActionResult> Create([FromBody] HocSinhCreateRequest req, CancellationToken ct)
    {
        var maHs = string.IsNullOrWhiteSpace(req.MaHS)
            ? await StudentCodeGenerator.NextAsync(db, req.NamNhapHoc, ct)
            : req.MaHS.Trim();

        if (!EduCodeFormats.IsValidStudentCode(maHs))
            return BadRequest("MaHS không đúng định dạng HS-YYYY-xxxxx");

        if (!EduCodeFormats.IsValidClassCode(req.MaLop))
            return BadRequest("MaLop không đúng định dạng [Khối][Lớp]-[Năm]");

        if (await db.HocSinhs.AnyAsync(x => x.MaHS == maHs, ct))
            return Conflict(new { message = "MaHS đã tồn tại" });

        var hs = new HocSinh
        {
            MaHS = maHs,
            HoTen = req.HoTen,
            NgaySinh = req.NgaySinh,
            DiaChi = req.DiaChi,
            MaLop = req.MaLop,
            Email_PhuHuynh = req.Email_PhuHuynh,
            SDT_PhuHuynh = req.SDT_PhuHuynh,
            TrangThai = string.IsNullOrWhiteSpace(req.TrangThai) ? "Đang học" : req.TrangThai
        };
        db.HocSinhs.Add(hs);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { maHS = hs.MaHS }, ToDto(hs));
    }

    [HttpPut("{maHS}")]
    [Authorize(Policy = AppPolicies.CanEditStudents)]
    public async Task<ActionResult> Update([FromRoute] string maHS, [FromBody] HocSinhUpdateRequest input, CancellationToken ct)
    {
        var hs = await db.HocSinhs.FirstOrDefaultAsync(x => x.MaHS == maHS, ct);
        if (hs is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(input.MaLop) && !EduCodeFormats.IsValidClassCode(input.MaLop))
            return BadRequest("MaLop không đúng định dạng");

        hs.HoTen = input.HoTen;
        hs.NgaySinh = input.NgaySinh;
        hs.DiaChi = input.DiaChi;
        if (!string.IsNullOrWhiteSpace(input.MaLop)) hs.MaLop = input.MaLop;
        hs.Email_PhuHuynh = input.Email_PhuHuynh;
        hs.SDT_PhuHuynh = input.SDT_PhuHuynh;
        hs.TrangThai = string.IsNullOrWhiteSpace(input.TrangThai) ? hs.TrangThai : input.TrangThai!;

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{maHS}")]
    [Authorize(Policy = AppPolicies.CanEditStudents)]
    public async Task<ActionResult> Delete([FromRoute] string maHS, CancellationToken ct)
    {
        var hs = await db.HocSinhs.FirstOrDefaultAsync(x => x.MaHS == maHS, ct);
        if (hs is null) return NotFound();

        db.HocSinhs.Remove(hs);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task<bool> CanViewPhiAsync(string userId, CancellationToken ct) =>
        await access.CanViewFinanceAsync(userId, ct);
}
