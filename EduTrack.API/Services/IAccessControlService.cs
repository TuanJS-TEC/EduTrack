namespace EduTrack.API.Services;

public interface IAccessControlService
{
    Task<bool> UserHasPermissionAsync(string userId, string permission, CancellationToken ct = default);

    Task<IReadOnlyList<string>> GetParentStudentCodesAsync(string userId, CancellationToken ct = default);

    Task<bool> CanViewStudentAsync(string? userId, string maHS, CancellationToken ct = default);

    Task<bool> CanEditStudentRecordAsync(string? userId, CancellationToken ct = default);

    Task<bool> CanEditScoreAsync(string? userId, string maHS, string maMon, string namHoc, byte hocKy, CancellationToken ct = default);

    Task<bool> CanViewFinanceAsync(string? userId, CancellationToken ct = default);

    Task<bool> CanManageFinanceAsync(string? userId, CancellationToken ct = default);
}
