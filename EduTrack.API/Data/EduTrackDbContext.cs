using EduTrack.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Data;

public sealed class EduTrackDbContext(DbContextOptions<EduTrackDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
{
    public DbSet<HocSinh> HocSinhs => Set<HocSinh>();
    public DbSet<GiaoVien> GiaoViens => Set<GiaoVien>();
    public DbSet<LopHoc> LopHocs => Set<LopHoc>();
    public DbSet<MonHoc> MonHocs => Set<MonHoc>();
    public DbSet<DiemSo> DiemSos => Set<DiemSo>();
    public DbSet<DiemThanhPhan> DiemThanhPhans => Set<DiemThanhPhan>();
    public DbSet<LichHoc> LichHocs => Set<LichHoc>();
    public DbSet<HocPhi> HocPhis => Set<HocPhi>();
    public DbSet<ThongBao> ThongBaos => Set<ThongBao>();
    public DbSet<KyHoc> KyHocs => Set<KyHoc>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ParentStudentLink> ParentStudentLinks => Set<ParentStudentLink>();
    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();
    public DbSet<KyHocWorkflowLog> KyHocWorkflowLogs => Set<KyHocWorkflowLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<KyHoc>()
            .HasKey(x => new { x.NamHoc, x.HocKy });

        modelBuilder.Entity<KyHoc>()
            .Property(x => x.TrangThai)
            .HasMaxLength(20);

        modelBuilder.Entity<KyHocWorkflowLog>()
            .HasOne(x => x.KyHoc)
            .WithMany(x => x.WorkflowLogs)
            .HasForeignKey(x => new { x.NamHoc, x.HocKy })
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<KyHocWorkflowLog>()
            .HasIndex(x => new { x.NamHoc, x.HocKy, x.AtUtc });

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(x => x.TokenHash)
            .IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ParentStudentLink>()
            .HasIndex(x => new { x.UserId, x.MaHS })
            .IsUnique();

        modelBuilder.Entity<ParentStudentLink>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ParentStudentLink>()
            .HasOne(x => x.HocSinh)
            .WithMany()
            .HasForeignKey(x => x.MaHS)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DiemThanhPhan>()
            .HasOne(x => x.DiemSo)
            .WithMany(x => x.ThanhPhans)
            .HasForeignKey(x => x.MaDiem)
            .OnDelete(DeleteBehavior.Cascade);

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

        modelBuilder.Entity<ThongBao>()
            .HasIndex(x => new { x.MaHS, x.LoaiTB, x.DaDoc, x.NgayGui });

        modelBuilder.Entity<DiemSo>()
            .HasIndex(x => new { x.MaHS, x.MaMon, x.NamHoc, x.HocKy })
            .IsUnique();
    }
}
