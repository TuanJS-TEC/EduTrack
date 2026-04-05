namespace EduTrack.API.DTOs;

public sealed class HocSinhCreateRequest
{
    /// <summary>Để trống → sinh mã HS-YYYY-xxxxx theo năm nhập học.</summary>
    public string? MaHS { get; set; }

    public int NamNhapHoc { get; set; } = DateTime.UtcNow.Year;
    public string HoTen { get; set; } = "";
    public DateTime? NgaySinh { get; set; }
    public string? DiaChi { get; set; }
    public string MaLop { get; set; } = "";
    public string? Email_PhuHuynh { get; set; }
    public string? SDT_PhuHuynh { get; set; }
    public string TrangThai { get; set; } = "Đang học";
}
