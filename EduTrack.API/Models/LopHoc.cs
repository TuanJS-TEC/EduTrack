using System.ComponentModel.DataAnnotations;

namespace EduTrack.API.Models;

public sealed class LopHoc
{
    [Key]
    [MaxLength(20)]
    public string MaLop { get; set; } = "";

    [Required]
    [MaxLength(100)]
    public string TenLop { get; set; } = "";

    public int? KhoiLop { get; set; }

    [MaxLength(20)]
    public string? NamHoc { get; set; }

    [MaxLength(20)]
    public string? MaGVChuNhiem { get; set; }

    public GiaoVien? GiaoVienChuNhiem { get; set; }
    public ICollection<HocSinh> HocSinhs { get; set; } = new List<HocSinh>();
    public ICollection<LichHoc> LichHocs { get; set; } = new List<LichHoc>();
}

