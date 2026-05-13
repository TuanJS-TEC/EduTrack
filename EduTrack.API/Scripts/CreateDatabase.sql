/* ============================================================================
   EduTrack — Database recreation script
   Server:  DESKTOP-22UAJFI (Microsoft SQL Server 2025 Enterprise Developer)
   Target DB name: EduTrack
   Khớp với EduTrackDbContextModelSnapshot.cs (sau khi áp dụng 5 migrations):
     1) 20260326131106_InitialCreate
     2) 20260402033408_AddTrangThaiToHocSinh
     3) 20260405021309_IdentityRbacDiemNamHoc
     4) 20260414142215_AddThongBaoReadStateAndFilterIndex
     5) 20260414150038_AddKyHocWorkflowAndAuditEnhancements
   ----------------------------------------------------------------------------
   Cách dùng:
     - Mở SSMS / Azure Data Studio, kết nối tới DESKTOP-22UAJFI.
     - Mở file này, F5 để chạy toàn bộ.
     - Sau khi script chạy xong, chạy lại API (dotnet run trong EduTrack.API).
       DbSeeder sẽ tự seed admin/gvcn/bgh/ketoan/phuhuynh + dữ liệu demo.
   ============================================================================ */

USE master;
GO

IF DB_ID(N'EduTrack') IS NOT NULL
BEGIN
    ALTER DATABASE [EduTrack] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [EduTrack];
END
GO

CREATE DATABASE [EduTrack]
    COLLATE Latin1_General_CI_AS;
GO

ALTER DATABASE [EduTrack] SET RECOVERY SIMPLE;
GO

USE [EduTrack];
GO

/* ============================================================================
   0. __EFMigrationsHistory  (EF Core sẽ KHÔNG áp lại migration đã ghi vào đây)
   ============================================================================ */
CREATE TABLE [dbo].[__EFMigrationsHistory] (
    [MigrationId]    nvarchar(150) NOT NULL,
    [ProductVersion] nvarchar(32)  NOT NULL,
    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED ([MigrationId])
);
GO

/* ============================================================================
   1. Bảng độc lập (không có FK)
   ============================================================================ */

CREATE TABLE [dbo].[GiaoViens] (
    [MaGV]       nvarchar(20)   NOT NULL,
    [HoTen]      nvarchar(100)  NOT NULL,
    [ChuyenMon]  nvarchar(100)  NULL,
    [Email]      nvarchar(255)  NULL,
    [LuongCoBan] decimal(18,2)  NULL,
    CONSTRAINT [PK_GiaoViens] PRIMARY KEY CLUSTERED ([MaGV])
);
GO

CREATE TABLE [dbo].[KyHocs] (
    [NamHoc]         nvarchar(12) NOT NULL,
    [HocKy]          tinyint      NOT NULL,
    [Locked]         bit          NOT NULL,
    [TrangThai]      nvarchar(20) NOT NULL CONSTRAINT [DF_KyHocs_TrangThai] DEFAULT (N'Mo'),
    [SubmittedAtUtc] datetime2    NULL,
    [ApprovedAtUtc]  datetime2    NULL,
    [ReopenedAtUtc]  datetime2    NULL,
    CONSTRAINT [PK_KyHocs] PRIMARY KEY CLUSTERED ([NamHoc], [HocKy])
);
GO

CREATE TABLE [dbo].[AuditLogEntries] (
    [Id]            bigint         IDENTITY(1,1) NOT NULL,
    [UserId]        nvarchar(450)  NULL,
    [UserName]      nvarchar(256)  NULL,
    [Action]        nvarchar(64)   NOT NULL,
    [EntityType]    nvarchar(64)   NOT NULL,
    [EntityKey]     nvarchar(256)  NULL,
    [OldSnapshot]   nvarchar(max)  NULL,
    [NewSnapshot]   nvarchar(max)  NULL,
    [ViolationCode] nvarchar(64)   NULL,
    [Severity]      nvarchar(32)   NULL,
    [MetadataJson]  nvarchar(max)  NULL,
    [AtUtc]         datetime2      NOT NULL,
    CONSTRAINT [PK_AuditLogEntries] PRIMARY KEY CLUSTERED ([Id])
);
GO

/* ============================================================================
   2. ASP.NET Core Identity
   ============================================================================ */

CREATE TABLE [dbo].[AspNetRoles] (
    [Id]               nvarchar(450)  NOT NULL,
    [Name]             nvarchar(256)  NULL,
    [NormalizedName]   nvarchar(256)  NULL,
    [ConcurrencyStamp] nvarchar(max)  NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED ([Id])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex]
    ON [dbo].[AspNetRoles] ([NormalizedName])
    WHERE [NormalizedName] IS NOT NULL;
GO

CREATE TABLE [dbo].[AspNetUsers] (
    [Id]                   nvarchar(450)  NOT NULL,
    [MaGV]                 nvarchar(20)   NULL,
    [HoTen]                nvarchar(max)  NULL,
    [UserName]             nvarchar(256)  NULL,
    [NormalizedUserName]   nvarchar(256)  NULL,
    [Email]                nvarchar(256)  NULL,
    [NormalizedEmail]      nvarchar(256)  NULL,
    [EmailConfirmed]       bit            NOT NULL,
    [PasswordHash]         nvarchar(max)  NULL,
    [SecurityStamp]        nvarchar(max)  NULL,
    [ConcurrencyStamp]     nvarchar(max)  NULL,
    [PhoneNumber]          nvarchar(max)  NULL,
    [PhoneNumberConfirmed] bit            NOT NULL,
    [TwoFactorEnabled]     bit            NOT NULL,
    [LockoutEnd]           datetimeoffset NULL,
    [LockoutEnabled]       bit            NOT NULL,
    [AccessFailedCount]    int            NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED ([Id])
);
GO
CREATE NONCLUSTERED INDEX [EmailIndex]
    ON [dbo].[AspNetUsers] ([NormalizedEmail]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex]
    ON [dbo].[AspNetUsers] ([NormalizedUserName])
    WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE TABLE [dbo].[AspNetRoleClaims] (
    [Id]         int            IDENTITY(1,1) NOT NULL,
    [RoleId]     nvarchar(450)  NOT NULL,
    [ClaimType]  nvarchar(max)  NULL,
    [ClaimValue] nvarchar(max)  NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
        FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles]([Id]) ON DELETE CASCADE
);
GO
CREATE NONCLUSTERED INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims] ([RoleId]);
GO

CREATE TABLE [dbo].[AspNetUserClaims] (
    [Id]         int            IDENTITY(1,1) NOT NULL,
    [UserId]     nvarchar(450)  NOT NULL,
    [ClaimType]  nvarchar(max)  NULL,
    [ClaimValue] nvarchar(max)  NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
);
GO
CREATE NONCLUSTERED INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims] ([UserId]);
GO

CREATE TABLE [dbo].[AspNetUserLogins] (
    [LoginProvider]       nvarchar(450)  NOT NULL,
    [ProviderKey]         nvarchar(450)  NOT NULL,
    [ProviderDisplayName] nvarchar(max)  NULL,
    [UserId]              nvarchar(450)  NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
);
GO
CREATE NONCLUSTERED INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins] ([UserId]);
GO

CREATE TABLE [dbo].[AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
        FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
);
GO
CREATE NONCLUSTERED INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles] ([RoleId]);
GO

CREATE TABLE [dbo].[AspNetUserTokens] (
    [UserId]        nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name]          nvarchar(450) NOT NULL,
    [Value]         nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [dbo].[RefreshTokens] (
    [Id]                  int             IDENTITY(1,1) NOT NULL,
    [UserId]              nvarchar(450)   NOT NULL,
    [TokenHash]           nvarchar(128)   NOT NULL,
    [ExpiresAtUtc]        datetime2       NOT NULL,
    [CreatedAtUtc]        datetime2       NOT NULL,
    [RevokedAtUtc]        datetime2       NULL,
    [ReplacedByTokenHash] nvarchar(128)   NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_RefreshTokens_AspNetUsers_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RefreshTokens_TokenHash] ON [dbo].[RefreshTokens] ([TokenHash]);
CREATE NONCLUSTERED INDEX [IX_RefreshTokens_UserId] ON [dbo].[RefreshTokens] ([UserId]);
GO

/* ============================================================================
   3. Bảng nghiệp vụ — phụ thuộc GiaoViens / LopHocs / MonHocs / HocSinhs
   ============================================================================ */

CREATE TABLE [dbo].[LopHocs] (
    [MaLop]        nvarchar(20)  NOT NULL,
    [TenLop]       nvarchar(100) NOT NULL,
    [KhoiLop]      nvarchar(20)  NULL,
    [NamHoc]       nvarchar(20)  NULL,
    [MaGVChuNhiem] nvarchar(20)  NULL,
    CONSTRAINT [PK_LopHocs] PRIMARY KEY CLUSTERED ([MaLop]),
    CONSTRAINT [FK_LopHocs_GiaoViens_MaGVChuNhiem]
        FOREIGN KEY ([MaGVChuNhiem]) REFERENCES [dbo].[GiaoViens]([MaGV]) ON DELETE SET NULL
);
GO
CREATE NONCLUSTERED INDEX [IX_LopHocs_MaGVChuNhiem] ON [dbo].[LopHocs] ([MaGVChuNhiem]);
GO

CREATE TABLE [dbo].[MonHocs] (
    [MaMon]   nvarchar(20)  NOT NULL,
    [TenMon]  nvarchar(100) NOT NULL,
    [SoTiet]  int           NULL,
    [HeSoThi] decimal(4,2)  NULL,
    [MaGV]    nvarchar(20)  NULL,
    CONSTRAINT [PK_MonHocs] PRIMARY KEY CLUSTERED ([MaMon]),
    CONSTRAINT [FK_MonHocs_GiaoViens_MaGV]
        FOREIGN KEY ([MaGV]) REFERENCES [dbo].[GiaoViens]([MaGV]) ON DELETE SET NULL
);
GO
CREATE NONCLUSTERED INDEX [IX_MonHocs_MaGV] ON [dbo].[MonHocs] ([MaGV]);
GO

CREATE TABLE [dbo].[HocSinhs] (
    [MaHS]           nvarchar(20)  NOT NULL,
    [HoTen]          nvarchar(100) NOT NULL,
    [NgaySinh]       datetime2     NULL,
    [DiaChi]         nvarchar(255) NULL,
    [MaLop]          nvarchar(20)  NOT NULL,
    [Email_PhuHuynh] nvarchar(255) NULL,
    [SDT_PhuHuynh]   nvarchar(20)  NULL,
    [TrangThai]      nvarchar(20)  NOT NULL CONSTRAINT [DF_HocSinhs_TrangThai] DEFAULT (N''),
    CONSTRAINT [PK_HocSinhs] PRIMARY KEY CLUSTERED ([MaHS]),
    CONSTRAINT [FK_HocSinhs_LopHocs_MaLop]
        FOREIGN KEY ([MaLop]) REFERENCES [dbo].[LopHocs]([MaLop]) ON DELETE NO ACTION
);
GO
CREATE NONCLUSTERED INDEX [IX_HocSinhs_MaLop] ON [dbo].[HocSinhs] ([MaLop]);
GO

CREATE TABLE [dbo].[LichHocs] (
    [MaLich] int           IDENTITY(1,1) NOT NULL,
    [MaMon]  nvarchar(20)  NOT NULL,
    [MaLop]  nvarchar(20)  NOT NULL,
    [MaGV]   nvarchar(20)  NULL,
    [Thu]    tinyint       NULL,
    [TietBD] int           NULL,
    [TietKT] int           NULL,
    [Phong]  nvarchar(50)  NULL,
    CONSTRAINT [PK_LichHocs] PRIMARY KEY CLUSTERED ([MaLich]),
    CONSTRAINT [FK_LichHocs_GiaoViens_MaGV]
        FOREIGN KEY ([MaGV]) REFERENCES [dbo].[GiaoViens]([MaGV]) ON DELETE SET NULL,
    CONSTRAINT [FK_LichHocs_LopHocs_MaLop]
        FOREIGN KEY ([MaLop]) REFERENCES [dbo].[LopHocs]([MaLop]) ON DELETE CASCADE,
    CONSTRAINT [FK_LichHocs_MonHocs_MaMon]
        FOREIGN KEY ([MaMon]) REFERENCES [dbo].[MonHocs]([MaMon]) ON DELETE CASCADE
);
GO
CREATE NONCLUSTERED INDEX [IX_LichHocs_MaGV]  ON [dbo].[LichHocs] ([MaGV]);
CREATE NONCLUSTERED INDEX [IX_LichHocs_MaLop] ON [dbo].[LichHocs] ([MaLop]);
CREATE NONCLUSTERED INDEX [IX_LichHocs_MaMon] ON [dbo].[LichHocs] ([MaMon]);
GO

CREATE TABLE [dbo].[DiemSos] (
    [MaDiem]     int           IDENTITY(1,1) NOT NULL,
    [MaHS]       nvarchar(20)  NOT NULL,
    [MaMon]      nvarchar(20)  NOT NULL,
    [HocKy]      tinyint       NOT NULL,
    [NamHoc]     nvarchar(12)  NOT NULL CONSTRAINT [DF_DiemSos_NamHoc] DEFAULT (N'2025-2026'),
    [DiemMieng]  decimal(4,2)  NULL,
    [Diem15p]    decimal(4,2)  NULL,
    [DiemGiuaKy] decimal(4,2)  NULL,
    [DiemCuoiKy] decimal(4,2)  NULL,
    [DiemTBMon]  decimal(4,2)  NULL,
    CONSTRAINT [PK_DiemSos] PRIMARY KEY CLUSTERED ([MaDiem]),
    CONSTRAINT [FK_DiemSos_HocSinhs_MaHS]
        FOREIGN KEY ([MaHS]) REFERENCES [dbo].[HocSinhs]([MaHS]) ON DELETE CASCADE,
    CONSTRAINT [FK_DiemSos_MonHocs_MaMon]
        FOREIGN KEY ([MaMon]) REFERENCES [dbo].[MonHocs]([MaMon]) ON DELETE CASCADE
);
GO
CREATE NONCLUSTERED INDEX [IX_DiemSos_MaMon] ON [dbo].[DiemSos] ([MaMon]);
CREATE UNIQUE NONCLUSTERED INDEX [IX_DiemSos_MaHS_MaMon_NamHoc_HocKy]
    ON [dbo].[DiemSos] ([MaHS], [MaMon], [NamHoc], [HocKy]);
GO

CREATE TABLE [dbo].[DiemThanhPhans] (
    [Id]     int          IDENTITY(1,1) NOT NULL,
    [MaDiem] int          NOT NULL,
    [Loai]   tinyint      NOT NULL,
    [Diem]   decimal(4,2) NOT NULL,
    [ThuTu]  int          NOT NULL,
    CONSTRAINT [PK_DiemThanhPhans] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_DiemThanhPhans_DiemSos_MaDiem]
        FOREIGN KEY ([MaDiem]) REFERENCES [dbo].[DiemSos]([MaDiem]) ON DELETE CASCADE
);
GO
CREATE NONCLUSTERED INDEX [IX_DiemThanhPhans_MaDiem] ON [dbo].[DiemThanhPhans] ([MaDiem]);
GO

CREATE TABLE [dbo].[HocPhis] (
    [MaHocPhi]  int            IDENTITY(1,1) NOT NULL,
    [MaHS]      nvarchar(20)   NOT NULL,
    [HocKy]     tinyint        NOT NULL,
    [SoTien]    decimal(18,2)  NOT NULL,
    [NgayDong]  datetime2      NULL,
    [TrangThai] nvarchar(20)   NULL,
    CONSTRAINT [PK_HocPhis] PRIMARY KEY CLUSTERED ([MaHocPhi]),
    CONSTRAINT [FK_HocPhis_HocSinhs_MaHS]
        FOREIGN KEY ([MaHS]) REFERENCES [dbo].[HocSinhs]([MaHS]) ON DELETE CASCADE
);
GO
CREATE NONCLUSTERED INDEX [IX_HocPhis_MaHS] ON [dbo].[HocPhis] ([MaHS]);
GO

CREATE TABLE [dbo].[ThongBaos] (
    [MaTB]    int            IDENTITY(1,1) NOT NULL,
    [TieuDe]  nvarchar(200)  NULL,
    [NoiDung] nvarchar(max)  NULL,
    [NgayGui] datetime2      NOT NULL,
    [LoaiTB]  nvarchar(50)   NULL,
    [DaDoc]   bit            NOT NULL CONSTRAINT [DF_ThongBaos_DaDoc] DEFAULT (0),
    [NgayDoc] datetime2      NULL,
    [MaHS]    nvarchar(20)   NULL,
    CONSTRAINT [PK_ThongBaos] PRIMARY KEY CLUSTERED ([MaTB]),
    CONSTRAINT [FK_ThongBaos_HocSinhs_MaHS]
        FOREIGN KEY ([MaHS]) REFERENCES [dbo].[HocSinhs]([MaHS]) ON DELETE SET NULL
);
GO
CREATE NONCLUSTERED INDEX [IX_ThongBaos_MaHS_LoaiTB_DaDoc_NgayGui]
    ON [dbo].[ThongBaos] ([MaHS], [LoaiTB], [DaDoc], [NgayGui]);
GO

CREATE TABLE [dbo].[ParentStudentLinks] (
    [Id]     int           IDENTITY(1,1) NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [MaHS]   nvarchar(20)  NOT NULL,
    CONSTRAINT [PK_ParentStudentLinks] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_ParentStudentLinks_AspNetUsers_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ParentStudentLinks_HocSinhs_MaHS]
        FOREIGN KEY ([MaHS]) REFERENCES [dbo].[HocSinhs]([MaHS]) ON DELETE CASCADE
);
GO
CREATE NONCLUSTERED INDEX [IX_ParentStudentLinks_MaHS] ON [dbo].[ParentStudentLinks] ([MaHS]);
CREATE UNIQUE NONCLUSTERED INDEX [IX_ParentStudentLinks_UserId_MaHS]
    ON [dbo].[ParentStudentLinks] ([UserId], [MaHS]);
GO

CREATE TABLE [dbo].[KyHocWorkflowLogs] (
    [Id]                  bigint        IDENTITY(1,1) NOT NULL,
    [NamHoc]              nvarchar(12)  NOT NULL,
    [HocKy]               tinyint       NOT NULL,
    [FromStatus]          nvarchar(20)  NOT NULL,
    [ToStatus]            nvarchar(20)  NOT NULL,
    [PerformedByUserId]   nvarchar(450) NULL,
    [PerformedByUserName] nvarchar(256) NULL,
    [BienBan]             nvarchar(max) NULL,
    [AtUtc]               datetime2     NOT NULL,
    CONSTRAINT [PK_KyHocWorkflowLogs] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_KyHocWorkflowLogs_KyHocs_NamHoc_HocKy]
        FOREIGN KEY ([NamHoc], [HocKy]) REFERENCES [dbo].[KyHocs]([NamHoc], [HocKy]) ON DELETE CASCADE
);
GO
CREATE NONCLUSTERED INDEX [IX_KyHocWorkflowLogs_NamHoc_HocKy_AtUtc]
    ON [dbo].[KyHocWorkflowLogs] ([NamHoc], [HocKy], [AtUtc]);
GO

/* ============================================================================
   4. Đánh dấu các migration đã áp dụng để EF không chạy lại
   ============================================================================ */
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES
    (N'20260326131106_InitialCreate',                         N'8.0.14'),
    (N'20260402033408_AddTrangThaiToHocSinh',                 N'8.0.14'),
    (N'20260405021309_IdentityRbacDiemNamHoc',                N'8.0.14'),
    (N'20260414142215_AddThongBaoReadStateAndFilterIndex',    N'8.0.14'),
    (N'20260414150038_AddKyHocWorkflowAndAuditEnhancements',  N'8.0.14');
GO

PRINT N'>>> EduTrack database recreated successfully.';
PRINT N'>>> Run the API (dotnet run --project EduTrack.API) — DbSeeder will populate users + demo data.';
GO
