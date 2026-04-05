namespace EduTrack.API.DTOs;

public sealed class HocPhiResponseDto
{
    public int MaHocPhi { get; set; }
    public string MaHS { get; set; } = "";
    public byte HocKy { get; set; }
    public decimal SoTien { get; set; }
    public DateTime? NgayDong { get; set; }
    public string? TrangThai { get; set; }
}
