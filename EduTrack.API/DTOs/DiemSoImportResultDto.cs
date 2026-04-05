namespace EduTrack.API.DTOs;

public sealed class DiemSoImportResultDto
{
    public int Imported { get; set; }
    public int Skipped { get; set; }
    public List<string> Warnings { get; set; } = [];
}
