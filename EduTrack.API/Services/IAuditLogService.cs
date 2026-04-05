namespace EduTrack.API.Services;

public interface IAuditLogService
{
    Task LogAsync(string action, string entityType, string? entityKey, string? oldSnapshot, string? newSnapshot, CancellationToken ct = default);
}
