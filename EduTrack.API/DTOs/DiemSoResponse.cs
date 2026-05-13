namespace EduTrack.API.DTOs;

public sealed class DiemSoResponse
{
    public int MaDiem { get; set; }
    public string MaHS { get; set; } = "";
    public string MaMon { get; set; } = "";
    public string NamHoc { get; set; } = "";
    public byte HocKy { get; set; }

    public List<decimal> DiemMiengs { get; set; } = [];
    public List<decimal> Diem15ps { get; set; } = [];

    public decimal? DiemMieng { get; set; }
    public decimal? Diem15p { get; set; }
    public decimal? DiemGiuaKy { get; set; }
    public decimal? DiemCuoiKy { get; set; }
    public decimal? DiemTBMon { get; set; }

    /// <summary>ChuaCoDiem | ChuaDuDiem | DuDiem</summary>
    public string TrangThaiNhapDiem { get; set; } = "";
}
