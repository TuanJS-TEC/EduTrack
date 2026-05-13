using Microsoft.AspNetCore.Identity;

namespace EduTrack.API.Models;

public sealed class ApplicationUser : IdentityUser
{
    /// <summary>Mã giáo viên (GV-xxxxx) khi user là giáo viên.</summary>
    [System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string? MaGV { get; set; }

    public string? HoTen { get; set; }
}
