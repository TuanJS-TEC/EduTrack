using EduTrack.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Data;

public sealed class EduTrackDbContext(DbContextOptions<EduTrackDbContext> options) : DbContext(options)
{
    public DbSet<HocSinh> HocSinhs => Set<HocSinh>();
    public DbSet<GiaoVien> GiaoViens => Set<GiaoVien>();
    public DbSet<LopHoc> LopHocs => Set<LopHoc>();
    public DbSet<MonHoc> MonHocs => Set<MonHoc>();
    public DbSet<DiemSo> DiemSos => Set<DiemSo>();
    public DbSet<LichHoc> LichHocs => Set<LichHoc>();
    public DbSet<HocPhi> HocPhis => Set<HocPhi>();
    public DbSet<ThongBao> ThongBaos => Set<ThongBao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // BẮT BUỘC: Map sang danh từ số ít để khớp với DB của bạn. KHÔNG ĐƯỢC XÓA!
        modelBuilder.Entity<HocSinh>().ToTable("HocSinh");
        modelBuilder.Entity<GiaoVien>().ToTable("GiaoVien");
        modelBuilder.Entity<LopHoc>().ToTable("LopHoc");
        modelBuilder.Entity<MonHoc>().ToTable("MonHoc");
        modelBuilder.Entity<DiemSo>().ToTable("DiemSo");
        modelBuilder.Entity<LichHoc>().ToTable("LichHoc");
        modelBuilder.Entity<HocPhi>().ToTable("HocPhi");
        modelBuilder.Entity<ThongBao>().ToTable("ThongBao");

        modelBuilder.Entity<HocSinh>()
            .HasOne(x => x.LopHoc)
            .WithMany(x => x.HocSinhs)
            .HasForeignKey(x => x.MaLop)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LopHoc>()
            .HasOne(x => x.GiaoVienChuNhiem)
            .WithMany(x => x.LopChuNhiems)
            .HasForeignKey(x => x.MaGVChuNhiem)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<MonHoc>()
            .HasOne(x => x.GiaoVien)
            .WithMany(x => x.MonHocs)
            .HasForeignKey(x => x.MaGV)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<DiemSo>()
            .HasOne(x => x.HocSinh)
            .WithMany(x => x.DiemSos)
            .HasForeignKey(x => x.MaHS)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DiemSo>()
            .HasOne(x => x.MonHoc)
            .WithMany(x => x.DiemSos)
            .HasForeignKey(x => x.MaMon)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LichHoc>()
            .HasOne(x => x.MonHoc)
            .WithMany(x => x.LichHocs)
            .HasForeignKey(x => x.MaMon)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LichHoc>()
            .HasOne(x => x.LopHoc)
            .WithMany(x => x.LichHocs)
            .HasForeignKey(x => x.MaLop)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LichHoc>()
            .HasOne(x => x.GiaoVien)
            .WithMany(x => x.LichHocs)
            .HasForeignKey(x => x.MaGV)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<HocPhi>()
            .HasOne(x => x.HocSinh)
            .WithMany(x => x.HocPhis)
            .HasForeignKey(x => x.MaHS)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ThongBao>()
            .HasOne(x => x.HocSinh)
            .WithMany(x => x.ThongBaos)
            .HasForeignKey(x => x.MaHS)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<DiemSo>()
            .HasIndex(x => new { x.MaHS, x.MaMon, x.HocKy })
            .IsUnique();
    }
}

