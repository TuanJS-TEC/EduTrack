using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduTrack.API.Models;

public sealed class HocPhi
{
    [Key]
    public int MaHocPhi { get; set; }

    [Required]
    [MaxLength(20)]
    public string MaHS { get; set; } = "";

    public byte HocKy { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SoTien { get; set; }

    public DateTime? NgayDong { get; set; }

    [MaxLength(20)]
    public string? TrangThai { get; set; }

    public HocSinh? HocSinh { get; set; }
}

