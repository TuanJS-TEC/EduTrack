namespace EduTrack.API.DTOs;

public sealed class HocSinhFullProfileResponse
{
    public HocSinhResponse HocSinh { get; set; } = null!;
    public string? TenLop { get; set; }
    public string? KhoiLop { get; set; }
    public string? NamHocLop { get; set; }

    public List<DiemKyTomTatDto> DiemTheoKy { get; set; } = [];
    public List<HocPhiResponseDto> HocPhis { get; set; } = [];
    public List<ThongBaoTomTatDto> ThongBaos { get; set; } = [];
}

public sealed class DiemKyTomTatDto
{
    public string NamHoc { get; set; } = "";
    public byte HocKy { get; set; }
    public List<BangDiemItemResponse> Mon { get; set; } = [];
    public string? HocLuc { get; set; }
}

public sealed class ThongBaoTomTatDto
{
    public int MaTB { get; set; }
    public string TieuDe { get; set; } = "";
    public string? LoaiTB { get; set; }
    public bool DaDoc { get; set; }
    public DateTime? NgayDoc { get; set; }
    public DateTime NgayGui { get; set; }
    public string? NoiDung { get; set; }
}
