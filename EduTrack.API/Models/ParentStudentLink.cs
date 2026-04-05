using System.ComponentModel.DataAnnotations;

namespace EduTrack.API.Models;

public sealed class ParentStudentLink
{
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = "";

    [Required]
    [MaxLength(20)]
    public string MaHS { get; set; } = "";

    public ApplicationUser? User { get; set; }
    public HocSinh? HocSinh { get; set; }
}
