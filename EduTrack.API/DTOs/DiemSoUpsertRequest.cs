namespace EduTrack.API.DTOs;

public sealed class DiemSoUpsertRequest
{
    public string MaHS { get; set; } = "";
    public string MaMon { get; set; } = "";
    public string NamHoc { get; set; } = "2025-2026";
    public byte HocKy { get; set; }

    /// <summary>Nhiều điểm miệng (ưu tiên hơn DiemMieng đơn).</summary>
    public List<decimal>? DiemMiengs { get; set; }

    /// <summary>Nhiều điểm 15 phút.</summary>
    public List<decimal>? Diem15ps { get; set; }

    public decimal? DiemMieng { get; set; }
    public decimal? Diem15p { get; set; }
    public decimal? DiemGiuaKy { get; set; }
    public decimal? DiemCuoiKy { get; set; }
}

public sealed class DiemSoBulkUpsertRequest
{
    public string NamHoc { get; set; } = "2025-2026";
    public byte HocKy { get; set; }
    public string MaMon { get; set; } = "";
    public string MaLop { get; set; } = "";
    public List<DiemSoUpsertRequest> Rows { get; set; } = [];
}

