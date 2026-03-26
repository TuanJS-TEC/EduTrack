using EduTrack.API.Data;
using EduTrack.API.DTOs;
using EduTrack.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Controllers;

[ApiController]
[Route("api/dss")]
[Authorize]
public sealed class DssController(EduTrackDbContext db) : ControllerBase
{
    // What-If: giả định điểm CK đạt X thì TB & xếp loại bao nhiêu?
    [HttpPost("what-if")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<WhatIfResponse>> WhatIf([FromBody] WhatIfRequest req)
    {
        var ds = await db.DiemSos.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaHS == req.MaHS && x.MaMon == req.MaMon && x.HocKy == req.HocKy);

        var diemMieng = ds?.DiemMieng;
        var diem15p = ds?.Diem15p;
        var diemGiuaKy = ds?.DiemGiuaKy;
        var diemCuoiKy = ds?.DiemCuoiKy;

        var tbGiaDinh = GradeCalculator.CalcTbMon(diemMieng, diem15p, diemGiuaKy, req.DiemCuoiKyGiaDinh);
        var ckCan = GradeCalculator.CalcCanThietCuoiKy(req.TargetTb, diemMieng, diem15p, diemGiuaKy);

        return Ok(new WhatIfResponse
        {
            MaHS = req.MaHS,
            MaMon = req.MaMon,
            HocKy = req.HocKy,
            DiemMieng = diemMieng,
            Diem15p = diem15p,
            DiemGiuaKy = diemGiuaKy,
            DiemCuoiKyHienTai = diemCuoiKy,
            DiemCuoiKyGiaDinh = req.DiemCuoiKyGiaDinh,
            TbGiaDinh = tbGiaDinh,
            XepLoaiGiaDinh = GradeCalculator.XepLoai(tbGiaDinh),
            TargetTb = req.TargetTb,
            CkCanThietDeDatTarget = ckCan
        });
    }

    // Cảnh báo: HS có nguy cơ rớt môn (TB < 5 hoặc CK cần thiết > 7.0 => nguy cơ cao)
    [HttpGet("canh-bao-roi-mon")]
    public async Task<ActionResult<List<CanhBaoRoiMonResponse>>> CanhBaoRoiMon([FromQuery] byte hocKy, [FromQuery] string? maLop, [FromQuery] decimal targetTb = 5.0m)
    {
        var q = db.DiemSos.AsNoTracking().Where(x => x.HocKy == hocKy);
        if (!string.IsNullOrWhiteSpace(maLop))
            q = q.Where(x => x.HocSinh != null && x.HocSinh.MaLop == maLop);

        var rows = await q
            .Select(x => new
            {
                x.MaHS,
                HoTen = x.HocSinh!.HoTen,
                MaLop = x.HocSinh!.MaLop,
                x.MaMon,
                TenMon = x.MonHoc!.TenMon,
                x.HocKy,
                x.DiemMieng,
                x.Diem15p,
                x.DiemGiuaKy,
                x.DiemCuoiKy,
                x.DiemTBMon
            })
            .ToListAsync();

        var result = rows
            .Select(r =>
            {
                var ckCan = GradeCalculator.CalcCanThietCuoiKy(targetTb, r.DiemMieng, r.Diem15p, r.DiemGiuaKy);
                var mucDo = ckCan > 7.0m || (r.DiemTBMon is not null && r.DiemTBMon < 5.0m) ? "Do"
                    : ckCan > 5.0m ? "Vang"
                    : "Xanh";

                return new CanhBaoRoiMonResponse
                {
                    MaHS = r.MaHS,
                    HoTen = r.HoTen,
                    MaLop = r.MaLop,
                    MaMon = r.MaMon,
                    TenMon = r.TenMon,
                    HocKy = r.HocKy,
                    DiemMieng = r.DiemMieng,
                    Diem15p = r.Diem15p,
                    DiemGiuaKy = r.DiemGiuaKy,
                    DiemCuoiKy = r.DiemCuoiKy,
                    DiemTBMon = r.DiemTBMon,
                    CkCanThiet = ckCan,
                    MucDo = mucDo
                };
            })
            .Where(x => x.DiemTBMon is null || x.DiemTBMon < targetTb || x.CkCanThiet > 7.0m)
            .OrderBy(x => x.MaLop)
            .ThenBy(x => x.HoTen)
            .ToList();

        return Ok(result);
    }

    // Dashboard thống kê học lực toàn trường (tính theo TB môn đã có)
    [HttpGet("dashboard-hoc-luc")]
    public async Task<ActionResult<DashboardHocLucResponse>> DashboardHocLuc([FromQuery] byte hocKy, [FromQuery] string? namHoc)
    {
        var hsQ = db.HocSinhs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(namHoc))
            hsQ = hsQ.Where(x => x.LopHoc != null && x.LopHoc.NamHoc == namHoc);

        var hs = await hsQ.Select(x => new { x.MaHS, x.MaLop, TenLop = x.LopHoc!.TenLop }).ToListAsync();
        var ds = await db.DiemSos.AsNoTracking().Where(x => x.HocKy == hocKy && x.DiemTBMon != null)
            .Select(x => new { x.MaHS, Tb = x.DiemTBMon!.Value })
            .ToListAsync();

        // TB chung 1 HS = trung bình các TB môn
        var tbByHs = ds.GroupBy(x => x.MaHS).ToDictionary(g => g.Key, g => g.Average(v => v.Tb));

        var counts = new { Gioi = 0, Kha = 0, TrungBinh = 0, Yeu = 0, Kem = 0 };
        var gioi = 0; var kha = 0; var tb = 0; var yeu = 0; var kem = 0;

        foreach (var h in hs)
        {
            if (!tbByHs.TryGetValue(h.MaHS, out var tbChung)) continue;
            var xl = GradeCalculator.XepLoai((decimal)tbChung);
            switch (xl)
            {
                case "Gioi": gioi++; break;
                case "Kha": kha++; break;
                case "TrungBinh": tb++; break;
                case "Yeu": yeu++; break;
                case "Kem": kem++; break;
            }
        }

        var theoLop = hs
            .GroupBy(x => new { x.MaLop, x.TenLop })
            .Select(g =>
            {
                var tbList = g.Select(h => tbByHs.TryGetValue(h.MaHS, out var v) ? (decimal?)v : null)
                    .Where(v => v.HasValue).Select(v => v!.Value).ToList();

                var tbChung = tbList.Count == 0 ? (decimal?)null : Math.Round(tbList.Average(), 2, MidpointRounding.AwayFromZero);
                return new DashboardLopSummary
                {
                    MaLop = g.Key.MaLop,
                    TenLop = g.Key.TenLop,
                    SiSo = g.Count(),
                    TbChung = tbChung
                };
            })
            .OrderBy(x => x.TenLop)
            .ToList();

        return Ok(new DashboardHocLucResponse
        {
            TongHocSinh = hs.Count,
            Gioi = gioi,
            Kha = kha,
            TrungBinh = tb,
            Yeu = yeu,
            Kem = kem,
            TheoLop = theoLop
        });
    }
}

