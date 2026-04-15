namespace EduTrack.API.DTOs;

public sealed class OperationalReportFilterDto
{
    public string NamHoc { get; set; } = "2025-2026";
    public byte HocKy { get; set; } = 1;
    public string? MaLop { get; set; }
    public string? KhoiLop { get; set; }
}

public sealed class BghDashboardDto
{
    public OperationalReportFilterDto Filter { get; set; } = new();
    public int TongLop { get; set; }
    public int TongHocSinh { get; set; }
    public int SoHocSinhCoDiemTongKet { get; set; }
    public decimal? DiemTrungBinhToanTruong { get; set; }
    public Dictionary<string, int> PhanBoHocLuc { get; set; } = new();
    public decimal TongHocPhiPhaiThu { get; set; }
    public decimal TongHocPhiDaThu { get; set; }
    public decimal TyLeThuHocPhi { get; set; }
}

public sealed class KeToanDashboardDto
{
    public OperationalReportFilterDto Filter { get; set; } = new();
    public int TongHoSoHocPhi { get; set; }
    public int SoHoSoDaThu { get; set; }
    public int SoHoSoChuaThu { get; set; }
    public decimal TongPhaiThu { get; set; }
    public decimal TongDaThu { get; set; }
    public decimal TongConNo { get; set; }
    public Dictionary<string, decimal> ThuTheoLop { get; set; } = new();
}

public sealed class GvcnDashboardDto
{
    public OperationalReportFilterDto Filter { get; set; } = new();
    public string MaLop { get; set; } = "";
    public string? TenLop { get; set; }
    public int SiSo { get; set; }
    public int SoHocSinhCoTbHocKy { get; set; }
    public decimal? DiemTrungBinhLop { get; set; }
    public int SoHocSinhNguyCo { get; set; }
    public List<GvcnCanhBaoItemDto> CanhBao { get; set; } = [];
}

public sealed class GvcnCanhBaoItemDto
{
    public string MaHS { get; set; } = "";
    public string HoTen { get; set; } = "";
    public decimal? DiemTrungBinh { get; set; }
    public int SoMonDuoiTrungBinh { get; set; }
}
