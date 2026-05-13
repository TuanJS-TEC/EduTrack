namespace EduTrack.API.DTOs;

public sealed class BangDiemItemResponse
{
    public string MaHS { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string MaLop { get; set; } = "";
    public string MaMon { get; set; } = "";
    public string TenMon { get; set; } = "";
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

    public string? XepLoai { get; set; }
    public bool QuaMon { get; set; }
    public bool Liet { get; set; }
}

