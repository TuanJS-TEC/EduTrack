using EduTrack.API.Data;
using EduTrack.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Services;

public sealed class DbSeeder(EduTrackDbContext db)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        // Idempotent: nếu đã có HS hoặc điểm thì bỏ qua
        if (await db.HocSinhs.AsNoTracking().AnyAsync(ct) || await db.DiemSos.AsNoTracking().AnyAsync(ct))
            return;

        const string namHoc = "2025-2026";

        var gv = new[]
        {
            new GiaoVien { MaGV = "GV001", HoTen = "Nguyễn Văn An", ChuyenMon = "Toán", Email = "an.gv@edutrack.local", LuongCoBan = 12000000 },
            new GiaoVien { MaGV = "GV002", HoTen = "Trần Thị Bình", ChuyenMon = "Văn", Email = "binh.gv@edutrack.local", LuongCoBan = 11500000 },
            new GiaoVien { MaGV = "GV003", HoTen = "Lê Minh Cường", ChuyenMon = "Anh", Email = "cuong.gv@edutrack.local", LuongCoBan = 11000000 },
            new GiaoVien { MaGV = "GV004", HoTen = "Phạm Thu Dung", ChuyenMon = "Lý", Email = "dung.gv@edutrack.local", LuongCoBan = 11200000 },
        };

        var lop = new[]
        {
            new LopHoc { MaLop = "10A1", TenLop = "10A1", KhoiLop = 10, NamHoc = namHoc, MaGVChuNhiem = "GV001" },
            new LopHoc { MaLop = "10A2", TenLop = "10A2", KhoiLop = 10, NamHoc = namHoc, MaGVChuNhiem = "GV002" },
        };

        var mon = new[]
        {
            new MonHoc { MaMon = "TOAN", TenMon = "Toán", SoTiet = 70, HeSoThi = 1, MaGV = "GV001" },
            new MonHoc { MaMon = "VAN", TenMon = "Ngữ văn", SoTiet = 70, HeSoThi = 1, MaGV = "GV002" },
            new MonHoc { MaMon = "ANH", TenMon = "Tiếng Anh", SoTiet = 60, HeSoThi = 1, MaGV = "GV003" },
            new MonHoc { MaMon = "LY", TenMon = "Vật lý", SoTiet = 50, HeSoThi = 1, MaGV = "GV004" },
        };

        var hs = new List<HocSinh>();
        for (var i = 1; i <= 30; i++)
        {
            var ma = $"HS{i:000}";
            var maLop = i <= 15 ? "10A1" : "10A2";
            hs.Add(new HocSinh
            {
                MaHS = ma,
                HoTen = i <= 15 ? $"Học sinh A{i:00}" : $"Học sinh B{i - 15:00}",
                NgaySinh = new DateTime(2009, ((i - 1) % 12) + 1, ((i - 1) % 27) + 1),
                DiaChi = "Hà Nội",
                MaLop = maLop,
                Email_PhuHuynh = $"phuhuynh.{ma.ToLower()}@mail.local",
                SDT_PhuHuynh = $"09{(12340000 + i):000000}",
            });
        }

        db.GiaoViens.AddRange(gv);
        db.LopHocs.AddRange(lop);
        db.MonHocs.AddRange(mon);
        db.HocSinhs.AddRange(hs);
        await db.SaveChangesAsync(ct);

        // Điểm HK1: tạo ngẫu nhiên có kiểm soát để DSS/Dashboard có phân hoá
        var rnd = new Random(42);
        static decimal Clamp(decimal v) => Math.Max(0m, Math.Min(10m, v));
        decimal NextScore(decimal mean, decimal spread)
        {
            // Box-Muller (approx) đơn giản bằng 2 uniform
            var u = (decimal)rnd.NextDouble();
            var v = (decimal)rnd.NextDouble();
            var z = (u + v - 1m); // ~[-1,1]
            return Math.Round(Clamp(mean + z * spread), 2, MidpointRounding.AwayFromZero);
        }

        var diem = new List<DiemSo>();
        foreach (var s in hs)
        {
            // nhóm học sinh khá/giỏi nhiều hơn chút cho demo
            var baseMean = s.MaLop == "10A1" ? 6.8m : 6.2m;
            if (s.MaHS.EndsWith("001") || s.MaHS.EndsWith("002")) baseMean = 8.4m;      // top
            if (s.MaHS.EndsWith("014") || s.MaHS.EndsWith("029")) baseMean = 4.2m;      // risk

            foreach (var m in mon)
            {
                var diemM = NextScore(baseMean, 1.0m);
                var diem15 = NextScore(baseMean, 1.2m);
                var gk = NextScore(baseMean, 1.3m);
                var ck = NextScore(baseMean, 1.6m);

                var tb = GradeCalculator.CalcTbMon(diemM, diem15, gk, ck);
                diem.Add(new DiemSo
                {
                    MaHS = s.MaHS,
                    MaMon = m.MaMon,
                    HocKy = 1,
                    DiemMieng = diemM,
                    Diem15p = diem15,
                    DiemGiuaKy = gk,
                    DiemCuoiKy = ck,
                    DiemTBMon = tb
                });
            }
        }

        db.DiemSos.AddRange(diem);

        // Học phí + thông báo mẫu
        var hocPhi = hs.Take(10).Select((s, idx) => new HocPhi
        {
            MaHS = s.MaHS,
            HocKy = 1,
            SoTien = 2500000,
            NgayDong = DateTime.Today.AddDays(-idx),
            TrangThai = "DaDong"
        }).ToList();

        var thongBao = new List<ThongBao>
        {
            new ThongBao { TieuDe = "Thông báo họp PH", NoiDung = "Mời phụ huynh tham dự họp đầu kỳ.", LoaiTB = "SuKien", NgayGui = DateTime.UtcNow.AddDays(-7) },
            new ThongBao { TieuDe = "Nhắc học phí", NoiDung = "Vui lòng hoàn tất học phí học kỳ 1.", LoaiTB = "HocPhi", NgayGui = DateTime.UtcNow.AddDays(-3), MaHS = "HS003" },
        };

        db.HocPhis.AddRange(hocPhi);
        db.ThongBaos.AddRange(thongBao);

        await db.SaveChangesAsync(ct);
    }
}

