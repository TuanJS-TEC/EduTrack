using EduTrack.API.Models;

namespace EduTrack.API.Services;

/// <summary>Trạng thái nhập điểm (null được phép; TBM chỉ có khi đủ GK+CK).</summary>
public static class DiemNhapTrangThai
{
    public const string ChuaCoDiem = "ChuaCoDiem";
    public const string ChuaDuDiem = "ChuaDuDiem";
    public const string DuDiem = "DuDiem";

    public static string Compute(DiemSo? ds)
    {
        if (ds is null) return ChuaCoDiem;

        var hasThanhPhan = ds.ThanhPhans.Count > 0;
        var hasPartial = ds.DiemGiuaKy.HasValue || ds.DiemCuoiKy.HasValue
                         || ds.DiemMieng.HasValue || ds.Diem15p.HasValue
                         || hasThanhPhan;

        if (!hasPartial) return ChuaCoDiem;

        var tbm = DiemSoScoreReader.RecalculateTbm(ds);
        return tbm is null ? ChuaDuDiem : DuDiem;
    }
}
