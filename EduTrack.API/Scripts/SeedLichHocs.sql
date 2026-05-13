/* ============================================================================
   EduTrack — Seed LichHocs (Thời khóa biểu mẫu)
   Server : DESKTOP-22UAJFI
   DB     : EduTrack
   Yêu cầu: Các bảng GiaoViens / MonHocs / LopHocs đã có dữ liệu seed.

   Quy ước:
     Thu     : 2=T2, 3=T3, 4=T4, 5=T5, 6=T6, 7=T7
     TietBD..TietKT: 1-2 / 3-4 / 6-7 / 8-9 (1-5 sáng, 6-10 chiều)

   Thiết kế đảm bảo KHÔNG có xung đột thời gian giữa các GV
   (mỗi GV chỉ dạy 1 ca tại 1 thời điểm).
   ============================================================================ */

USE [EduTrack];
GO

DELETE FROM [dbo].[LichHocs];
DBCC CHECKIDENT ('[dbo].[LichHocs]', RESEED, 0) WITH NO_INFOMSGS;
GO

INSERT INTO [dbo].[LichHocs] ([MaMon], [MaLop], [MaGV], [Thu], [TietBD], [TietKT], [Phong])
VALUES
    -- ===== Lớp 10A1-2025 (phòng P-101) =====
    (N'SUB-MATH', N'10A1-2025', N'GV-00001', 2, 1, 2, N'P-101'),  -- T2 tiết 1-2 : Toán
    (N'SUB-LIT',  N'10A1-2025', N'GV-00002', 2, 3, 4, N'P-101'),  -- T2 tiết 3-4 : Văn
    (N'SUB-ENG',  N'10A1-2025', N'GV-00003', 4, 1, 2, N'P-101'),  -- T4 tiết 1-2 : Anh
    (N'SUB-PHY',  N'10A1-2025', N'GV-00004', 6, 3, 4, N'P-101'),  -- T6 tiết 3-4 : Lý

    -- ===== Lớp 10A2-2025 (phòng P-102) =====
    (N'SUB-MATH', N'10A2-2025', N'GV-00001', 2, 6, 7, N'P-102'),  -- T2 tiết 6-7 : Toán
    (N'SUB-LIT',  N'10A2-2025', N'GV-00002', 3, 1, 2, N'P-102'),  -- T3 tiết 1-2 : Văn
    (N'SUB-ENG',  N'10A2-2025', N'GV-00003', 4, 6, 7, N'P-102'),  -- T4 tiết 6-7 : Anh
    (N'SUB-PHY',  N'10A2-2025', N'GV-00004', 5, 3, 4, N'P-102'),  -- T5 tiết 3-4 : Lý

    -- ===== Lớp 11B1-2025 (phòng P-201) =====
    (N'SUB-MATH', N'11B1-2025', N'GV-00001', 3, 3, 4, N'P-201'),  -- T3 tiết 3-4 : Toán
    (N'SUB-LIT',  N'11B1-2025', N'GV-00002', 4, 3, 4, N'P-201'),  -- T4 tiết 3-4 : Văn
    (N'SUB-ENG',  N'11B1-2025', N'GV-00003', 5, 1, 2, N'P-201'),  -- T5 tiết 1-2 : Anh
    (N'SUB-PHY',  N'11B1-2025', N'GV-00004', 6, 1, 2, N'P-201'),  -- T6 tiết 1-2 : Lý

    -- ===== Lớp 12C1-2025 (phòng P-301) =====
    (N'SUB-MATH', N'12C1-2025', N'GV-00001', 3, 6, 7, N'P-301'),  -- T3 tiết 6-7 : Toán
    (N'SUB-LIT',  N'12C1-2025', N'GV-00002', 5, 6, 7, N'P-301'),  -- T5 tiết 6-7 : Văn
    (N'SUB-ENG',  N'12C1-2025', N'GV-00003', 6, 6, 7, N'P-301'),  -- T6 tiết 6-7 : Anh
    (N'SUB-PHY',  N'12C1-2025', N'GV-00004', 7, 1, 2, N'P-301'); -- T7 tiết 1-2 : Lý
GO

PRINT N'>>> Đã seed 16 dòng vào LichHocs (4 lớp × 4 môn, không xung đột GV).';
GO

-- ============================================================================
-- Kiểm tra nhanh
-- ============================================================================
SELECT  lh.MaLich,
        lh.MaLop,
        m.TenMon,
        gv.HoTen        AS GiaoVien,
        CASE lh.Thu
            WHEN 2 THEN N'Thứ 2'
            WHEN 3 THEN N'Thứ 3'
            WHEN 4 THEN N'Thứ 4'
            WHEN 5 THEN N'Thứ 5'
            WHEN 6 THEN N'Thứ 6'
            WHEN 7 THEN N'Thứ 7'
        END             AS Thu,
        CONCAT(N'Tiết ', lh.TietBD, N'-', lh.TietKT) AS Tiet,
        lh.Phong
FROM    [dbo].[LichHocs] lh
JOIN    [dbo].[MonHocs]  m  ON m.MaMon = lh.MaMon
JOIN    [dbo].[GiaoViens] gv ON gv.MaGV = lh.MaGV
ORDER BY lh.MaLop, lh.Thu, lh.TietBD;
GO
