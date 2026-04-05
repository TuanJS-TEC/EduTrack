using System.ComponentModel.DataAnnotations;

namespace EduTrack.API.Models;

public sealed class AuditLogEntry
{
    public long Id { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }

    [MaxLength(256)]
    public string? UserName { get; set; }

    [Required]
    [MaxLength(64)]
    public string Action { get; set; } = "";

    [Required]
    [MaxLength(64)]
    public string EntityType { get; set; } = "";

    [MaxLength(256)]
    public string? EntityKey { get; set; }

    public string? OldSnapshot { get; set; }
    public string? NewSnapshot { get; set; }

    public DateTime AtUtc { get; set; }
}
