using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduTrack.API.Models;

public sealed class DiemSo
{
    [Key]
    public int MaDiem { get; set; }

    [Required]
    [MaxLength(20)]
    public string MaHS { get; set; } = "";

    [Required]
    [MaxLength(20)]
    public string MaMon { get; set; } = "";

    public byte HocKy { get; set; }

    [Column(TypeName = "decimal(4,2)")]
    public decimal? DiemMieng { get; set; }

    [Column(TypeName = "decimal(4,2)")]
    public decimal? Diem15p { get; set; }

    [Column(TypeName = "decimal(4,2)")]
    public decimal? DiemGiuaKy { get; set; }

    [Column(TypeName = "decimal(4,2)")]
    public decimal? DiemCuoiKy { get; set; }

    [Column(TypeName = "decimal(4,2)")]
    public decimal? DiemTBMon { get; set; }

    public HocSinh? HocSinh { get; set; }
    public MonHoc? MonHoc { get; set; }
}

