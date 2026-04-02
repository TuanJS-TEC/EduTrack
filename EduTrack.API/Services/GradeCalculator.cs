namespace EduTrack.API.Services;

public static class GradeCalculator
{
    // Theo flowchart: TB = M*0.1 + 15p*0.2 + GK*0.3 + CK*0.4
    public static decimal? CalcTbMon(decimal? diemMieng, decimal? diem15p, decimal? diemGiuaKy, decimal? diemCuoiKy)
    {
        if (diemMieng is null && diem15p is null && diemGiuaKy is null && diemCuoiKy is null) return null;

        var m = diemMieng ?? 0m;
        var p15 = diem15p ?? 0m;
        var gk = diemGiuaKy ?? 0m;
        var ck = diemCuoiKy ?? 0m;

        var tb = m * 0.1m + p15 * 0.2m + gk * 0.3m + ck * 0.4m;
        return Math.Round(tb, 2, MidpointRounding.AwayFromZero);
    }

    public static string? XepLoai(decimal? tb)
    {
        if (tb is null) return null;
        if (tb >= 8.0m) return "Gioi";
        if (tb >= 6.5m) return "Kha";
        if (tb >= 5.0m) return "TrungBinh";
        if (tb >= 3.5m) return "Yeu";
        return "Kem";
    }

    // What-if: cần điểm CK tối thiểu để đạt mục tiêu T (mặc định 5.0)
    public static decimal CalcCanThietCuoiKy(decimal targetTb, decimal? diemMieng, decimal? diem15p, decimal? diemGiuaKy)
    {
        var m = diemMieng ?? 0m;
        var p15 = diem15p ?? 0m;
        var gk = diemGiuaKy ?? 0m;
        // CK_can = (T - M*0.1 - 15p*0.2 - GK*0.3) / 0.4
        var ckCan = (targetTb - m * 0.1m - p15 * 0.2m - gk * 0.3m) / 0.4m;
        return Math.Round(ckCan, 2, MidpointRounding.AwayFromZero);
    }
}

