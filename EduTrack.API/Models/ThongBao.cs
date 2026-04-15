using System.ComponentModel.DataAnnotations;

namespace EduTrack.API.Models;

public sealed class ThongBao
{
    [Key]
    public int MaTB { get; set; }

    [MaxLength(200)]
    public string? TieuDe { get; set; }

    public string? NoiDung { get; set; }

    public DateTime NgayGui { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? LoaiTB { get; set; }

    public bool DaDoc { get; set; }

    public DateTime? NgayDoc { get; set; }

    [MaxLength(20)]
    public string? MaHS { get; set; }

    public HocSinh? HocSinh { get; set; }
}

