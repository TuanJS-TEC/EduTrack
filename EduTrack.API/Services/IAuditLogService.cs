namespace EduTrack.API.Services;

public interface IAuditLogService
{
    Task LogAsync(string action, string entityType, string? entityKey, string? oldSnapshot, string? newSnapshot, CancellationToken ct = default);
    Task LogViolationAsync(
        string action,
        string entityType,
        string? entityKey,
        string violationCode,
        string severity,
        string? metadataJson = null,
        CancellationToken ct = default);
}
