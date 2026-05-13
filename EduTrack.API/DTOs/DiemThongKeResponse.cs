namespace EduTrack.API.DTOs;

public sealed class DiemThongKeResponse
{
    public int SiSo { get; set; }

    /// <summary>Số HS có TBM (đủ điểm tính trung bình môn).</summary>
    public int SoHocSinhCoTbm { get; set; }

    /// <summary>Số HS chưa có điểm thành phần nào.</summary>
    public int SoHocSinhChuaCoDiem { get; set; }

    public decimal? TbLop { get; set; }
    public List<BangDiemItemResponse> Top { get; set; } = [];
    public List<BangDiemItemResponse> Bottom { get; set; } = [];

    /// <summary>Phần nguyên của TBM → số lượng (tương thích bản cũ).</summary>
    public Dictionary<int, int> Histogram { get; set; } = new();

    /// <summary>Khoảng điểm TBM: 0-2, 2-4, …, 8-10.</summary>
    public Dictionary<string, int> PhanBoMucDiem { get; set; } = new();

    /// <summary>Đếm theo XepLoai (Gioi, Kha, …) — chỉ HS có TBM.</summary>
    public Dictionary<string, int> PhanBoXepLoai { get; set; } = new();
}
