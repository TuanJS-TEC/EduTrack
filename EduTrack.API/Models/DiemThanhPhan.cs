using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduTrack.API.Models;

/// <summary>Điểm thành phần: miệng / 15 phút (nhiều bản ghi).</summary>
public sealed class DiemThanhPhan
{
    public int Id { get; set; }

    public int MaDiem { get; set; }

    /// <summary>1 = miệng, 2 = 15 phút</summary>
    public byte Loai { get; set; }

    [Column(TypeName = "decimal(4,2)")]
    public decimal Diem { get; set; }

    public int ThuTu { get; set; }

    public DiemSo? DiemSo { get; set; }
}
