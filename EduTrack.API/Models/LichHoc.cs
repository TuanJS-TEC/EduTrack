using System.ComponentModel.DataAnnotations;

namespace EduTrack.API.Models;

public sealed class LichHoc
{
    [Key]
    public int MaLich { get; set; }

    [Required]
    [MaxLength(20)]
    public string MaMon { get; set; } = "";

    [Required]
    [MaxLength(20)]
    public string MaLop { get; set; } = "";

    [MaxLength(20)]
    public string? MaGV { get; set; }

    public byte? Thu { get; set; }

    public int? TietBD { get; set; }

    public int? TietKT { get; set; }

    [MaxLength(50)]
    public string? Phong { get; set; }

    public MonHoc? MonHoc { get; set; }
    public LopHoc? LopHoc { get; set; }
    public GiaoVien? GiaoVien { get; set; }
}

