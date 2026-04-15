using System.ComponentModel.DataAnnotations;

namespace EduTrack.API.Models;

public sealed class KyHocWorkflowLog
{
    public long Id { get; set; }

    [MaxLength(12)]
    public string NamHoc { get; set; } = "";

    public byte HocKy { get; set; }

    [MaxLength(20)]
    public string FromStatus { get; set; } = "";

    [MaxLength(20)]
    public string ToStatus { get; set; } = "";

    [MaxLength(450)]
    public string? PerformedByUserId { get; set; }

    [MaxLength(256)]
    public string? PerformedByUserName { get; set; }

    public string? BienBan { get; set; }

    public DateTime AtUtc { get; set; }

    public KyHoc? KyHoc { get; set; }
}
