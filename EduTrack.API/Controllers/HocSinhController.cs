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
            TenLop = hs.LopHoc?.TenLop,
            KhoiLop = hs.LopHoc?.KhoiLop,
            NamHocLop = hs.LopHoc?.NamHoc,
            Email_PhuHuynh = hs.Email_PhuHuynh,
            SDT_PhuHuynh = hs.SDT_PhuHuynh,
            TrangThai = hs.TrangThai,
            DiemTB = TinhDiemTbChung(hs.DiemSos),
            HanhKiem = XepHanhKiem(TinhDiemTbChung(hs.DiemSos)),
        };

    private static HocPhiResponseDto ToHocPhiDto(HocPhi h) =>
        new()
        {
            MaHocPhi = h.MaHocPhi,
            MaHS = h.MaHS,
            HocKy = h.HocKy,
            SoTien = h.SoTien,
            NgayDong = h.NgayDong,
            TrangThai = h.TrangThai,
        };

    private static DateTime? TryParseExcelDate(IXLCell cell)
    {
        if (cell.IsEmpty()) return null;
        if (cell.DataType == XLDataType.DateTime)
        {
            try
            {
                return cell.GetDateTime();
            }
            catch
            {
                // fall through
            }
        }

        var str = cell.GetString().Trim();
        return string.IsNullOrEmpty(str) ? null : DateTime.TryParse(str, out var dt) ? dt : null;
    }

    [HttpGet("paged")]
    [Authorize(Policy = AppPolicies.CanViewStudents)]
    public async Task<ActionResult<PagedResult<HocSinhResponse>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? maLop = null,
        [FromQuery] string? khoiLop = null,
        [FromQuery] string? namHoc = null,
        [FromQuery] string? trangThai = null,
        [FromQuery] string? sort = "HoTen",
        [FromQuery] bool sortDesc = false,
        CancellationToken ct = default)
    {
        var userId = current.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var queryable = db.HocSinhs.AsNoTracking().Include(h => h.LopHoc).Include(h => h.DiemSos).ThenInclude(d => d.ThanhPhans).AsQueryable();

        if (User.IsInRole(RolePermissionSeeder.Parent))
        {
            var codes = await access.GetParentStudentCodesAsync(userId, ct);
            queryable = queryable.Where(h => codes.Contains(h.MaHS));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            queryable = queryable.Where(x =>
                x.HoTen.Contains(s) ||
                x.MaHS.Contains(s) ||
                (x.DiaChi != null && x.DiaChi.Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(maLop)) queryable = queryable.Where(x => x.MaLop == maLop);
        if (!string.IsNullOrWhiteSpace(khoiLop)) queryable = queryable.Where(x => x.LopHoc != null && x.LopHoc.KhoiLop == khoiLop);
        if (!string.IsNullOrWhiteSpace(namHoc)) queryable = queryable.Where(x => x.LopHoc != null && x.LopHoc.NamHoc == namHoc);
        if (!string.IsNullOrWhiteSpace(trangThai)) queryable = queryable.Where(x => x.TrangThai == trangThai);

        var total = await queryable.CountAsync(ct);
        queryable = (sort, sortDesc) switch
        {
            ("MaHS", true) => queryable.OrderByDescending(x => x.MaHS),
            ("MaHS", false) => queryable.OrderBy(x => x.MaHS),
            ("MaLop", true) => queryable.OrderByDescending(x => x.MaLop).ThenBy(x => x.HoTen),
            ("MaLop", false) => queryable.OrderBy(x => x.MaLop).ThenBy(x => x.HoTen),
            ("TrangThai", true) => queryable.OrderByDescending(x => x.TrangThai).ThenBy(x => x.HoTen),
            ("TrangThai", false) => queryable.OrderBy(x => x.TrangThai).ThenBy(x => x.HoTen),
            ("NgaySinh", true) => queryable.OrderByDescending(x => x.NgaySinh).ThenBy(x => x.HoTen),
            ("NgaySinh", false) => queryable.OrderBy(x => x.NgaySinh).ThenBy(x => x.HoTen),
            ("NamHoc", true) => queryable.OrderByDescending(x => x.LopHoc!.NamHoc).ThenBy(x => x.HoTen),
            ("NamHoc", false) => queryable.OrderBy(x => x.LopHoc!.NamHoc).ThenBy(x => x.HoTen),
            ("KhoiLop", true) => queryable.OrderByDescending(x => x.LopHoc!.KhoiLop).ThenBy(x => x.MaLop).ThenBy(x => x.HoTen),
            ("KhoiLop", false) => queryable.OrderBy(x => x.LopHoc!.KhoiLop).ThenBy(x => x.MaLop).ThenBy(x => x.HoTen),
            ("HoTen", true) => queryable.OrderByDescending(x => x.HoTen),
            _ => queryable.OrderBy(x => x.HoTen),
        };

        var items = await queryable.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
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
            .Include(h => h.LopHoc)
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
            .Include(h => h.LopHoc)
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
                        TrangThaiNhapDiem = DiemNhapTrangThai.Compute(m),
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
            .Select(t => new ThongBaoTomTatDto
            {
                MaTB = t.MaTB,
                TieuDe = t.TieuDe ?? "",
                LoaiTB = t.LoaiTB,
                NgayGui = t.NgayGui,
                NoiDung = t.NoiDung
            })
            .ToListAsync(ct);

        var phiDtos = await CanViewPhiAsync(userId, ct)
            ? hocPhis.Select(ToHocPhiDto).ToList()
            : [];

        return Ok(new HocSinhFullProfileResponse
        {
            HocSinh = ToDto(hs),
            TenLop = hs.LopHoc?.TenLop,
            KhoiLop = hs.LopHoc?.KhoiLop,
            NamHocLop = hs.LopHoc?.NamHoc,
            DiemTheoKy = diemTheoKy,
            HocPhis = phiDtos,
            ThongBaos = thongBaos
        });
    }

    [HttpGet("{maHS}/lich-su-hoc-tap")]
    [Authorize(Policy = AppPolicies.CanViewStudents)]
    public async Task<ActionResult<List<LichSuHocTapResponse>>> GetLichSuHocTap(
        [FromRoute] string maHS,
        [FromQuery] string? namHoc = null,
        [FromQuery] byte? hocKy = null,
        CancellationToken ct = default)
    {
        var userId = current.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        if (!await access.CanViewStudentAsync(userId, maHS, ct)) return Forbid();

        var diemQuery = db.DiemSos.AsNoTracking()
            .Include(x => x.ThanhPhans)
            .Where(x => x.MaHS == maHS);

        if (!string.IsNullOrWhiteSpace(namHoc)) diemQuery = diemQuery.Where(x => x.NamHoc == namHoc);
        if (hocKy.HasValue) diemQuery = diemQuery.Where(x => x.HocKy == hocKy.Value);

        var ds = await diemQuery.ToListAsync(ct);

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
                    HanhKiem = XepHanhKiem(tbc),
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
    public async Task<IActionResult> ExportExcel(
        [FromQuery] string? search = null,
        [FromQuery] string? maLop = null,
        [FromQuery] string? khoiLop = null,
        [FromQuery] string? namHoc = null,
        [FromQuery] string? trangThai = null,
        CancellationToken ct = default)
    {
        var userId = current.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var exportQuery = db.HocSinhs.AsNoTracking().Include(h => h.LopHoc).AsQueryable();
        if (User.IsInRole(RolePermissionSeeder.Parent))
        {
            var codes = await access.GetParentStudentCodesAsync(userId, ct);
            exportQuery = exportQuery.Where(h => codes.Contains(h.MaHS));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            exportQuery = exportQuery.Where(x =>
                x.HoTen.Contains(s) ||
                x.MaHS.Contains(s) ||
                (x.DiaChi != null && x.DiaChi.Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(maLop)) exportQuery = exportQuery.Where(x => x.MaLop == maLop);
        if (!string.IsNullOrWhiteSpace(khoiLop)) exportQuery = exportQuery.Where(x => x.LopHoc != null && x.LopHoc.KhoiLop == khoiLop);
        if (!string.IsNullOrWhiteSpace(namHoc)) exportQuery = exportQuery.Where(x => x.LopHoc != null && x.LopHoc.NamHoc == namHoc);
        if (!string.IsNullOrWhiteSpace(trangThai)) exportQuery = exportQuery.Where(x => x.TrangThai == trangThai);

        var list = await exportQuery.OrderBy(x => x.MaLop).ThenBy(x => x.HoTen).ToListAsync(ct);

        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Hoc sinh");
        ws.Cell(1, 1).Value = "MaHS";
        ws.Cell(1, 2).Value = "HoTen";
        ws.Cell(1, 3).Value = "NgaySinh";
        ws.Cell(1, 4).Value = "MaLop";
        ws.Cell(1, 5).Value = "TrangThai";
        ws.Cell(1, 6).Value = "Email PH";
        ws.Cell(1, 7).Value = "SDT PH";
        ws.Cell(1, 8).Value = "DiaChi";
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
            ws.Cell(r, 8).Value = x.DiaChi ?? "";
            r++;
        }

        await using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "hoc-sinh.xlsx");
    }

    [HttpPost("import/excel")]
    [Authorize(Policy = AppPolicies.CanEditStudents)]
    public async Task<ActionResult<HocSinhImportResultDto>> ImportExcel([FromForm] IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0) return BadRequest("File rỗng");

        var result = new HocSinhImportResultDto();
        await using var stream = file.OpenReadStream();
        using var wb = new XLWorkbook(stream);
        var ws = wb.Worksheet(1);
        var rows = ws.RangeUsed()?.RowsUsed().Skip(1) ?? Enumerable.Empty<IXLRangeRow>();
        foreach (var row in rows)
        {
            var maHs = row.Cell(1).GetString().Trim();
            var hoTen = row.Cell(2).GetString().Trim();
            var ngaySinh = TryParseExcelDate(row.Cell(3));
            var maLop = row.Cell(4).GetString().Trim();
            var trangThai = row.Cell(5).GetString().Trim();
            var emailPh = row.Cell(6).GetString().Trim();
            var sdtPh = row.Cell(7).GetString().Trim();
            var diaChi = row.Cell(8).GetString().Trim();

            if (string.IsNullOrWhiteSpace(hoTen) && string.IsNullOrWhiteSpace(maLop))
            {
                result.Skipped++;
                continue;
            }

            if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(maLop))
            {
                result.Skipped++;
                result.Warnings.Add($"Dòng {row.RowNumber()}: thiếu HoTen hoặc MaLop.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(maHs))
                maHs = await StudentCodeGenerator.NextAsync(db, DateTime.UtcNow.Year, ct);

            if (!EduCodeFormats.IsValidStudentCode(maHs) || !EduCodeFormats.IsValidClassCode(maLop))
            {
                result.Skipped++;
                result.Warnings.Add($"Dòng {row.RowNumber()}: MaHS hoặc MaLop không hợp lệ ({maHs}, {maLop}).");
                continue;
            }

            if (await db.HocSinhs.AnyAsync(x => x.MaHS == maHs, ct))
            {
                var exist = await db.HocSinhs.FirstAsync(x => x.MaHS == maHs, ct);
                exist.HoTen = hoTen;
                exist.MaLop = maLop;
                exist.NgaySinh = ngaySinh;
                if (!string.IsNullOrWhiteSpace(trangThai)) exist.TrangThai = trangThai;
                exist.Email_PhuHuynh = string.IsNullOrWhiteSpace(emailPh) ? exist.Email_PhuHuynh : emailPh;
                exist.SDT_PhuHuynh = string.IsNullOrWhiteSpace(sdtPh) ? exist.SDT_PhuHuynh : sdtPh;
                exist.DiaChi = string.IsNullOrWhiteSpace(diaChi) ? exist.DiaChi : diaChi;
            }
            else
            {
                db.HocSinhs.Add(new HocSinh
                {
                    MaHS = maHs,
                    HoTen = hoTen,
                    MaLop = maLop,
                    NgaySinh = ngaySinh,
                    DiaChi = string.IsNullOrWhiteSpace(diaChi) ? null : diaChi,
                    Email_PhuHuynh = string.IsNullOrWhiteSpace(emailPh) ? null : emailPh,
                    SDT_PhuHuynh = string.IsNullOrWhiteSpace(sdtPh) ? null : sdtPh,
                    TrangThai = string.IsNullOrWhiteSpace(trangThai) ? "Đang học" : trangThai
                });
            }

            result.Imported++;
        }

        await db.SaveChangesAsync(ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.CanEditStudents)]
    public async Task<ActionResult<HocSinhResponse>> Create([FromBody] HocSinhCreateRequest req, CancellationToken ct)
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

        var created = await db.HocSinhs
            .AsNoTracking()
            .Include(x => x.LopHoc)
            .Include(x => x.DiemSos).ThenInclude(d => d.ThanhPhans)
            .FirstAsync(x => x.MaHS == hs.MaHS, ct);
        return CreatedAtAction(nameof(GetById), new { maHS = created.MaHS }, ToDto(created));
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
