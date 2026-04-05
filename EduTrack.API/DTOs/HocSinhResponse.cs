namespace EduTrack.API.DTOs;

public sealed class HocSinhResponse
{
    public string MaHS { get; set; } = "";
    public string HoTen { get; set; } = "";
    public DateTime? NgaySinh { get; set; }
    public string? DiaChi { get; set; }
    public string MaLop { get; set; } = "";

    /// <summary>Có giá trị khi truy vấn Include LopHoc.</summary>
    public string? TenLop { get; set; }

    public string? KhoiLop { get; set; }
    public string? NamHocLop { get; set; }

    public string? Email_PhuHuynh { get; set; }
    public string? SDT_PhuHuynh { get; set; }
    public string TrangThai { get; set; } = "Đang học";

    // Computed từ DiemSos — không lưu DB
    public decimal? DiemTB { get; set; }
    public string? HanhKiem { get; set; }
}
