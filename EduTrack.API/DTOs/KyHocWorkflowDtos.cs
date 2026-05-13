namespace EduTrack.API.DTOs;

public sealed class KyHocWorkflowActionRequest
{
    public string NamHoc { get; set; } = "";
    public byte HocKy { get; set; }
    public string? BienBan { get; set; }
}

public sealed class KyHocWorkflowLogDto
{
    public long Id { get; set; }
    public string NamHoc { get; set; } = "";
    public byte HocKy { get; set; }
    public string FromStatus { get; set; } = "";
    public string ToStatus { get; set; } = "";
    public string? PerformedByUserId { get; set; }
    public string? PerformedByUserName { get; set; }
    public string? BienBan { get; set; }
    public DateTime AtUtc { get; set; }
}
