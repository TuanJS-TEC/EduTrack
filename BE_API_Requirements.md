# Tài liệu Yêu cầu Cập nhật Backend (EduTrack)

Đây là danh sách các thay đổi cần thực hiện phía API để đồng bộ với giao diện Người dùng (Frontend).

## 1. Lưu thay đổi - Trạng thái Học sinh (Persistence)

**Vấn đề:** Hiện tại sau khi Sửa và Lưu Học sinh, nếu Reload lại trang thì thông tin "Trạng thái" (Đang học / Đã nghỉ) bị mất. Các trường khác như Email/SĐT có thể cũng gặp vấn đề nếu không được Map đúng.

**Yêu cầu kỹ thuật:**
- **Model:** Thêm thuộc tính `public string? TrangThai { get; set; }` vào lớp `HocSinh`.
- **Database:** Thêm cột `TrangThai` (NVARCHAR(20)) vào bảng `HocSinh`.
- **Controller:** Cập nhật hàm `Update` trong `HocSinhController.cs` để gán giá trị mới:
  ```csharp
  // Cần thêm dòng này vào logic Update:
  existingHocSinh.TrangThai = input.TrangThai;
  ```

---

## 2. Tính toán Điểm TB và Hạnh kiểm Động

**Yêu cầu:** 
- Trong API `GET /api/hocsinh`, trả về 2 trường `DiemTB` và `HanhKiem` được tính toán realtime từ bảng `DiemSo`. Tránh để Frontend tự tính vì logic này nằm ở nghiệp vụ giáo dục.

---

## 3. Lớp học & Giáo viên (Breadcrumbs & Details)

- **Tên GV Chủ nhiệm:** API Lớp học cần kèm theo `TenGVChuNhiem` để Frontend hiển thị trên danh sách.
- **Tổng quát:** Thêm API `GET /api/dashboard/summary` báo cáo nhanh số lượng (Học sinh, Lớp học, Giáo viên).

---
*Vui lòng phản hồi lại danh sách này sau khi đã cập nhật API để Frontend đồng bộ.*
