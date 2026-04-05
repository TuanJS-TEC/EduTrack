using EduTrack.API.Models;

namespace EduTrack.API.Services;

public static class DiemSoScoreReader
{
    public static (List<decimal> Mieng, List<decimal> Fifteen) GetComponentLists(DiemSo d)
    {
        var mieng = d.ThanhPhans.Where(t => t.Loai == 1).OrderBy(t => t.ThuTu).Select(t => t.Diem).ToList();
        var fifteen = d.ThanhPhans.Where(t => t.Loai == 2).OrderBy(t => t.ThuTu).Select(t => t.Diem).ToList();

        if (mieng.Count == 0 && d.DiemMieng.HasValue) mieng.Add(d.DiemMieng.Value);
        if (fifteen.Count == 0 && d.Diem15p.HasValue) fifteen.Add(d.Diem15p.Value);

        return (mieng, fifteen);
    }

    public static decimal? RecalculateTbm(DiemSo d) =>
        GradeCalculator.CalcTbm(GetComponentLists(d).Mieng, GetComponentLists(d).Fifteen, d.DiemGiuaKy, d.DiemCuoiKy);
}
