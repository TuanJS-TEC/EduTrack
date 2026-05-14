namespace EduTrack.API.Authorization;

public static class AppPermissions
{
    public const string UsersManage = "Users.Manage";
    public const string RolesManage = "Roles.Manage";
    public const string SystemConfigure = "System.Configure";

    public const string StudentsView = "Students.View";
    public const string StudentsEdit = "Students.Edit";
    public const string StudentsViewOwn = "Students.ViewOwn";

    public const string ScoresView = "Scores.View";
    public const string ScoresEdit = "Scores.Edit";

    public const string FinanceManage = "Finance.Manage";
    public const string FinanceView = "Finance.View";

    public const string NotificationsSend = "Notifications.Send";

    public const string DashboardView = "Dashboard.View";
    public const string ReportsView = "Reports.View";

    public const string TeachersView = "Teachers.View";
}

public static class AppPolicies
{
    public const string CanManageUsers = nameof(CanManageUsers);
    public const string CanManageRoles = nameof(CanManageRoles);
    public const string CanConfigureSystem = nameof(CanConfigureSystem);
    public const string CanViewStudents = nameof(CanViewStudents);
    public const string CanEditStudents = nameof(CanEditStudents);
    public const string CanViewOwnStudents = nameof(CanViewOwnStudents);
    public const string CanViewScores = nameof(CanViewScores);
    public const string CanEditScores = nameof(CanEditScores);
    public const string CanManageFinance = nameof(CanManageFinance);
    public const string CanViewFinance = nameof(CanViewFinance);
    public const string CanSendNotifications = nameof(CanSendNotifications);
    public const string CanViewDashboard = nameof(CanViewDashboard);
    public const string CanViewReports = nameof(CanViewReports);
    public const string CanViewTeachers = nameof(CanViewTeachers);
    /// <summary>Thêm/sửa/xóa hồ sơ giáo viên (Admin, BGH).</summary>
    public const string CanManageTeachers = nameof(CanManageTeachers);
}
