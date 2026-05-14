namespace EduTrack.API.DTOs;

/// <summary>Bản ghi TKB phẳng cho lưới (tránh vòng lặp JSON khi Include navigation).</summary>
public sealed class LichHocListItemDto
{
    public int MaLich { get; set; }
    public string MaMon { get; set; } = "";
    public string MaLop { get; set; } = "";
    public string? MaGV { get; set; }
    public byte? Thu { get; set; }
    public int? TietBD { get; set; }
    public int? TietKT { get; set; }
    public string? Phong { get; set; }
    public string? TenMon { get; set; }
    public string? TenLop { get; set; }
    public string? TenGV { get; set; }
}
