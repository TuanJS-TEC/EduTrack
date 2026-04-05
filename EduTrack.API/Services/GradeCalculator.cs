namespace EduTrack.API.Services;

public static class GradeCalculator
{
    /// <summary>Làm tròn 1 chữ số thập phân (0.05 làm tròn lên — AwayFromZero).</summary>
    public static decimal RoundOneDecimal(decimal value) =>
        Math.Round(value, 1, MidpointRounding.AwayFromZero);

    /// <summary>
    /// TBM = (TB_miệng×1 + TB_15p×2 + GK×3 + CK×4) / 10.
    /// Thiếu GK hoặc CK → null. Không có điểm miệng/15p → coi TB tương ứng = 0.
    /// </summary>
    public static decimal? CalcTbm(IReadOnlyList<decimal> diemMieng, IReadOnlyList<decimal> diem15p, decimal? diemGk, decimal? diemCk)
    {
        if (diemGk is null || diemCk is null) return null;

        var tbM = diemMieng.Count > 0 ? diemMieng.Average() : 0m;
        var tb15 = diem15p.Count > 0 ? diem15p.Average() : 0m;

        var raw = (tbM * 1m + tb15 * 2m + diemGk.Value * 3m + diemCk.Value * 4m) / 10m;
        return RoundOneDecimal(raw);
    }

    /// <summary>Tương thích mã gọi cũ (cùng công thức TBM mới).</summary>
    public static decimal? CalcTbMon(decimal? diemMieng, decimal? diem15p, decimal? diemGk, decimal? diemCk) =>
        CalcTbm(diemMieng, diem15p, diemGk, diemCk);

    /// <summary>Legacy: một điểm miệng / một điểm 15p.</summary>
    public static decimal? CalcTbm(decimal? diemMieng, decimal? diem15p, decimal? diemGk, decimal? diemCk)
    {
        var m = new List<decimal>();
        var p = new List<decimal>();
        if (diemMieng.HasValue) m.Add(diemMieng.Value);
        if (diem15p.HasValue) p.Add(diem15p.Value);
        return CalcTbm(m, p, diemGk, diemCk);
    }

    public static bool IsLiet(decimal? diemCk) => diemCk is < 3.0m;

    public static bool PassedMon(decimal? tbm, decimal? diemCk) =>
        tbm is >= 5.0m && diemCk is >= 3.0m;

    /// <summary>Xếp loại theo môn (dựa trên TBM), có nhãn liệt khi CK &lt; 3.</summary>
    public static string? XepLoaiMon(decimal? tbm, decimal? diemCk)
    {
        if (tbm is null) return null;
        if (IsLiet(diemCk)) return "Liet";
        if (tbm >= 8.0m) return "Gioi";
        if (tbm >= 6.5m) return "Kha";
        if (tbm >= 5.0m) return "TrungBinh";
        if (tbm >= 3.5m) return "Yeu";
        return "Kem";
    }

    /// <summary>Alias tương thích DSS/dashboard cũ (chỉ theo TBM).</summary>
    public static string? XepLoai(decimal? tbMon) =>
        tbMon is null ? null : XepLoaiMon(tbMon, diemCk: null);

    /// <summary>Điểm CK tối thiểu để đạt TBM mục tiêu (TB_m, TB_15p, GK đã biết).</summary>
    public static decimal? CalcCanThietCuoiKy(decimal targetTbm, IReadOnlyList<decimal> diemMieng, IReadOnlyList<decimal> diem15p, decimal? diemGk)
    {
        if (diemGk is null) return null;
        var tbM = diemMieng.Count > 0 ? diemMieng.Average() : 0m;
        var tb15 = diem15p.Count > 0 ? diem15p.Average() : 0m;
        // target = (tbM + 2*tb15 + 3*GK + 4*CK)/10 => CK = (10*T - tbM - 2*tb15 - 3*GK)/4
        var ck = (10m * targetTbm - tbM - 2m * tb15 - 3m * diemGk.Value) / 4m;
        return RoundOneDecimal(ck);
    }

    public static decimal CalcCanThietCuoiKy(decimal targetTbm, decimal? diemMieng, decimal? diem15p, decimal? diemGk)
    {
        var m = new List<decimal>();
        var p = new List<decimal>();
        if (diemMieng.HasValue) m.Add(diemMieng.Value);
        if (diem15p.HasValue) p.Add(diem15p.Value);
        return CalcCanThietCuoiKy(targetTbm, m, p, diemGk) ?? 0m;
    }

    private static int RankToInt(string rank) => rank switch
    {
        "Gioi" => 5,
        "Kha" => 4,
        "TrungBinh" => 3,
        "Yeu" => 2,
        _ => 1
    };

    private static string IntToRank(int v) => v switch
    {
        >= 5 => "Gioi",
        4 => "Kha",
        3 => "TrungBinh",
        2 => "Yeu",
        _ => "Kem"
    };

    private static int Downgrade(int rank) => Math.Max(1, rank - 1);

    /// <summary>
    /// Học lực cả năm/kỳ theo TBC và điều kiện từng môn (spec 2.x).
    /// </summary>
    public static string? CalcHocLuc(IReadOnlyList<(decimal? tbm, decimal? ck)> mons)
    {
        var tbms = mons.Where(x => x.tbm.HasValue).Select(x => x.tbm!.Value).ToList();
        if (tbms.Count == 0) return null;

        var tbc = RoundOneDecimal(tbms.Average());
        var minTbm = tbms.Min();
        var anyLiet = mons.Any(m => IsLiet(m.ck));

        int rank;
        if (tbc >= 8.0m && minTbm >= 6.5m && !anyLiet) rank = RankToInt("Gioi");
        else if (tbc >= 6.5m && minTbm >= 5.0m && !anyLiet) rank = RankToInt("Kha");
        else if (tbc >= 5.0m && minTbm >= 3.5m && !anyLiet) rank = RankToInt("TrungBinh");
        else if (tbc >= 3.5m && minTbm >= 2.0m) rank = RankToInt("Yeu");
        else rank = RankToInt("Kem");

        // 2.2 / 2.4: có môn TBM &lt; 3.5 → hạ một bậc
        if (mons.Any(m => m.tbm is < 3.5m)) rank = Downgrade(rank);

        // 2.4: có môn liệt → tối đa Trung bình
        if (anyLiet) rank = Math.Min(rank, RankToInt("TrungBinh"));

        return IntToRank(rank);
    }
}
