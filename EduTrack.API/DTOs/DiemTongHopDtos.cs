namespace EduTrack.API.DTOs;

public sealed class DiemMonKyItemDto
{
    public string MaMon { get; set; } = "";
    public string TenMon { get; set; } = "";
    public decimal? DiemTBMon { get; set; }
    public string TrangThaiNhapDiem { get; set; } = "";
}

public sealed class DiemTongHopHocSinhResponse
{
    public string MaHS { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string MaLop { get; set; } = "";
    public string NamHoc { get; set; } = "";
    public byte HocKy { get; set; }
    public List<DiemMonKyItemDto> TheoMon { get; set; } = [];
    public decimal? TbChungKy { get; set; }
    public int SoMonCoTbm { get; set; }
}

public sealed class DiemTongHopLopResponse
{
    public string MaLop { get; set; } = "";
    public string? TenLop { get; set; }
    public string NamHoc { get; set; } = "";
    public byte HocKy { get; set; }
    public int SiSo { get; set; }
    public int SoHocSinhCoTbChung { get; set; }
    public decimal? TbChungLop { get; set; }
}

public sealed class DiemTongHopKhoiResponse
{
    public string KhoiLop { get; set; } = "";
    public string NamHoc { get; set; } = "";
    public byte HocKy { get; set; }
    public int TongSoHocSinh { get; set; }
    public int SoHocSinhCoTbChung { get; set; }
    public decimal? TbChungKhoi { get; set; }
}

public sealed class DiemTongHopTruongResponse
{
    public string NamHoc { get; set; } = "";
    public byte HocKy { get; set; }
    public int TongSoHocSinh { get; set; }
    public int SoHocSinhCoTbChung { get; set; }
    public decimal? TbChungTruong { get; set; }
}
