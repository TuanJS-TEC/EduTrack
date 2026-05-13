namespace EduTrack.API.DTOs;

public sealed class DiemSoBulkUpsertResultDto
{
    public int Updated { get; set; }
    public List<string> Errors { get; set; } = [];
}
