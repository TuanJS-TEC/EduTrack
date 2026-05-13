using System.Text.RegularExpressions;

namespace EduTrack.API.Validation;

public static partial class EduCodeFormats
{
    [GeneratedRegex(@"^HS-\d{4}-\d{5}$", RegexOptions.CultureInvariant)]
    private static partial Regex StudentCodeRegex();

    [GeneratedRegex(@"^GV-\d{5}$", RegexOptions.CultureInvariant)]
    private static partial Regex TeacherCodeRegex();

    [GeneratedRegex(@"^\d{1,2}[A-Z]\d-\d{4}$", RegexOptions.CultureInvariant)]
    private static partial Regex ClassCodeRegex();

    [GeneratedRegex(@"^SUB-[A-Z0-9-]{2,10}$", RegexOptions.CultureInvariant)]
    private static partial Regex SubjectCodeRegex();

    [GeneratedRegex(@"^\d{4}-\d{4}$", RegexOptions.CultureInvariant)]
    private static partial Regex SchoolYearRegex();

    public static bool IsValidStudentCode(string? code) =>
        !string.IsNullOrWhiteSpace(code) && StudentCodeRegex().IsMatch(code.Trim());

    public static bool IsValidTeacherCode(string? code) =>
        !string.IsNullOrWhiteSpace(code) && TeacherCodeRegex().IsMatch(code.Trim());

    public static bool IsValidClassCode(string? code) =>
        !string.IsNullOrWhiteSpace(code) && ClassCodeRegex().IsMatch(code.Trim());

    public static bool IsValidSubjectCode(string? code) =>
        !string.IsNullOrWhiteSpace(code) && SubjectCodeRegex().IsMatch(code.Trim());

    public static bool IsValidSchoolYear(string? y) =>
        !string.IsNullOrWhiteSpace(y) && SchoolYearRegex().IsMatch(y.Trim());
}
