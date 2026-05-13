namespace EduTrack.API.DTOs;

public sealed class AuditLogEntryDto
{
    public long Id { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = "";
    public string EntityType { get; set; } = "";
    public string? EntityKey { get; set; }
    public string? OldSnapshot { get; set; }
    public string? NewSnapshot { get; set; }
    public string? ViolationCode { get; set; }
    public string? Severity { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime AtUtc { get; set; }
}
