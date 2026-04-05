using System.ComponentModel.DataAnnotations;

namespace EduTrack.API.Models;

/// <summary>Trạng thái khóa nhập điểm theo năm học + học kỳ.</summary>
public sealed class KyHoc
{
    [MaxLength(12)]
    public string NamHoc { get; set; } = "";

    public byte HocKy { get; set; }

    public bool Locked { get; set; }
}
