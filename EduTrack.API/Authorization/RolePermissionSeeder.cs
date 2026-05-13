namespace EduTrack.API.Authorization;

public static class RolePermissionSeeder
{
    public const string Admin = "Admin";
    public const string Bgh = "BGH";
    public const string Teacher = "Teacher";
    public const string Accountant = "Accountant";
    public const string Parent = "Parent";

    public static IReadOnlyDictionary<string, string[]> RoleToPermissions { get; } =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [Admin] =
            [
                AppPermissions.UsersManage,
                AppPermissions.RolesManage,
                AppPermissions.SystemConfigure,
                AppPermissions.StudentsView,
                AppPermissions.StudentsEdit,
                AppPermissions.ScoresView,
                AppPermissions.ScoresEdit,
                AppPermissions.FinanceManage,
                AppPermissions.FinanceView,
                AppPermissions.NotificationsSend,
                AppPermissions.DashboardView,
                AppPermissions.ReportsView,
                AppPermissions.TeachersView,
            ],
            [Bgh] =
            [
                AppPermissions.DashboardView,
                AppPermissions.ReportsView,
                AppPermissions.StudentsView,
                AppPermissions.ScoresView,
                AppPermissions.FinanceView,
                AppPermissions.TeachersView,
            ],
            [Teacher] =
            [
                AppPermissions.StudentsView,
                AppPermissions.ScoresView,
                AppPermissions.ScoresEdit,
                AppPermissions.NotificationsSend,
            ],
            [Accountant] =
            [
                AppPermissions.FinanceManage,
                AppPermissions.FinanceView,
                AppPermissions.StudentsView,
                AppPermissions.ReportsView,
            ],
            [Parent] =
            [
                AppPermissions.StudentsView,
                AppPermissions.StudentsViewOwn,
                AppPermissions.ScoresView,
                AppPermissions.FinanceView,
            ],
        };

    public static IEnumerable<string> PermissionsForRoles(IEnumerable<string> roles)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (var r in roles)
        {
            if (RoleToPermissions.TryGetValue(r, out var perms))
                foreach (var p in perms) set.Add(p);
        }

        return set;
    }
}
