namespace EduTrack.API.DTOs;

public sealed class DiemSoUpsertRequest
{
    public string MaHS { get; set; } = "";
    public string MaMon { get; set; } = "";
    public byte HocKy { get; set; }

    public decimal? DiemMieng { get; set; }
    public decimal? Diem15p { get; set; }
    public decimal? DiemGiuaKy { get; set; }
    public decimal? DiemCuoiKy { get; set; }
}

