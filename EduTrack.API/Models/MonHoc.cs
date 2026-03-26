using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduTrack.API.Models;

public sealed class MonHoc
{
    [Key]
    [MaxLength(20)]
    public string MaMon { get; set; } = "";

    [Required]
    [MaxLength(100)]
    public string TenMon { get; set; } = "";

    public int? SoTiet { get; set; }

    [Column(TypeName = "decimal(4,2)")]
    public decimal? HeSoThi { get; set; }

    [MaxLength(20)]
    public string? MaGV { get; set; }

    public GiaoVien? GiaoVien { get; set; }
    public ICollection<DiemSo> DiemSos { get; set; } = new List<DiemSo>();
    public ICollection<LichHoc> LichHocs { get; set; } = new List<LichHoc>();
}

