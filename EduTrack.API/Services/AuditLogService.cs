using EduTrack.API.Data;
using EduTrack.API.Models;

namespace EduTrack.API.Services;

public sealed class AuditLogService(EduTrackDbContext db, ICurrentUserService current) : IAuditLogService
{
    public async Task LogAsync(string action, string entityType, string? entityKey, string? oldSnapshot, string? newSnapshot, CancellationToken ct = default)
    {
        var userId = current.UserId;
        var userName = current.UserName;

        db.AuditLogEntries.Add(new AuditLogEntry
        {
            UserId = userId,
            UserName = userName,
            Action = action,
            EntityType = entityType,
            EntityKey = entityKey,
            OldSnapshot = oldSnapshot,
            NewSnapshot = newSnapshot,
            AtUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);
    }
}
