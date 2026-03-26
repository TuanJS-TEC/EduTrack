using System.ComponentModel.DataAnnotations;

namespace EduTrack.API.Models;

public sealed class HocSinh
{
    [Key]
    [MaxLength(20)]
    public string MaHS { get; set; } = "";

    [Required]
    [MaxLength(100)]
    public string HoTen { get; set; } = "";

    public DateTime? NgaySinh { get; set; }

    [MaxLength(255)]
    public string? DiaChi { get; set; }

    [Required]
    [MaxLength(20)]
    public string MaLop { get; set; } = "";

    [MaxLength(255)]
    public string? Email_PhuHuynh { get; set; }

    [MaxLength(20)]
    public string? SDT_PhuHuynh { get; set; }

    public LopHoc? LopHoc { get; set; }
    public ICollection<DiemSo> DiemSos { get; set; } = new List<DiemSo>();
    public ICollection<HocPhi> HocPhis { get; set; } = new List<HocPhi>();
    public ICollection<ThongBao> ThongBaos { get; set; } = new List<ThongBao>();
}

