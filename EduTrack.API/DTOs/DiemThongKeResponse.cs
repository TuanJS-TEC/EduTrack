namespace EduTrack.API.DTOs;

public sealed class DiemThongKeResponse
{
    public int SiSo { get; set; }
    public decimal? TbLop { get; set; }
    public List<BangDiemItemResponse> Top { get; set; } = [];
    public List<BangDiemItemResponse> Bottom { get; set; } = [];
    public Dictionary<int, int> Histogram { get; set; } = new();
}
