namespace EduTrack.API.DTOs;

public sealed class HocSinhImportResultDto
{
    public int Imported { get; set; }
    public int Skipped { get; set; }
    public List<string> Warnings { get; set; } = [];
}
