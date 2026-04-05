namespace EduTrack.API.DTOs;

public sealed class HocSinhUpdateRequest
{
    public string HoTen { get; set; } = "";
    public DateTime? NgaySinh { get; set; }
    public string? DiaChi { get; set; }
    public string MaLop { get; set; } = "";
    public string? Email_PhuHuynh { get; set; }
    public string? SDT_PhuHuynh { get; set; }
    public string? TrangThai { get; set; }
}
