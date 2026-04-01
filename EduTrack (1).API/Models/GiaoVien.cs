using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduTrack.API.Models;

public sealed class GiaoVien
{
    [Key]
    [MaxLength(20)]
    public string MaGV { get; set; } = "";

    [Required]
    [MaxLength(100)]
    public string HoTen { get; set; } = "";

    [MaxLength(100)]
    public string? ChuyenMon { get; set; }

    [MaxLength(255)]
    public string? Email { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? LuongCoBan { get; set; }

    public ICollection<MonHoc> MonHocs { get; set; } = new List<MonHoc>();
    public ICollection<LichHoc> LichHocs { get; set; } = new List<LichHoc>();
    public ICollection<LopHoc> LopChuNhiems { get; set; } = new List<LopHoc>();
}

