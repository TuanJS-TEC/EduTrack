using EduTrack.API.Authorization;
using EduTrack.API.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EduTrack.API.Models;

namespace EduTrack.API.Services;

public sealed class AccessControlService(
    EduTrackDbContext db,
    UserManager<ApplicationUser> userManager) : IAccessControlService
{
    public async Task<bool> UserHasPermissionAsync(string userId, string permission, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return false;
        var roles = await userManager.GetRolesAsync(user);
        return RolePermissionSeeder.PermissionsForRoles(roles).Contains(permission);
    }

    public async Task<IReadOnlyList<string>> GetParentStudentCodesAsync(string userId, CancellationToken ct = default) =>
        await db.ParentStudentLinks.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.MaHS)
            .ToListAsync(ct);

    public async Task<bool> CanViewStudentAsync(string? userId, string maHS, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(userId)) return false;
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return false;
        var roles = await userManager.GetRolesAsync(user);
        if (roles.Contains(RolePermissionSeeder.Admin) || roles.Contains(RolePermissionSeeder.Bgh) ||
            roles.Contains(RolePermissionSeeder.Teacher) || roles.Contains(RolePermissionSeeder.Accountant))
            return true;

        if (roles.Contains(RolePermissionSeeder.Parent))
        {
            var codes = await GetParentStudentCodesAsync(userId, ct);
            return codes.Contains(maHS);
        }

        return false;
    }

    public async Task<bool> CanEditStudentRecordAsync(string? userId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(userId)) return false;
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return false;
        var roles = await userManager.GetRolesAsync(user);
        return roles.Contains(RolePermissionSeeder.Admin);
    }

    public async Task<bool> CanEditScoreAsync(string? userId, string maHS, string maMon, string namHoc, byte hocKy, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(userId)) return false;
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return false;
        var roles = await userManager.GetRolesAsync(user);
        if (roles.Contains(RolePermissionSeeder.Admin)) return true;
        if (!roles.Contains(RolePermissionSeeder.Teacher)) return false;
        if (string.IsNullOrEmpty(user.MaGV)) return false;

        var maLop = await db.HocSinhs.AsNoTracking()
            .Where(h => h.MaHS == maHS)
            .Select(h => h.MaLop)
            .FirstOrDefaultAsync(ct);
        if (maLop is null) return false;

        var isCn = await db.LopHocs.AsNoTracking()
            .AnyAsync(l => l.MaLop == maLop && l.MaGVChuNhiem == user.MaGV, ct);
        if (isCn) return true;

        return await db.LichHocs.AsNoTracking()
            .AnyAsync(l => l.MaLop == maLop && l.MaMon == maMon && l.MaGV == user.MaGV, ct);
    }

    public async Task<bool> CanViewFinanceAsync(string? userId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(userId)) return false;
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return false;
        var roles = await userManager.GetRolesAsync(user);
        return RolePermissionSeeder.PermissionsForRoles(roles).Contains(AppPermissions.FinanceView);
    }

    public async Task<bool> CanManageFinanceAsync(string? userId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(userId)) return false;
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return false;
        var roles = await userManager.GetRolesAsync(user);
        return roles.Contains(RolePermissionSeeder.Admin) || roles.Contains(RolePermissionSeeder.Accountant);
    }
}
