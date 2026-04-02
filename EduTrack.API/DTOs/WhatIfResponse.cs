namespace EduTrack.API.DTOs;

public sealed class WhatIfResponse
{
    public string MaHS { get; set; } = "";
    public string MaMon { get; set; } = "";
    public byte HocKy { get; set; }

    public decimal? DiemMieng { get; set; }
    public decimal? Diem15p { get; set; }
    public decimal? DiemGiuaKy { get; set; }
    public decimal? DiemCuoiKyHienTai { get; set; }

    public decimal DiemCuoiKyGiaDinh { get; set; }
    public decimal? TbGiaDinh { get; set; }
    public string? XepLoaiGiaDinh { get; set; }

    public decimal TargetTb { get; set; }
    public decimal CkCanThietDeDatTarget { get; set; }
}

