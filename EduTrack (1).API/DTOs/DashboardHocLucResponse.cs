namespace EduTrack.API.DTOs;

public sealed class DashboardHocLucResponse
{
    public int TongHocSinh { get; set; }
    public int Gioi { get; set; }
    public int Kha { get; set; }
    public int TrungBinh { get; set; }
    public int Yeu { get; set; }
    public int Kem { get; set; }

    public List<DashboardLopSummary> TheoLop { get; set; } = new();
}

public sealed class DashboardLopSummary
{
    public string MaLop { get; set; } = "";
    public string TenLop { get; set; } = "";
    public int SiSo { get; set; }
    public decimal? TbChung { get; set; }
}

