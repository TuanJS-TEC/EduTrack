namespace EduTrack.API.DTOs;

public sealed class WhatIfRequest
{
    public string MaHS { get; set; } = "";
    public string MaMon { get; set; } = "";
    public byte HocKy { get; set; }
    public decimal DiemCuoiKyGiaDinh { get; set; }
    public decimal TargetTb { get; set; } = 5.0m;
}

