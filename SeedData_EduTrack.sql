USE QuanLyTruongHoc;
GO

-- XÓA DỮ LIỆU CŨ ĐỂ ÁP DỤNG DỮ LIỆU MẪU (KHÔNG DÙNG CHO PRODUCTION)
DELETE FROM ThongBao;
DELETE FROM DiemSo;
DELETE FROM HocSinh;
DELETE FROM MonHoc;
DELETE FROM LopHoc;
DELETE FROM GiaoVien;
GO

PRINT N'Bắt đầu Seed dữ liệu mẫu...'

-- 1. SEED GIÁO VIÊN
INSERT INTO GiaoVien (MaGV, HoTen, ChuyenMon, Email, LuongCoBan) VALUES 
('GV001', N'Nguyễn Đức Anh', N'Toán học', 'anhnd@edutrack.edu.vn', 15000000),
('GV002', N'Trần Thị Bích', N'Vật Lý', 'bichtd@edutrack.edu.vn', 14500000),
('GV003', N'Lê Hoàng Tuấn', N'Hóa học', 'tuanlh@edutrack.edu.vn', 13000000),
('GV004', N'Phạm Mỹ Uyên', N'Ngữ Văn', 'uyenpm@edutrack.edu.vn', 14000000),
('GV005', N'Hoàng Quốc Việt', N'Tiếng Anh', 'viethq@edutrack.edu.vn', 16000000);
GO

-- 2. SEED LỚP HỌC
INSERT INTO LopHoc (MaLop, TenLop, KhoiLop, NamHoc, MaGVChuNhiem) VALUES 
('10A1', N'10A1', '10', '2025-2026', 'GV001'),
('10A2', N'10A2', '10', '2025-2026', 'GV002'),
('11A1', N'11A1', '11', '2025-2026', 'GV003'),
('12A1', N'12A1', '12', '2025-2026', 'GV004');
GO

-- 3. SEED MÔN HỌC
INSERT INTO MonHoc (MaMon, TenMon, SoTiet, HeSoThi, MaGV) VALUES 
('TOAN', N'Toán học', 120, 2.0, 'GV001'),
('VL', N'Vật Lý', 90, 1.0, 'GV002'),
('HOA', N'Hóa học', 90, 1.0, 'GV003'),
('VAN', N'Ngữ Văn', 120, 2.0, 'GV004'),
('ANH', N'Tiếng Anh', 105, 1.0, 'GV005');
GO

-- 4. SEED HỌC SINH
INSERT INTO HocSinh (MaHS, HoTen, NgaySinh, DiaChi, MaLop, Email_PhuHuynh, SDT_PhuHuynh) VALUES 
('HS001', N'Lê Thị Hương', '2010-05-12', N'Hà Nội', '10A1', 'phuhuynh_hs1@gmail.com', '0988111222'),
('HS002', N'Nguyễn Bá Kiên', '2010-08-20', N'Hà Nội', '10A1', 'phuhuynh_hs2@gmail.com', '0988222333'),
('HS003', N'Vũ Đức Long', '2010-12-01', N'Hải Phòng', '10A2', 'phuhuynh_hs3@gmail.com', '0988333444'),
('HS004', N'Trần Mai Dung', '2009-02-15', N'Bắc Ninh', '11A1', 'phuhuynh_hs4@gmail.com', '0988444555'),
('HS005', N'Đỗ Thái Bảo', '2008-07-30', N'Hà Nội', '12A1', 'phuhuynh_hs5@gmail.com', '0988555666');
GO

-- 5. SEED ĐIỂM SỐ (KỲ 1)
-- Học sinh HS001 (Thiếu điểm -> Yếu)
INSERT INTO DiemSo (MaHS, MaMon, HocKy, NamHoc, DiemMieng, Diem15p, DiemGiuaKy, DiemCuoiKy) VALUES 
('HS001', 'TOAN', 1, '2025-2026', 6.0, 5.5, 4.0, NULL),
('HS001', 'VL', 1, '2025-2026', 5.0, 6.0, 5.5, NULL),
('HS001', 'VAN', 1, '2025-2026', 7.0, 6.5, 6.0, NULL);

-- Học sinh HS002 (Giỏi)
INSERT INTO DiemSo (MaHS, MaMon, HocKy, NamHoc, DiemMieng, Diem15p, DiemGiuaKy, DiemCuoiKy) VALUES 
('HS002', 'TOAN', 1, '2025-2026', 9.5, 9.0, 8.5, 9.0),
('HS002', 'VL', 1, '2025-2026', 8.0, 9.0, 9.5, 8.5),
('HS002', 'VAN', 1, '2025-2026', 8.5, 8.0, 8.0, 8.5);

-- Học sinh HS003 (Khá)
INSERT INTO DiemSo (MaHS, MaMon, HocKy, NamHoc, DiemMieng, Diem15p, DiemGiuaKy, DiemCuoiKy) VALUES 
('HS003', 'TOAN', 1, '2025-2026', 7.0, 6.5, 7.5, 6.5),
('HS003', 'VL', 1, '2025-2026', 7.5, 8.0, 7.0, 8.0);
GO

-- Cứu bảng điểm (Trigger backend tự tính DiemTBMon khi Fetch nếu null, hoặc Update thủ công)

-- 6. SEED THÔNG BÁO
INSERT INTO ThongBao (TieuDe, NoiDung, NgayGui, LoaiTB, MaHS, DaDoc) VALUES 
(N'Cảnh báo học tập K1', N'Học sinh Lê Thị Hương đang có dấu hiệu sa sút môn Toán.', GETDATE(), 'alert', 'HS001', 0),
(N'Tuyệt vời', N'Nguyễn Bá Kiên đạt điểm 9.0 Giữa Ký Môn Lý.', GETDATE(), 'grade', 'HS002', 0),
(N'Đóng học phí', N'Gia đình chú ý hoàn tất biên lai học phí tháng 11.', DATEADD(day, -2, GETDATE()), 'payment', 'HS003', 1),
(N'Nghỉ Lễ 2/9', N'Toàn trường nghỉ lễ Quốc Khánh trong 4 ngày.', DATEADD(day, -10, GETDATE()), 'schedule', NULL, 1),
(N'Thông báo xét tuyển', N'Các học sinh lớp 12 chuẩn bị hồ sơ xét tuyển.', DATEADD(day, -5, GETDATE()), 'enrollment', 'HS005', 0);
GO

PRINT N'Hoàn tất Seed dữ liệu!'
GO
