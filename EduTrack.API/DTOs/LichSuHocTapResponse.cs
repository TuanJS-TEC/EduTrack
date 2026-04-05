namespace EduTrack.API.DTOs;

public sealed class LichSuHocTapResponse
{
    public string NamHoc { get; set; } = "";
    public byte HocKy { get; set; }
    public decimal? Tbc { get; set; }
    public string? HocLuc { get; set; }
    public int SoMonCoDiem { get; set; }
}
