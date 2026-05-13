using EduTrack.API.Authorization;
using EduTrack.API.Data;
using EduTrack.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Services;

public sealed class DbSeeder(
    EduTrackDbContext db,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedRolesAsync(ct);
        await SeedUsersAsync(ct);

        if (await db.HocSinhs.AsNoTracking().AnyAsync(ct))
            return;

        await SeedAcademicDemoAsync(ct);
    }

    private async Task SeedRolesAsync(CancellationToken ct)
    {
        foreach (var role in RolePermissionSeeder.RoleToPermissions.Keys)
        {
            if (await roleManager.RoleExistsAsync(role)) continue;
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private async Task SeedUsersAsync(CancellationToken ct)
    {
        if (await userManager.FindByNameAsync("admin") is null)
        {
            var admin = new ApplicationUser { UserName = "admin", Email = "admin@edutrack.local", HoTen = "Quản trị" };
            await userManager.CreateAsync(admin, "Admin123");
            await userManager.AddToRoleAsync(admin, RolePermissionSeeder.Admin);
        }

        if (await userManager.FindByNameAsync("gvcn") is null)
        {
            var gvUser = new ApplicationUser
            {
                UserName = "gvcn",
                Email = "gvcn@edutrack.local",
                HoTen = "Giáo viên chủ nhiệm",
                MaGV = "GV-00001"
            };
            await userManager.CreateAsync(gvUser, "Teacher123");
            await userManager.AddToRoleAsync(gvUser, RolePermissionSeeder.Teacher);
        }

        if (await userManager.FindByNameAsync("bgh") is null)
        {
            var u = new ApplicationUser { UserName = "bgh", Email = "bgh@edutrack.local", HoTen = "Ban giám hiệu" };
            await userManager.CreateAsync(u, "Bgh12345");
            await userManager.AddToRoleAsync(u, RolePermissionSeeder.Bgh);
        }

        if (await userManager.FindByNameAsync("ketoan") is null)
        {
            var u = new ApplicationUser { UserName = "ketoan", Email = "ketoan@edutrack.local", HoTen = "Kế toán" };
            await userManager.CreateAsync(u, "Acct12345");
            await userManager.AddToRoleAsync(u, RolePermissionSeeder.Accountant);
        }

        var parentUser = await userManager.FindByNameAsync("phuhuynh");
        if (parentUser is null)
        {
            parentUser = new ApplicationUser { UserName = "phuhuynh", Email = "ph@edutrack.local", HoTen = "Phụ huynh mẫu" };
            await userManager.CreateAsync(parentUser, "Parent123");
            await userManager.AddToRoleAsync(parentUser, RolePermissionSeeder.Parent);
        }
    }

    private async Task SeedAcademicDemoAsync(CancellationToken ct)
    {
        const string namHoc = "2025-2026";

        if (!await db.KyHocs.AnyAsync(k => k.NamHoc == namHoc && k.HocKy == 1, ct))
            db.KyHocs.Add(new KyHoc { NamHoc = namHoc, HocKy = 1, Locked = false });

        var gv =
            new[]
            {
                new GiaoVien { MaGV = "GV-00001", HoTen = "Nguyễn Văn An", ChuyenMon = "Toán", Email = "an.gv@edutrack.local", LuongCoBan = 12000000 },
                new GiaoVien { MaGV = "GV-00002", HoTen = "Trần Thị Bình", ChuyenMon = "Văn", Email = "binh.gv@edutrack.local", LuongCoBan = 11500000 },
                new GiaoVien { MaGV = "GV-00003", HoTen = "Lê Minh Cường", ChuyenMon = "Anh", Email = "cuong.gv@edutrack.local", LuongCoBan = 11000000 },
                new GiaoVien { MaGV = "GV-00004", HoTen = "Phạm Thu Dung", ChuyenMon = "Lý", Email = "dung.gv@edutrack.local", LuongCoBan = 11200000 },
            };

        var lop = new[]
        {
            new LopHoc { MaLop = "10A1-2025", TenLop = "10A1", KhoiLop = "10", NamHoc = namHoc, MaGVChuNhiem = "GV-00001" },
            new LopHoc { MaLop = "10A2-2025", TenLop = "10A2", KhoiLop = "10", NamHoc = namHoc, MaGVChuNhiem = "GV-00002" },
            new LopHoc { MaLop = "11B1-2025", TenLop = "11B1", KhoiLop = "11", NamHoc = namHoc, MaGVChuNhiem = "GV-00003" },
            new LopHoc { MaLop = "12C1-2025", TenLop = "12C1", KhoiLop = "12", NamHoc = namHoc, MaGVChuNhiem = "GV-00004" },
        };

        var mon = new[]
        {
            new MonHoc { MaMon = "SUB-MATH", TenMon = "Toán", SoTiet = 70, HeSoThi = 1.0m, MaGV = "GV-00001" },
            new MonHoc { MaMon = "SUB-LIT", TenMon = "Ngữ văn", SoTiet = 70, HeSoThi = 1.0m, MaGV = "GV-00002" },
            new MonHoc { MaMon = "SUB-ENG", TenMon = "Tiếng Anh", SoTiet = 60, HeSoThi = 1.0m, MaGV = "GV-00003" },
            new MonHoc { MaMon = "SUB-PHY", TenMon = "Vật lý", SoTiet = 50, HeSoThi = 1.0m, MaGV = "GV-00004" },
        };

        db.GiaoViens.AddRange(gv);
        db.LopHocs.AddRange(lop);
        db.MonHocs.AddRange(mon);
        await db.SaveChangesAsync(ct);

        var hs = new List<HocSinh>();
        const int enrollYear = 2025;
        for (var i = 1; i <= 30; i++)
        {
            var ma = $"HS-{enrollYear}-{i:D5}";
            var maLop = i <= 15 ? "10A1-2025" : "10A2-2025";
            hs.Add(new HocSinh
            {
                MaHS = ma,
                HoTen = i <= 15 ? $"Học sinh A{i:00}" : $"Học sinh B{i - 15:00}",
                NgaySinh = new DateTime(2009, ((i - 1) % 12) + 1, ((i - 1) % 27) + 1),
                DiaChi = "Hà Nội",
                MaLop = maLop,
                Email_PhuHuynh = $"phuhuynh.{ma.ToLowerInvariant()}@mail.local",
                SDT_PhuHuynh = $"09{(12340000 + i):000000}",
            });
        }

        db.HocSinhs.AddRange(hs);
        await db.SaveChangesAsync(ct);

        var parent = await userManager.FindByNameAsync("phuhuynh");
        if (parent is not null && !await db.ParentStudentLinks.AnyAsync(x => x.UserId == parent.Id, ct))
        {
            var firstHs = hs[0].MaHS;
            db.ParentStudentLinks.Add(new ParentStudentLink { UserId = parent.Id, MaHS = firstHs });
            await db.SaveChangesAsync(ct);
        }

        var rnd = new Random(42);
        static decimal Clamp(decimal v) => Math.Max(0m, Math.Min(10m, v));
        decimal NextScore(decimal mean, decimal spread)
        {
            var u = (decimal)rnd.NextDouble();
            var v = (decimal)rnd.NextDouble();
            var z = u + v - 1m;
            return Math.Round(Clamp(mean + z * spread), 2, MidpointRounding.AwayFromZero);
        }

        var diem = new List<DiemSo>();
        foreach (var s in hs)
        {
            var baseMean = s.MaLop == "10A1-2025" ? 6.8m : 6.2m;
            if (s.MaHS.EndsWith("00001") || s.MaHS.EndsWith("00002")) baseMean = 8.4m;
            if (s.MaHS.EndsWith("00014") || s.MaHS.EndsWith("00029")) baseMean = 4.2m;

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
                    NamHoc = namHoc,
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
        await db.SaveChangesAsync(ct);

        var hocPhi = hs.Select((s, idx) => new HocPhi
        {
            MaHS = s.MaHS,
            HocKy = 1,
            SoTien = idx % 3 == 0 ? 3000000m : 2500000m,
            NgayDong = idx < 18 ? DateTime.Today.AddDays(-(idx + 1)) : DateTime.Today.AddDays(15 - idx),
            TrangThai = idx < 12 ? "PAID" : idx < 22 ? "UNPAID" : "OVERDUE"
        }).ToList();

        var thongBao = new List<ThongBao>
        {
            new ThongBao { TieuDe = "Thông báo họp PH", NoiDung = "Mời phụ huynh tham dự họp đầu kỳ.", LoaiTB = "SuKien", NgayGui = DateTime.UtcNow.AddDays(-7) },
            new ThongBao { TieuDe = "Nhắc học phí", NoiDung = "Vui lòng hoàn tất học phí học kỳ 1.", LoaiTB = "HocPhi", NgayGui = DateTime.UtcNow.AddDays(-3), MaHS = hs[2].MaHS },
        };

        db.HocPhis.AddRange(hocPhi);
        db.ThongBaos.AddRange(thongBao);

        await db.SaveChangesAsync(ct);
    }
}
