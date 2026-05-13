namespace EduTrack.API.DTOs;

public sealed class DssInterventionItemDto
{
    public string MaHS { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string MaLop { get; set; } = "";
    public decimal? TbHocKy { get; set; }
    public int SoMonNguyCo { get; set; }
    public string MucRuiRo { get; set; } = "Thap";
    public List<string> MonUuTien { get; set; } = [];
    public List<string> KhuyenNghi { get; set; } = [];
}

public sealed class DssInterventionResponse
{
    public string NamHoc { get; set; } = "";
    public byte HocKy { get; set; }
    public string? MaLop { get; set; }
    public List<DssInterventionItemDto> DanhSachCanThiep { get; set; } = [];
}

public sealed class DssScenarioAdjustSubjectDto
{
    public string MaMon { get; set; } = "";
    public decimal DiemCuoiKyGiaDinh { get; set; }
}

public sealed class DssScenarioStudentDto
{
    public string MaHS { get; set; } = "";
    public List<DssScenarioAdjustSubjectDto> DieuChinhMonHoc { get; set; } = [];
}

public sealed class DssMultiScenarioRequest
{
    public string NamHoc { get; set; } = "2025-2026";
    public byte HocKy { get; set; } = 1;
    public string? MaLop { get; set; }
    public List<DssScenarioStudentDto> Scenarios { get; set; } = [];
}

public sealed class DssMultiScenarioStudentResultDto
{
    public string MaHS { get; set; } = "";
    public decimal? TbTruoc { get; set; }
    public decimal? TbSau { get; set; }
    public decimal? ChenhLech { get; set; }
    public string? XepLoaiTruoc { get; set; }
    public string? XepLoaiSau { get; set; }
    public List<string> MonTacDong { get; set; } = [];
}

public sealed class DssMultiScenarioResponse
{
    public string NamHoc { get; set; } = "";
    public byte HocKy { get; set; }
    public string? MaLop { get; set; }
    public List<DssMultiScenarioStudentResultDto> KetQua { get; set; } = [];
}
