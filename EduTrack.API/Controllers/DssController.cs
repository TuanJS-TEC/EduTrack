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

    [HttpGet("can-thiep")]
    [Authorize(Policy = AppPolicies.CanViewDashboard)]
    public async Task<ActionResult<DssInterventionResponse>> CanThiep(
        [FromQuery] byte hocKy,
        [FromQuery] string namHoc = "2025-2026",
        [FromQuery] string? maLop = null,
        CancellationToken ct = default)
    {
        var hsQuery = db.HocSinhs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(maLop))
            hsQuery = hsQuery.Where(x => x.MaLop == maLop);

        var hsList = await hsQuery.Select(x => new { x.MaHS, x.HoTen, x.MaLop }).ToListAsync(ct);
        var maHsSet = hsList.Select(x => x.MaHS).ToHashSet();
        var dsRows = await db.DiemSos.AsNoTracking()
            .Include(x => x.ThanhPhans)
            .Include(x => x.MonHoc)
            .Where(x => x.NamHoc == namHoc && x.HocKy == hocKy && maHsSet.Contains(x.MaHS))
            .ToListAsync(ct);

        var byHs = dsRows.GroupBy(x => x.MaHS).ToDictionary(x => x.Key, x => x.ToList());
        var result = new List<DssInterventionItemDto>();
        foreach (var hs in hsList)
        {
            if (!byHs.TryGetValue(hs.MaHS, out var monRows) || monRows.Count == 0)
                continue;

            var tbList = monRows.Select(DiemSoScoreReader.RecalculateTbm).Where(x => x.HasValue).Select(x => x!.Value).ToList();
            var tbHocKy = tbList.Count == 0 ? (decimal?)null : GradeCalculator.RoundOneDecimal(tbList.Average());
            var monNguyCo = monRows
                .Select(m => new { m.MaMon, m.MonHoc, Tbm = DiemSoScoreReader.RecalculateTbm(m), m.DiemCuoiKy })
                .Where(x => !GradeCalculator.PassedMon(x.Tbm, x.DiemCuoiKy) || (x.Tbm.HasValue && x.Tbm.Value < 5.0m))
                .OrderBy(x => x.Tbm ?? 0m)
                .ToList();

            if (monNguyCo.Count == 0)
                continue;

            var mucRuiRo = (tbHocKy, monNguyCo.Count) switch
            {
                (null, _) => "Cao",
                (< 4.5m, _) => "Cao",
                (_, >= 3) => "Cao",
                (< 5.5m, _) => "TrungBinh",
                (_, 2) => "TrungBinh",
                _ => "Thap"
            };

            var monUuTien = monNguyCo
                .Take(3)
                .Select(x => $"{x.MaMon}-{x.MonHoc?.TenMon ?? x.MaMon}")
                .ToList();

            var khuyenNghi = new List<string>
            {
                "Tổ chức phụ đạo theo nhóm môn ưu tiên trong 2 tuần.",
                "Giao bài luyện tập có giám sát phụ huynh."
            };
            if (mucRuiRo == "Cao") khuyenNghi.Add("Họp GVCN + bộ môn + phụ huynh để lập cam kết can thiệp.");

            result.Add(new DssInterventionItemDto
            {
                MaHS = hs.MaHS,
                HoTen = hs.HoTen,
                MaLop = hs.MaLop,
                TbHocKy = tbHocKy,
                SoMonNguyCo = monNguyCo.Count,
                MucRuiRo = mucRuiRo,
                MonUuTien = monUuTien,
                KhuyenNghi = khuyenNghi
            });
        }

        return Ok(new DssInterventionResponse
        {
            NamHoc = namHoc,
            HocKy = hocKy,
            MaLop = maLop,
            DanhSachCanThiep = result
                .OrderByDescending(x => RiskWeight(x.MucRuiRo))
                .ThenByDescending(x => x.SoMonNguyCo)
                .ThenBy(x => x.HoTen)
                .ToList()
        });
    }

    [HttpPost("mo-phong")]
    [Authorize(Policy = AppPolicies.CanViewDashboard)]
    public async Task<ActionResult<DssMultiScenarioResponse>> MoPhongNhieuKichBan(
        [FromBody] DssMultiScenarioRequest req,
        CancellationToken ct = default)
    {
        if (req.Scenarios.Count == 0)
            return BadRequest(new { message = "Danh sách kịch bản rỗng." });

        var maHsSet = req.Scenarios.Select(x => x.MaHS).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToHashSet();
        var hsQuery = db.HocSinhs.AsNoTracking().Where(x => maHsSet.Contains(x.MaHS));
        if (!string.IsNullOrWhiteSpace(req.MaLop))
            hsQuery = hsQuery.Where(x => x.MaLop == req.MaLop);
        var hsRows = await hsQuery.Select(x => x.MaHS).ToListAsync(ct);
        var allowedSet = hsRows.ToHashSet();

        var dsRows = await db.DiemSos.AsNoTracking()
            .Include(x => x.ThanhPhans)
            .Where(x => x.NamHoc == req.NamHoc && x.HocKy == req.HocKy && allowedSet.Contains(x.MaHS))
            .ToListAsync(ct);

        var byHs = dsRows.GroupBy(x => x.MaHS).ToDictionary(x => x.Key, x => x.ToList());
        var output = new List<DssMultiScenarioStudentResultDto>();

        foreach (var scenario in req.Scenarios)
        {
            if (!byHs.TryGetValue(scenario.MaHS, out var monRows) || monRows.Count == 0) continue;

            var beforeTbList = monRows.Select(DiemSoScoreReader.RecalculateTbm).Where(x => x.HasValue).Select(x => x!.Value).ToList();
            var beforeTb = beforeTbList.Count == 0 ? (decimal?)null : GradeCalculator.RoundOneDecimal(beforeTbList.Average());
            var beforeXl = GradeCalculator.CalcHocLuc(monRows.Select(m => (DiemSoScoreReader.RecalculateTbm(m), m.DiemCuoiKy)).ToList());

            var adjustMap = scenario.DieuChinhMonHoc.ToDictionary(x => x.MaMon, x => x.DiemCuoiKyGiaDinh, StringComparer.OrdinalIgnoreCase);
            var afterPairs = new List<(decimal? tbm, decimal? ck)>();
            foreach (var m in monRows)
            {
                var (mieng, p15) = DiemSoScoreReader.GetComponentLists(m);
                var ck = adjustMap.TryGetValue(m.MaMon, out var newCk) ? newCk : m.DiemCuoiKy;
                var tbm = GradeCalculator.CalcTbm(mieng, p15, m.DiemGiuaKy, ck);
                afterPairs.Add((tbm, ck));
            }

            var afterTbList = afterPairs.Where(x => x.tbm.HasValue).Select(x => x.tbm!.Value).ToList();
            var afterTb = afterTbList.Count == 0 ? (decimal?)null : GradeCalculator.RoundOneDecimal(afterTbList.Average());
            var afterXl = GradeCalculator.CalcHocLuc(afterPairs);

            output.Add(new DssMultiScenarioStudentResultDto
            {
                MaHS = scenario.MaHS,
                TbTruoc = beforeTb,
                TbSau = afterTb,
                ChenhLech = (beforeTb, afterTb) switch
                {
                    (null, _) => null,
                    (_, null) => null,
                    _ => GradeCalculator.RoundOneDecimal(afterTb!.Value - beforeTb!.Value)
                },
                XepLoaiTruoc = beforeXl,
                XepLoaiSau = afterXl,
                MonTacDong = adjustMap.Keys.OrderBy(x => x).ToList()
            });
        }

        return Ok(new DssMultiScenarioResponse
        {
            NamHoc = req.NamHoc,
            HocKy = req.HocKy,
            MaLop = req.MaLop,
            KetQua = output.OrderByDescending(x => x.ChenhLech ?? decimal.MinValue).ThenBy(x => x.MaHS).ToList()
        });
    }

    private static int RiskWeight(string level) =>
        level.Equals("Cao", StringComparison.OrdinalIgnoreCase) ? 3 :
        level.Equals("TrungBinh", StringComparison.OrdinalIgnoreCase) ? 2 : 1;
}
