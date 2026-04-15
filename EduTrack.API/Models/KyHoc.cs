using System.ComponentModel.DataAnnotations;

namespace EduTrack.API.Models;

/// <summary>Trạng thái khóa nhập điểm theo năm học + học kỳ.</summary>
public sealed class KyHoc
{
    [MaxLength(12)]
    public string NamHoc { get; set; } = "";

    public byte HocKy { get; set; }

    public bool Locked { get; set; }

    [MaxLength(20)]
    public string TrangThai { get; set; } = "Mo";

    public DateTime? SubmittedAtUtc { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public DateTime? ReopenedAtUtc { get; set; }

    public ICollection<KyHocWorkflowLog> WorkflowLogs { get; set; } = new List<KyHocWorkflowLog>();
}
