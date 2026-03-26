namespace EduTrack.API.DTOs;

public sealed class CanhBaoRoiMonResponse
{
    public string MaHS { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string MaLop { get; set; } = "";
    public string MaMon { get; set; } = "";
    public string TenMon { get; set; } = "";
    public byte HocKy { get; set; }

    public decimal? DiemMieng { get; set; }
    public decimal? Diem15p { get; set; }
    public decimal? DiemGiuaKy { get; set; }
    public decimal? DiemCuoiKy { get; set; }
    public decimal? DiemTBMon { get; set; }

    public decimal CkCanThiet { get; set; }
    public string MucDo { get; set; } = ""; // Do / Vang / Xanh
}

