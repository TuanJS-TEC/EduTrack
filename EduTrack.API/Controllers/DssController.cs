using EduTrack.API.Authorization;
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
    [HttpPost("what-if")]
    [Authorize(Policy = AppPolicies.CanEditScores)]
    public async Task<ActionResult<WhatIfResponse>> WhatIf([FromBody] WhatIfRequest req, CancellationToken ct)
    {
        var ds = await db.DiemSos.AsNoTracking()
            .Include(x => x.ThanhPhans)
            .FirstOrDefaultAsync(x => x.MaHS == req.MaHS && x.MaMon == req.MaMon && x.HocKy == req.HocKy && x.NamHoc == req.NamHoc, ct);

        var (mList, pList) = ds is null ? ([], []) : DiemSoScoreReader.GetComponentLists(ds);
        var diemGiuaKy = ds?.DiemGiuaKy;

        var tbGiaDinh = GradeCalculator.CalcTbm(mList, pList, diemGiuaKy, req.DiemCuoiKyGiaDinh);
        var ckCan = GradeCalculator.CalcCanThietCuoiKy(req.TargetTb, mList, pList, diemGiuaKy);

        return Ok(new WhatIfResponse
        {
            MaHS = req.MaHS,
            MaMon = req.MaMon,
            HocKy = req.HocKy,
            DiemMieng = ds?.DiemMieng,
            Diem15p = ds?.Diem15p,
            DiemGiuaKy = diemGiuaKy,
            DiemCuoiKyHienTai = ds?.DiemCuoiKy,
            DiemCuoiKyGiaDinh = req.DiemCuoiKyGiaDinh,
            TbGiaDinh = tbGiaDinh,
            XepLoaiGiaDinh = GradeCalculator.XepLoaiMon(tbGiaDinh, req.DiemCuoiKyGiaDinh),
            TargetTb = req.TargetTb,
            CkCanThietDeDatTarget = ckCan ?? 0m
        });
    }

    [HttpGet("canh-bao-roi-mon")]
    [Authorize(Policy = AppPolicies.CanViewScores)]
    public async Task<ActionResult<List<CanhBaoRoiMonResponse>>> CanhBaoRoiMon(
        [FromQuery] byte hocKy,
        [FromQuery] string namHoc = "2025-2026",
        [FromQuery] string? maLop = null,
        [FromQuery] decimal targetTb = 5.0m,
        CancellationToken ct = default)
    {
        var q = db.DiemSos.AsNoTracking()
            .Include(x => x.ThanhPhans)
            .Include(x => x.HocSinh)
            .Include(x => x.MonHoc)
            .Where(x => x.HocKy == hocKy && x.NamHoc == namHoc);

        if (!string.IsNullOrWhiteSpace(maLop))
            q = q.Where(x => x.HocSinh != null && x.HocSinh.MaLop == maLop);

        var rows = await q.ToListAsync(ct);

        var result = rows
            .Select(r =>
            {
                var (mList, pList) = DiemSoScoreReader.GetComponentLists(r);
                var tbm = DiemSoScoreReader.RecalculateTbm(r);
                var ckCan = GradeCalculator.CalcCanThietCuoiKy(targetTb, mList, pList, r.DiemGiuaKy) ?? 0m;
                var passed = GradeCalculator.PassedMon(tbm, r.DiemCuoiKy);
                var mucDo = !passed && (ckCan > 7.0m || (tbm is not null && tbm < 5.0m)) ? "Do"
                    : ckCan > 5.0m ? "Vang"
                    : "Xanh";

                return new CanhBaoRoiMonResponse
                {
                    MaHS = r.MaHS,
                    HoTen = r.HocSinh?.HoTen ?? "",
                    MaLop = r.HocSinh?.MaLop ?? "",
                    MaMon = r.MaMon,
                    TenMon = r.MonHoc?.TenMon ?? "",
                    HocKy = r.HocKy,
                    DiemMieng = r.DiemMieng,
                    Diem15p = r.Diem15p,
                    DiemGiuaKy = r.DiemGiuaKy,
                    DiemCuoiKy = r.DiemCuoiKy,
                    DiemTBMon = tbm,
                    CkCanThiet = ckCan,
                    MucDo = mucDo
                };
            })
            .Where(x => !GradeCalculator.PassedMon(x.DiemTBMon, x.DiemCuoiKy) || x.CkCanThiet > 7.0m)
            .OrderBy(x => x.MaLop)
            .ThenBy(x => x.HoTen)
            .ToList();

        return Ok(result);
    }

    [HttpGet("dashboard-hoc-luc")]
    [Authorize(Policy = AppPolicies.CanViewDashboard)]
    public async Task<ActionResult<DashboardHocLucResponse>> DashboardHocLuc(
        [FromQuery] byte hocKy,
        [FromQuery] string namHoc = "2025-2026",
        CancellationToken ct = default)
    {
        var hsQ = db.HocSinhs.AsNoTracking().Include(x => x.LopHoc).AsQueryable();
        hsQ = hsQ.Where(x => x.LopHoc != null && x.LopHoc.NamHoc == namHoc);

        var hs = await hsQ.Select(x => new { x.MaHS, x.MaLop, TenLop = x.LopHoc!.TenLop }).ToListAsync(ct);

        var ds = await db.DiemSos.AsNoTracking()
            .Include(x => x.ThanhPhans)
            .Where(x => x.HocKy == hocKy && x.NamHoc == namHoc)
            .ToListAsync(ct);

        var byHs = ds.GroupBy(x => x.MaHS).ToDictionary(g => g.Key, g => g.ToList());

        var gioi = 0;
        var kha = 0;
        var tb = 0;
        var yeu = 0;
        var kem = 0;

        foreach (var h in hs)
        {
            if (!byHs.TryGetValue(h.MaHS, out var mons) || mons.Count == 0) continue;

            var pairs = mons.Select(m => (DiemSoScoreReader.RecalculateTbm(m), m.DiemCuoiKy)).ToList();
            var xl = GradeCalculator.CalcHocLuc(pairs.Select(p => (p.Item1, p.Item2)).ToList());
            if (xl is null) continue;
            switch (xl)
            {
                case "Gioi": gioi++; break;
                case "Kha": kha++; break;
                case "TrungBinh": tb++; break;
                case "Yeu": yeu++; break;
                default: kem++; break;
            }
        }

        var tbByHs = new Dictionary<string, decimal>();
        foreach (var kv in byHs)
        {
            var vals = kv.Value.Select(m => DiemSoScoreReader.RecalculateTbm(m)).Where(x => x.HasValue).Select(x => x!.Value).ToList();
            if (vals.Count > 0) tbByHs[kv.Key] = GradeCalculator.RoundOneDecimal(vals.Average());
        }

        var theoLop = hs
            .GroupBy(x => new { x.MaLop, x.TenLop })
            .Select(g =>
            {
                var tbList = g.Select(h => tbByHs.TryGetValue(h.MaHS, out var v) ? (decimal?)v : null)
                    .Where(v => v.HasValue).Select(v => v!.Value).ToList();

                var tbChung = tbList.Count == 0 ? (decimal?)null : GradeCalculator.RoundOneDecimal(tbList.Average());
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
