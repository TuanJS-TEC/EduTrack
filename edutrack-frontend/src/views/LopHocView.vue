<script setup>
import { ref, computed, onMounted } from 'vue'
import { Plus, Search, MoreHorizontal, Users, UserSquare, Calendar, Filter, RefreshCcw, X, ChevronRight, Award, TrendingUp, Book } from 'lucide-vue-next'
import { apiService } from '../services/api'

const search = ref('')
const gradeFilter = ref('All')

// // Mock Summary Data
// const summaryCards = computed(() => [
//   { id: 1, title: 'Tổng số lớp học', value: classesList.value.length.toString(), subtitle: 'Toàn trường', icon: Users, color: 'text-blue-500 bg-blue-50 dark:text-blue-400 dark:bg-blue-500/10' },
//   { id: 2, title: 'Năm học hiện tại', value: '2025-2026', subtitle: 'Học kỳ 1', icon: Calendar, color: 'text-purple-500 bg-purple-50 dark:text-purple-400 dark:bg-purple-500/10' },
// ])

// // Mock data
// const mockClasses = [
//   { MaLop: '10A', TenLop: 'Lớp 10A - Khối chuyên Toán', KhoiLop: 10, NamHoc: '2025-2026', MaGVChuNhiem: 'GV001' },
//   { MaLop: '10B', TenLop: 'Lớp 10B - Khối chuyên Lý', KhoiLop: 10, NamHoc: '2025-2026', MaGVChuNhiem: 'GV002' },
//   { MaLop: '10C', TenLop: 'Lớp 10C - Khối chuyên Hóa', KhoiLop: 10, NamHoc: '2025-2026', MaGVChuNhiem: 'GV003' },
//   { MaLop: '11A', TenLop: 'Lớp 11A - Khối chuyên Anh', KhoiLop: 11, NamHoc: '2025-2026', MaGVChuNhiem: 'GV004' },
//   { MaLop: '11B', TenLop: 'Lớp 11B - Khối chuyên Văn', KhoiLop: 11, NamHoc: '2025-2026', MaGVChuNhiem: 'GV005' },
//   { MaLop: '12A', TenLop: 'Lớp 12A - Đại trà', KhoiLop: 12, NamHoc: '2025-2026', MaGVChuNhiem: 'GV006' },
//   { MaLop: '12B', TenLop: 'Lớp 12B - Đại trà', KhoiLop: 12, NamHoc: '2025-2026', MaGVChuNhiem: 'GV007' },
//   { MaLop: '9A', TenLop: 'Lớp 9A - Chuẩn bị THPT', KhoiLop: 9, NamHoc: '2025-2026', MaGVChuNhiem: 'GV008' },
// ]

// const mockTeachers = {
//   GV001: { MaGV: 'GV001', HoTen: 'Nguyễn Văn A', ChuyenMon: 'Toán học', Email_PhuHuynh: 'nguyenvana@school.edu.vn', SDT_PhuHuynh: '0912345678' },
//   GV002: { MaGV: 'GV002', HoTen: 'Trần Thị B', ChuyenMon: 'Vật Lý', Email_PhuHuynh: 'tranthib@school.edu.vn', SDT_PhuHuynh: '0987654321' },
//   GV003: { MaGV: 'GV003', HoTen: 'Lê Văn C', ChuyenMon: 'Hóa học', Email_PhuHuynh: 'levanc@school.edu.vn', SDT_PhuHuynh: '0923456789' },
//   GV004: { MaGV: 'GV004', HoTen: 'Phạm Thị D', ChuyenMon: 'Tiếng Anh', Email_PhuHuynh: 'phamthid@school.edu.vn', SDT_PhuHuynh: '0934567890' },
//   GV005: { MaGV: 'GV005', HoTen: 'Đỗ Văn E', ChuyenMon: 'Ngữ Văn', Email_PhuHuynh: 'dovane@school.edu.vn', SDT_PhuHuynh: '0945678901' },
//   GV006: { MaGV: 'GV006', HoTen: 'Vũ Thị F', ChuyenMon: 'Lịch Sử', Email_PhuHuynh: 'vuthif@school.edu.vn', SDT_PhuHuynh: '0956789012' },
//   GV007: { MaGV: 'GV007', HoTen: 'Hoàng Văn G', ChuyenMon: 'Địa Lý', Email_PhuHuynh: 'hoangvang@school.edu.vn', SDT_PhuHuynh: '0967890123' },
//   GV008: { MaGV: 'GV008', HoTen: 'Đinh Thị H', ChuyenMon: 'Sinh Học', Email_PhuHuynh: 'dinhthih@school.edu.vn', SDT_PhuHuynh: '0978901234' },
// }

// const mockStudentsData = {
//   '10A': [
//     { MaHS: 'HS001', HoTen: 'Trần Minh Quân', NgaySinh: '2008-05-15', DiaChi: '123 Nguyễn Huệ, TP.HCM', MaLop: '10A', Email_PhuHuynh: 'phuhuynh1@email.com', SDT_PhuHuynh: '0911111111' },
//     { MaHS: 'HS002', HoTen: 'Phạm Ngọc Linh', NgaySinh: '2008-07-22', DiaChi: '456 Lê Lợi, TP.HCM', MaLop: '10A', Email_PhuHuynh: 'phuhuynh2@email.com', SDT_PhuHuynh: '0922222222' },
//     { MaHS: 'HS003', HoTen: 'Lê Đức Hưng', NgaySinh: '2008-03-08', DiaChi: '789 Trần Hưng Đạo, TP.HCM', MaLop: '10A', Email_PhuHuynh: 'phuhuynh3@email.com', SDT_PhuHuynh: '0933333333' },
//     { MaHS: 'HS004', HoTen: 'Vương Thu Hà', NgaySinh: '2008-11-30', DiaChi: '321 Võ Văn Kiệt, TP.HCM', MaLop: '10A', Email_PhuHuynh: 'phuhuynh4@email.com', SDT_PhuHuynh: '0944444444' },
//     { MaHS: 'HS005', HoTen: 'Phan Huy Hoàng', NgaySinh: '2008-09-18', DiaChi: '654 Cách Mạng Tháng 8, TP.HCM', MaLop: '10A', Email_PhuHuynh: 'phuhuynh5@email.com', SDT_PhuHuynh: '0955555555' },
//   ],
//   '10B': [
//     { MaHS: 'HS006', HoTen: 'Nguyễn Thị Hoa', NgaySinh: '2008-04-12', DiaChi: '111 Nguyễn Văn Linh, TP.HCM', MaLop: '10B', Email_PhuHuynh: 'phuhuynh6@email.com', SDT_PhuHuynh: '0966666666' },
//     { MaHS: 'HS007', HoTen: 'Đặng Hoàng Anh', NgaySinh: '2008-06-28', DiaChi: '222 Lý Thường Kiệt, TP.HCM', MaLop: '10B', Email_PhuHuynh: 'phuhuynh7@email.com', SDT_PhuHuynh: '0977777777' },
//     { MaHS: 'HS008', HoTen: 'Tạ Minh Dung', NgaySinh: '2008-02-14', DiaChi: '333 Bạch Đằng, TP.HCM', MaLop: '10B', Email_PhuHuynh: 'phuhuynh8@email.com', SDT_PhuHuynh: '0988888888' },
//     { MaHS: 'HS009', HoTen: 'Bùi Thị Kim Chi', NgaySinh: '2008-10-05', DiaChi: '444 Tôn Đức Thắng, TP.HCM', MaLop: '10B', Email_PhuHuynh: 'phuhuynh9@email.com', SDT_PhuHuynh: '0999999999' },
//   ],
//   '10C': [
//     { MaHS: 'HS010', HoTen: 'Vũ Hà Anh', NgaySinh: '2008-08-19', DiaChi: '555 Gia Long, TP.HCM', MaLop: '10C', Email_PhuHuynh: 'phuhuynh10@email.com', SDT_PhuHuynh: '0911111112' },
//     { MaHS: 'HS011', HoTen: 'Cao Thị May', NgaySinh: '2008-01-25', DiaChi: '666 Hàm Nghi, TP.HCM', MaLop: '10C', Email_PhuHuynh: 'phuhuynh11@email.com', SDT_PhuHuynh: '0922222223' },
//     { MaHS: 'HS012', HoTen: 'Trương Đức Thành', NgaySinh: '2008-12-10', DiaChi: '777 Phan Bội Châu, TP.HCM', MaLop: '10C', Email_PhuHuynh: 'phuhuynh12@email.com', SDT_PhuHuynh: '0933333334' },
//     { MaHS: 'HS013', HoTen: 'Hoàng Thị Vân', NgaySinh: '2008-05-03', DiaChi: '888 Ngô Quyền, TP.HCM', MaLop: '10C', Email_PhuHuynh: 'phuhuynh13@email.com', SDT_PhuHuynh: '0944444445' },
//     { MaHS: 'HS014', HoTen: 'Trần Toàn Hùng', NgaySinh: '2008-09-09', DiaChi: '999 Nguyễn Thái Học, TP.HCM', MaLop: '10C', Email_PhuHuynh: 'phuhuynh14@email.com', SDT_PhuHuynh: '0955555556' },
//     { MaHS: 'HS015', HoTen: 'Lương Thị Bích Phương', NgaySinh: '2008-03-21', DiaChi: '1010 Phan Phú Tiên, TP.HCM', MaLop: '10C', Email_PhuHuynh: 'phuhuynh15@email.com', SDT_PhuHuynh: '0966666667' },
//   ],
//   '12B': [
//     { MaHS: 'HS016', HoTen: 'Trần Thị Quỳnh Như', NgaySinh: '2006-07-14', DiaChi: '123 Võ Thị Sáu, TP.HCM', MaLop: '12B', Email_PhuHuynh: 'phuhuynh16@email.com', SDT_PhuHuynh: '0988877766' },
//   ],
// }

// // Mock Grades Data (Điểm học sinh) - Organized by HocKy
// const mockGradesData = {
//   'HS001': {
//     1: { // HocKy 1
//       'MON001': { DiemMieng: 8, Diem15p: 7.5, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 7, Diem15p: 7, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.6, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 9, Diem15p: 8.5, DiemGiuaKy: 9, DiemCuoiKy: 9, DiemTBMon: 8.9, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 8.5, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.3, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 7.5, Diem15p: 7, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.6, TenMon: 'Ngữ Văn' },
//     },
//     2: { // HocKy 2
//       'MON001': { DiemMieng: 8.5, Diem15p: 8, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.6, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.0, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 9, Diem15p: 8.5, DiemGiuaKy: 9, DiemCuoiKy: 9.5, DiemTBMon: 9.1, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 8.5, Diem15p: 8.5, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.6, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 8, Diem15p: 7.5, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Ngữ Văn' },
//     }
//   },
//   'HS002': {
//     1: {
//       'MON001': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7, DiemCuoiKy: 8, DiemTBMon: 7.6, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 8, Diem15p: 8.5, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.6, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 9, Diem15p: 9, DiemGiuaKy: 9, DiemCuoiKy: 9, DiemTBMon: 9, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Ngữ Văn' },
//     },
//     2: {
//       'MON001': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.2, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 8.5, Diem15p: 8.5, DiemGiuaKy: 9, DiemCuoiKy: 9, DiemTBMon: 8.9, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 8.5, Diem15p: 8, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.6, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 9, Diem15p: 9, DiemGiuaKy: 9, DiemCuoiKy: 9.5, DiemTBMon: 9.2, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 8.5, Diem15p: 8.5, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.7, TenMon: 'Ngữ Văn' },
//     }
//   },
//   'HS003': {
//     1: {
//       'MON001': { DiemMieng: 9, Diem15p: 8.5, DiemGiuaKy: 9, DiemCuoiKy: 9.5, DiemTBMon: 9.1, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 8.5, Diem15p: 8, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.6, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 8, Diem15p: 8.5, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.3, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 7.5, Diem15p: 7, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.6, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 8.5, Diem15p: 8, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.6, TenMon: 'Ngữ Văn' },
//     },
//     2: { // HocKy 2
//       'MON001': { DiemMieng: 9, Diem15p: 9, DiemGiuaKy: 9, DiemCuoiKy: 9.5, DiemTBMon: 9.1, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 8, Diem15p: 8.5, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.6, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 8.5, Diem15p: 8, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.7, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 9, Diem15p: 8.5, DiemGiuaKy: 9, DiemCuoiKy: 9, DiemTBMon: 8.9, TenMon: 'Ngữ Văn' },
//     }
//   },
//   'HS004': {
//     1: {
//       'MON001': { DiemMieng: 6.5, Diem15p: 6, DiemGiuaKy: 6.5, DiemCuoiKy: 7, DiemTBMon: 6.6, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 7, Diem15p: 6, DiemGiuaKy: 6.5, DiemCuoiKy: 7, DiemTBMon: 6.6, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 5, Diem15p: 5.5, DiemGiuaKy: 5, DiemCuoiKy: 5.5, DiemTBMon: 5.2, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 8, Diem15p: 7.5, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 7, Diem15p: 7, DiemGiuaKy: 7, DiemCuoiKy: 7, DiemTBMon: 7, TenMon: 'Ngữ Văn' },
//     },
//     2: {
//       'MON001': { DiemMieng: 7, Diem15p: 6.5, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.2, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 7.5, Diem15p: 6.5, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.3, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 5.5, Diem15p: 6, DiemGiuaKy: 5.5, DiemCuoiKy: 6, DiemTBMon: 5.8, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 8.5, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.2, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 7.5, Diem15p: 7, DiemGiuaKy: 7.5, DiemCuoiKy: 7.5, DiemTBMon: 7.4, TenMon: 'Ngữ Văn' },
//     }
//   },
//   'HS005': {
//     1: {
//       'MON001': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8, DiemTBMon: 8, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 8.5, Diem15p: 8, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.6, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 7, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Ngữ Văn' },
//     },
//     2: {
//       'MON001': { DiemMieng: 8.5, Diem15p: 8.5, DiemGiuaKy: 8.5, DiemCuoiKy: 8.5, DiemTBMon: 8.5, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 8.5, Diem15p: 8.5, DiemGiuaKy: 9, DiemCuoiKy: 9, DiemTBMon: 8.8, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 8.5, Diem15p: 8.5, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.6, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 7.5, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Ngữ Văn' },
//     }
//   },
//   // HS006 - 10B
//   'HS006': {
//     1: {
//       'MON001': { DiemMieng: 7.5, Diem15p: 7, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.6, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 8.5, Diem15p: 8, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.6, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 7, Diem15p: 7, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.1, TenMon: 'Ngữ Văn' },
//     },
//     2: {
//       'MON001': { DiemMieng: 8, Diem15p: 7.5, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 8.5, Diem15p: 8.5, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.7, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 8.5, Diem15p: 8.5, DiemGiuaKy: 9, DiemCuoiKy: 9, DiemTBMon: 8.8, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Ngữ Văn' },
//     }
//   },
//   'HS007': {
//     1: {
//       'MON001': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 6.5, Diem15p: 6.5, DiemGiuaKy: 6.5, DiemCuoiKy: 7, DiemTBMon: 6.7, TenMon: 'Ngữ Văn' },
//     },
//     2: {
//       'MON001': { DiemMieng: 8.5, Diem15p: 8, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.6, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 8.5, Diem15p: 8.5, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.7, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 8, Diem15p: 8.5, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.2, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 7, Diem15p: 7, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.2, TenMon: 'Ngữ Văn' },
//     }
//   },
//   'HS008': {
//     1: {
//       'MON001': { DiemMieng: 6.5, Diem15p: 6.5, DiemGiuaKy: 6.5, DiemCuoiKy: 7, DiemTBMon: 6.7, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 6, Diem15p: 6, DiemGiuaKy: 6, DiemCuoiKy: 6.5, DiemTBMon: 6.1, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 6.5, Diem15p: 6.5, DiemGiuaKy: 6.5, DiemCuoiKy: 7, DiemTBMon: 6.7, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 7, Diem15p: 6.5, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.1, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 5.5, Diem15p: 5.5, DiemGiuaKy: 5.5, DiemCuoiKy: 6, DiemTBMon: 5.7, TenMon: 'Ngữ Văn' },
//     },
//     2: {
//       'MON001': { DiemMieng: 7, Diem15p: 6.5, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.2, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 6.5, Diem15p: 6.5, DiemGiuaKy: 6.5, DiemCuoiKy: 7, DiemTBMon: 6.7, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 7, Diem15p: 7, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.2, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 7.5, Diem15p: 7, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.5, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 6, Diem15p: 6, DiemGiuaKy: 6, DiemCuoiKy: 6.5, DiemTBMon: 6.2, TenMon: 'Ngữ Văn' },
//     }
//   },
//   'HS009': {
//     1: {
//       'MON001': { DiemMieng: 7, Diem15p: 7, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.1, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 7.5, Diem15p: 7, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.6, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Ngữ Văn' },
//     },
//     2: {
//       'MON001': { DiemMieng: 7.5, Diem15p: 7, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.6, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 8.5, Diem15p: 8.5, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.6, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Ngữ Văn' },
//     }
//   },
//   'HS010': {
//     1: {
//       'MON001': { DiemMieng: 7, Diem15p: 7, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.1, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 6.5, Diem15p: 6.5, DiemGiuaKy: 6.5, DiemCuoiKy: 7, DiemTBMon: 6.7, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 7, Diem15p: 7, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.1, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 6.5, Diem15p: 6.5, DiemGiuaKy: 6.5, DiemCuoiKy: 7, DiemTBMon: 6.7, TenMon: 'Ngữ Văn' },
//     },
//     2: {
//       'MON001': { DiemMieng: 7.5, Diem15p: 7, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.6, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 7, Diem15p: 7, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.1, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 7, Diem15p: 7, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.1, TenMon: 'Ngữ Văn' },
//     }
//   },
//   'HS011': {
//     1: {
//       'MON001': { DiemMieng: 6.5, Diem15p: 6.5, DiemGiuaKy: 6.5, DiemCuoiKy: 7, DiemTBMon: 6.7, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 6, Diem15p: 6, DiemGiuaKy: 6, DiemCuoiKy: 6.5, DiemTBMon: 6.1, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 6.5, Diem15p: 6.5, DiemGiuaKy: 6.5, DiemCuoiKy: 7, DiemTBMon: 6.7, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 6.5, Diem15p: 6.5, DiemGiuaKy: 6.5, DiemCuoiKy: 7, DiemTBMon: 6.7, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 5.5, Diem15p: 5.5, DiemGiuaKy: 5.5, DiemCuoiKy: 6, DiemTBMon: 5.7, TenMon: 'Ngữ Văn' },
//     },
//     2: {
//       'MON001': { DiemMieng: 7, Diem15p: 6.5, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.2, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 6.5, Diem15p: 6.5, DiemGiuaKy: 6.5, DiemCuoiKy: 7, DiemTBMon: 6.7, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 7, Diem15p: 7, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.2, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 7, Diem15p: 7, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.2, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 6, Diem15p: 6, DiemGiuaKy: 6, DiemCuoiKy: 6.5, DiemTBMon: 6.2, TenMon: 'Ngữ Văn' },
//     }
//   },
//   'HS012': {
//     1: {
//       'MON001': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 8.5, Diem15p: 8.5, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.7, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Ngữ Văn' },
//     },
//     2: {
//       'MON001': { DiemMieng: 8.5, Diem15p: 8, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.6, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 8.5, Diem15p: 8.5, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.7, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 9, Diem15p: 9, DiemGiuaKy: 9, điemCuoiKy: 9.5, DiemTBMon: 9.2, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Ngữ Văn' },
//     }
//   },
//   'HS013': {
//     1: {
//       'MON001': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 7, Diem15p: 7, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.1, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 7, Diem15p: 7, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.1, TenMon: 'Ngữ Văn' },
//     },
//     2: {
//       'MON001': { DiemMieng: 8, Diem15p: 7.5, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Ngữ Văn' },
//     }
//   },
//   'HS014': {
//     1: {
//       'MON001': { DiemMieng: 6.5, Diem15p: 6.5, DiemGiuaKy: 6.5, DiemCuoiKy: 7, DiemTBMon: 6.7, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 6.5, Diem15p: 6.5, DiemGiuaKy: 6.5, DiemCuoiKy: 7, DiemTBMon: 6.7, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 5.5, Diem15p: 5.5, DiemGiuaKy: 5.5, DiemCuoiKy: 6, DiemTBMon: 5.7, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 6, Diem15p: 6, DiemGiuaKy: 6, DiemCuoiKy: 6.5, DiemTBMon: 6.2, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 5.5, Diem15p: 5.5, DiemGiuaKy: 5.5, DiemCuoiKy: 6, DiemTBMon: 5.7, TenMon: 'Ngữ Văn' },
//     },
//     2: {
//       'MON001': { DiemMieng: 7, Diem15p: 6.5, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.2, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 7, Diem15p: 6.5, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.2, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 6, Diem15p: 6, DiemGiuaKy: 6, DiemCuoiKy: 6.5, DiemTBMon: 6.2, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 6.5, Diem15p: 6.5, DiemGiuaKy: 6.5, DiemCuoiKy: 7, DiemTBMon: 6.7, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 6, Diem15p: 6, DiemGiuaKy: 6, DiemCuoiKy: 6.5, DiemTBMon: 6.2, TenMon: 'Ngữ Văn' },
//     }
//   },
//   'HS015': {
//     1: {
//       'MON001': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 8.5, Diem15p: 8, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.6, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Ngữ Văn' },
//     },
//     2: {
//       'MON001': { DiemMieng: 8.5, Diem15p: 8, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.6, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 8.5, Diem15p: 8, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.6, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 9, Diem15p: 9, DiemGiuaKy: 9, DiemCuoiKy: 9, DiemTBMon: 9, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 8.5, Diem15p: 8.5, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.7, TenMon: 'Ngữ Văn' },
//     }
//   },
//   'HS016': {
//     '2025-HK1': {
//       'MON001': { DiemMieng: 8.5, Diem15p: 8, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.6, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 8, Diem15p: 8.5, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.3, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 8.5, Diem15p: 8, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.7, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 9, Diem15p: 9, DiemGiuaKy: 9, DiemCuoiKy: 9, DiemTBMon: 9, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 8, Diem15p: 8.5, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.3, TenMon: 'Ngữ Văn' },
//       'MON018': { DiemMieng: 8, Diem15p: 7.5, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Lịch Sử' },
//       'MON019': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Địa Lý' },
//     },
//     '2025-HK2': {
//       'MON001': { DiemMieng: 9, Diem15p: 8.5, DiemGiuaKy: 9, DiemCuoiKy: 9.5, DiemTBMon: 9.1, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 8.5, Diem15p: 8.5, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.7, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 8.5, Diem15p: 8.5, DiemGiuaKy: 9, DiemCuoiKy: 9, DiemTBMon: 8.9, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 9, Diem15p: 9, DiemGiuaKy: 9.5, DiemCuoiKy: 9.5, DiemTBMon: 9.3, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 8.5, Diem15p: 8.5, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.7, TenMon: 'Ngữ Văn' },
//       'MON018': { DiemMieng: 8.5, Diem15p: 8, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.7, TenMon: 'Lịch Sử' },
//       'MON019': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Địa Lý' },
//     },
//     '2024-HK2': {
//       'MON001': { DiemMieng: 8, Diem15p: 7.5, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, TenMon: 'Ngữ Văn' },
//       'MON018': { DiemMieng: 7, Diem15p: 7, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.1, TenMon: 'Lịch Sử' },
//       'MON019': { DiemMieng: 7, Diem15p: 7, DiemGiuaKy: 7, DiemCuoiKy: 7, DiemTBMon: 7, TenMon: 'Địa Lý' },
//     },
//     '2024-HK1': {
//       'MON001': { DiemMieng: 7.5, Diem15p: 7, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.6, TenMon: 'Toán' },
//       'MON002': { DiemMieng: 7, Diem15p: 7, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.1, TenMon: 'Vật Lý' },
//       'MON003': { DiemMieng: 7, Diem15p: 7, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.1, TenMon: 'Hóa Học' },
//       'MON004': { DiemMieng: 8, Diem15p: 7.5, DiemGiuaKy: 8, DiemCuoiKy: 8, DiemTBMon: 7.9, TenMon: 'Tiếng Anh' },
//       'MON005': { DiemMieng: 7, Diem15p: 7, DiemGiuaKy: 7, DiemCuoiKy: 7.5, DiemTBMon: 7.1, TenMon: 'Ngữ Văn' },
//       'MON018': { DiemMieng: 6.5, Diem15p: 6.5, DiemGiuaKy: 6.5, DiemCuoiKy: 7, DiemTBMon: 6.6, TenMon: 'Lịch Sử' },
//       'MON019': { DiemMieng: 6.5, Diem15p: 6.5, DiemGiuaKy: 6.5, DiemCuoiKy: 7, DiemTBMon: 6.6, TenMon: 'Địa Lý' },
//     },
//   },
// }

const classesList = ref([])
const loading = ref(false)

// Selected class details
const selectedClass = ref(null)
const classTeacher = ref(null)
const classStudents = ref([])
const loadingDetails = ref(false)

// Selected student for grades
const selectedStudent = ref(null)
const studentGrades = ref([])
const selectedHocKy = ref(1)
const availableTerms = ref([])
const termsGroupedByYear = ref({})

// Class statistics
const classStats = ref({ excellent: 0, good: 0, average: 0, weak: 0 })

const fetchClasses = async () => {
  loading.value = true
  try {
    // Try to fetch from API, fallback to mock data
    try {
      const res = await apiService.getLopHocs()
      classesList.value = res.data
    } catch (apiError) {
      console.warn("API call failed, using mock data:", apiError.message)
      classesList.value = mockClasses
    }
  } catch (error) {
    console.error("Lỗi khi tải dữ liệu Lớp học:", error)
    classesList.value = mockClasses // Fallback
  } finally {
    loading.value = false
  }
}

// Calculate average GPA for a student across all semesters in a year
const getStudentYearAverage = (studentID, year) => {
  const studentData = mockGradesData[studentID]
  if (!studentData) return 0
  
  const hk1Key = `${year}-HK1`
  const hk2Key = `${year}-HK2`
  const allGrades = []
  
  if (studentData[hk1Key]) {
    Object.values(studentData[hk1Key]).forEach(grade => {
      allGrades.push(grade.DiemTBMon)
    })
  }
  if (studentData[hk2Key]) {
    Object.values(studentData[hk2Key]).forEach(grade => {
      allGrades.push(grade.DiemTBMon)
    })
  }
  
  if (allGrades.length === 0) return 0
  return (allGrades.reduce((a, b) => a + b, 0) / allGrades.length).toFixed(2)
}

// Group available terms by school year
const groupTermsByYear = (termKeys) => {
  const grouped = {}
  
  termKeys.forEach(term => {
    if (term.includes('-')) {
      const [year, semester] = term.split('-')
      if (!grouped[year]) {
        grouped[year] = []
      }
      grouped[year].push(term)
    } else {
      // Old format - put in a default year
      if (!grouped['2025']) {
        grouped['2025'] = []
      }
      grouped['2025'].push(parseInt(term))
    }
  })
  
  // Sort both years and their semesters
  return Object.keys(grouped).sort((a, b) => b.localeCompare(a)).reduce((acc, year) => {
    acc[year] = grouped[year].sort((a, b) => {
      if (typeof a === 'string' && typeof b === 'string') {
        return b.localeCompare(a)
      }
      return b - a
    })
    return acc
  }, {})
}

// Calculate class statistics by grade level
const calculateClassStats = async (students) => {
  let excellent = 0, good = 0, average = 0, weak = 0
  
  students.forEach(student => {
    // Calculate average GPA from the latest available data
    const studentData = mockGradesData[student.MaHS]
    
    const allGrades = []
    if (studentData) {
      Object.values(studentData).forEach(semester => {
        if (typeof semester === 'object') {
          Object.values(semester).forEach(grade => {
            if (grade.DiemTBMon) {
              allGrades.push(grade.DiemTBMon)
            }
          })
        }
      })
    }
    
    // Calculate average or default to 0 if no grades
    let avgGPA = 0
    if (allGrades.length > 0) {
      avgGPA = parseFloat((allGrades.reduce((a, b) => a + b, 0) / allGrades.length).toFixed(2))
    }
    
    // Count all students including those with no data
    if (avgGPA >= 8) excellent++
    else if (avgGPA >= 7) good++
    else if (avgGPA >= 6) average++
    else weak++
  })
  
  classStats.value = { excellent, good, average, weak }
}

const selectClass = async (cls) => {
  selectedClass.value = cls
  selectedStudent.value = null
  studentGrades.value = []
  selectedHocKy.value = null
  availableTerms.value = []
  loadingDetails.value = true
  classTeacher.value = null
  classStudents.value = []
  
  try {
    // Fetch students of the class
    try {
      const studentsRes = await apiService.getHocSinhs(cls.MaLop)
      classStudents.value = studentsRes.data || []
    } catch (e) {
      console.warn("Students API failed, using mock data")
      classStudents.value = mockStudentsData[cls.MaLop] || []
    }
    
    // Calculate class statistics
    await calculateClassStats(classStudents.value)
    
    // Fetch teacher info if exists
    if (cls.MaGVChuNhiem) {
      try {
        const teacherRes = await apiService.getGiaoVienById(cls.MaGVChuNhiem)
        classTeacher.value = teacherRes.data
      } catch (e) {
        console.warn("Teacher API failed, using mock data")
        classTeacher.value = mockTeachers[cls.MaGVChuNhiem] || null
      }
    }
  } catch (error) {
    console.error("Lỗi khi tải chi tiết lớp học:", error)
  } finally {
    loadingDetails.value = false
  }
}

const closeDetails = () => {
  selectedClass.value = null
  classTeacher.value = null
  classStudents.value = []
  selectedStudent.value = null
  studentGrades.value = []
}

const viewStudentGrades = (student) => {
  selectedStudent.value = student
  // Get available terms for this student
  const studentData = mockGradesData[student.MaHS] || {}
  const termKeys = Object.keys(studentData)
  
  // Group terms by school year
  termsGroupedByYear.value = groupTermsByYear(termKeys)
  const flatTerms = Object.values(termsGroupedByYear.value).flat()
  
  availableTerms.value = flatTerms
  
  // Select the latest term by default
  if (availableTerms.value.length > 0) {
    selectedHocKy.value = availableTerms.value[0]
    updateStudentGrades()
  }
}

const updateStudentGrades = () => {
  if (!selectedStudent.value || !selectedHocKy.value) return
  
  const studentData = mockGradesData[selectedStudent.value.MaHS]
  if (!studentData) {
    studentGrades.value = []
    return
  }
  
  // Check if it's a year average request (format: "2025-YEAR" or similar)
  if (String(selectedHocKy.value).includes('-YEAR')) {
    const year = String(selectedHocKy.value).split('-')[0]
    
    // Try to find data with both year-based keys and numeric keys
    let hk1Data = studentData[`${year}-HK1`] || {}
    let hk2Data = studentData[`${year}-HK2`] || {}
    
    // If year-based keys don't exist, try numeric keys (1, 2) for older format
    if (Object.keys(hk1Data).length === 0 && Object.keys(hk2Data).length === 0) {
      hk1Data = studentData[1] || {}
      hk2Data = studentData[2] || {}
    }
    
    // Create weighted grades for full year: (HK1 + 2*HK2) / 3
    const allSubjects = {}
    
    // Collect all subjects from both semesters
    Object.keys(hk1Data).forEach(subjectKey => {
      allSubjects[subjectKey] = { hk1: hk1Data[subjectKey], hk2: null }
    })
    Object.keys(hk2Data).forEach(subjectKey => {
      if (allSubjects[subjectKey]) {
        allSubjects[subjectKey].hk2 = hk2Data[subjectKey]
      } else {
        allSubjects[subjectKey] = { hk1: null, hk2: hk2Data[subjectKey] }
      }
    })
    
    // Calculate weighted averages per subject
    const weightedGrades = Object.values(allSubjects).map(({ hk1, hk2 }) => {
      const hk1Avg = hk1 ? hk1.DiemTBMon : 0
      const hk2Avg = hk2 ? hk2.DiemTBMon : 0
      const tenMon = (hk2 || hk1)?.TenMon || 'Chưa xác định'
      
      // Formula: (HK1 + 2*HK2) / 3
      const weightedAvg = (hk1Avg + 2 * hk2Avg) / 3
      
      return {
        TenMon: tenMon,
        DiemMieng: hk2 ? hk2.DiemMieng : (hk1 ? hk1.DiemMieng : 0),
        Diem15p: hk2 ? hk2.Diem15p : (hk1 ? hk1.Diem15p : 0),
        DiemGiuaKy: hk2 ? hk2.DiemGiuaKy : (hk1 ? hk1.DiemGiuaKy : 0),
        DiemCuoiKy: hk2 ? hk2.DiemCuoiKy : (hk1 ? hk1.DiemCuoiKy : 0),
        DiemTBMon: parseFloat(weightedAvg.toFixed(2)),
        // Store both semester info for display
        HK1Avg: hk1 ? parseFloat(hk1.DiemTBMon.toFixed(2)) : 0,
        HK2Avg: hk2 ? parseFloat(hk2.DiemTBMon.toFixed(2)) : 0
      }
    })
    
    studentGrades.value = weightedGrades
  } else if (studentData[selectedHocKy.value]) {
    studentGrades.value = Object.values(studentData[selectedHocKy.value])
  } else {
    studentGrades.value = []
  }
}

const getAverageGPA = () => {
  if (studentGrades.value.length === 0) return 0
  const total = studentGrades.value.reduce((sum, g) => sum + (g.DiemTBMon || 0), 0)
  return (total / studentGrades.value.length).toFixed(2)
}

const getGradeRating = (grade) => {
  if (grade >= 9) return 'A+ (Xuất sắc)'
  if (grade >= 8) return 'A (Giỏi)'
  if (grade >= 7) return 'B (Khá)'
  if (grade >= 6) return 'C (Trung bình)'
  if (grade >= 5) return 'D (Yếu)'
  return 'F (Kém)'
}

onMounted(() => {
  fetchClasses()
})

const filteredClasses = computed(() => {
  return classesList.value.filter(c => {
    return ((c.MaLop || '').toLowerCase().includes(search.value.toLowerCase()) || 
           (c.TenLop || '').toLowerCase().includes(search.value.toLowerCase()) ||
           (c.MaGVChuNhiem || '').toLowerCase().includes(search.value.toLowerCase())) &&
           (gradeFilter.value === 'All' || String(c.KhoiLop) === gradeFilter.value)
  })
})
</script>

<template>
  <div class="space-y-6">
    <!-- PAGE HEADER -->
    <div class="flex justify-between items-end">
      <div>
        <h2 class="text-2xl font-bold text-[#2B3674] dark:text-white mb-1">Lớp Học</h2>
        <p class="text-sm text-gray-400 dark:text-gray-400">Monitor class capacities, assignments, and schedules.</p>
      </div>
      <div class="flex items-center gap-3">
        <button @click="fetchClasses" class="flex items-center gap-2 px-4 py-2 border border-blue-200 dark:border-blue-500/30 rounded-lg text-sm font-medium text-blue-600 dark:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-500/10 transition-colors">
          <RefreshCcw :size="16" :class="{ 'animate-spin': loading }" />
          Refresh
        </button>
        <button class="flex items-center gap-2 px-4 py-2 bg-[#1E88E5] hover:bg-blue-600 text-white rounded-lg text-sm font-bold transition-colors shadow-sm shadow-blue-500/30 dark:shadow-none hidden sm:flex">
          <Plus :size="16" />
          Create Class
        </button>
      </div>
    </div>

    <!-- SUMMARY CARDS -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
      <div v-for="card in summaryCards" :key="card.id" class="bg-white dark:bg-[#111C44] rounded-2xl p-6 shadow-sm border border-gray-100/50 dark:border-white/5 flex items-center gap-5">
        <div :class="['w-14 h-14 rounded-full flex items-center justify-center', card.color]">
          <component :is="card.icon" :size="24" stroke-width="2" />
        </div>
        <div>
          <p class="text-xs font-bold text-gray-400 dark:text-gray-400 tracking-wider mb-1">{{ card.title }}</p>
          <div class="flex items-end gap-2">
            <h3 class="text-2xl font-extrabold text-[#2B3674] dark:text-white leading-none">{{ card.value }}</h3>
            <span class="text-xs font-bold text-gray-400 dark:text-gray-500 mb-0.5">{{ card.subtitle }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- MAIN TABLE CARD -->
    <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 relative min-h-[400px]">
      
      <!-- TOOLBAR -->
      <div class="p-5 border-b border-gray-100 dark:border-white/5 flex flex-wrap items-center justify-between gap-4 bg-gray-50/50 dark:bg-white/5">
        <div class="relative flex-1 min-w-[200px] max-w-sm">
          <Search :size="18" class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 dark:text-gray-500" />
          <input 
            v-model="search"
            type="text" 
            placeholder="Tìm theo Lớp, Tên lớp, Mã GV..." 
            class="w-full pl-10 pr-4 py-2 bg-white dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 shadow-sm rounded-lg text-sm font-medium focus:border-blue-500 transition-all outline-none dark:text-white placeholder-gray-400 dark:placeholder-gray-500"
          />
        </div>
        
        <div class="flex items-center gap-3">
          <div class="relative">
            <select v-model="gradeFilter" class="appearance-none bg-white dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 shadow-sm text-gray-600 dark:text-gray-200 py-2 pl-4 pr-10 rounded-lg text-sm font-bold focus:outline-none focus:border-blue-500 relative cursor-pointer outline-none">
              <Filter :size="14" class="inline mr-2" />
              <option value="All">All Grades</option>
              <option value="9">Khối 9</option>
              <option value="10">Khối 10</option>
              <option value="11">Khối 11</option>
              <option value="12">Khối 12</option>
            </select>
          </div>
        </div>
      </div>

      <!-- LOADING OVERLAY -->
      <div v-if="loading" class="absolute inset-0 bg-white/50 dark:bg-[#111C44]/50 z-10 flex items-center justify-center backdrop-blur-sm rounded-2xl">
        <RefreshCcw :size="32" class="animate-spin text-blue-500" />
      </div>

      <!-- TABLE -->
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="text-[11px] font-bold text-gray-400 uppercase tracking-wider border-b border-gray-100 dark:border-white/5">
              <th class="py-4 px-6">MÃ LỚP / TÊN LỚP</th>
              <th class="py-4 px-3 text-center">NĂM HỌC</th>
              <th class="py-4 px-3">GV CHỦ NHIỆM (MÃ GV)</th>
              <th class="py-4 pr-6 pl-3 text-right">ACTIONS</th>
            </tr>
          </thead>
          <tbody class="text-sm font-medium">
            <tr v-for="cls in filteredClasses" :key="cls.MaLop" @click="selectClass(cls)" class="border-b border-gray-50 dark:border-white/5 hover:bg-blue-50/50 dark:hover:bg-blue-500/10 transition-colors cursor-pointer">
              <td class="py-4 px-6">
                <div class="flex items-center gap-3">
                  <div class="w-10 h-10 rounded-xl bg-blue-50 dark:bg-blue-500/10 border border-blue-100 dark:border-blue-500/20 flex flex-col items-center justify-center shrink-0">
                    <span class="text-[10px] font-bold text-blue-400 uppercase leading-none mb-0.5">Khối</span>
                    <strong class="text-blue-600 dark:text-blue-400 leading-none">{{ cls.KhoiLop }}</strong>
                  </div>
                  <div>
                    <h4 class="font-extrabold text-[#2B3674] dark:text-gray-100 text-base">{{ cls.MaLop }}</h4>
                    <p class="text-xs font-bold text-gray-400 dark:text-gray-500">{{ cls.TenLop }}</p>
                  </div>
                </div>
              </td>
              <td class="py-4 px-3 text-center font-bold text-[#1E88E5] dark:text-blue-400">
                {{ cls.NamHoc }}
              </td>
              <td class="py-4 px-3">
                <div class="flex items-center gap-2">
                  <div class="w-6 h-6 rounded-full bg-[#2B3674] dark:bg-gray-700 text-white flex items-center justify-center text-[10px] font-bold">
                    GV
                  </div>
                  <span class="font-bold text-[#2B3674] dark:text-gray-200">{{ cls.MaGVChuNhiem || 'Chưa phân công' }}</span>
                </div>
              </td>
              <td class="py-4 pr-6 pl-3 text-right">
                <button @click.stop class="p-2 text-gray-400 dark:text-gray-500 hover:text-[#2B3674] dark:hover:text-white transition-colors rounded-lg hover:bg-gray-50 dark:hover:bg-white/5">
                  <MoreHorizontal :size="18" />
                </button>
              </td>
            </tr>
            <tr v-if="filteredClasses.length === 0 && !loading">
              <td colspan="4" class="py-12 text-center text-gray-400 dark:text-gray-500 font-bold">
                Không tìm thấy danh sách Lớp học nào.
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- CLASS DETAIL PANEL -->
    <div v-if="selectedClass" class="space-y-6">
      
      <!-- HEADER WITH CLOSE BUTTON -->
      <div class="flex justify-between items-center">
        <div>
          <h3 class="text-2xl font-bold text-[#2B3674] dark:text-white">Chi Tiết Lớp: {{ selectedClass.MaLop }}</h3>
          <p class="text-sm text-gray-400 dark:text-gray-500 mt-1">{{ selectedClass.TenLop }}</p>
        </div>
        <button @click="closeDetails" class="p-2 text-gray-400 hover:text-[#2B3674] dark:hover:text-white transition-colors rounded-lg hover:bg-gray-50 dark:hover:bg-white/5">
          <X :size="24" />
        </button>
      </div>

      <!-- INFO & TEACHER ROW -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        <!-- CLASS INFO CARD -->
        <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 p-6">
          <h4 class="font-bold text-[#2B3674] dark:text-white mb-4">Thông Tin Lớp</h4>
          <div class="space-y-4">
            <div>
              <p class="text-xs font-bold text-gray-400 dark:text-gray-500 uppercase tracking-wide mb-1">MÃ LỚP</p>
              <p class="text-base font-bold text-[#2B3674] dark:text-white">{{ selectedClass.MaLop }}</p>
            </div>
            <div>
              <p class="text-xs font-bold text-gray-400 dark:text-gray-500 uppercase tracking-wide mb-1">KHỐI LỚP</p>
              <p class="text-base font-bold text-blue-600 dark:text-blue-400">Khối {{ selectedClass.KhoiLop }}</p>
            </div>
            <div>
              <p class="text-xs font-bold text-gray-400 dark:text-gray-500 uppercase tracking-wide mb-1">NĂM HỌC</p>
              <p class="text-base font-bold text-[#2B3674] dark:text-white">{{ selectedClass.NamHoc }}</p>
            </div>
          </div>
        </div>

        <!-- QUICK STATS -->
        <div class="bg-gradient-to-br from-blue-50 to-blue-100 dark:from-blue-500/10 dark:to-blue-500/5 rounded-2xl shadow-sm border border-blue-200 dark:border-blue-500/20 p-6">
          <h4 class="font-bold text-blue-900 dark:text-blue-100 mb-4">Thống Kê Học Sinh</h4>
          <div class="space-y-3">
            <div class="flex justify-between items-center">
              <span class="text-sm font-semibold text-blue-700 dark:text-blue-300">Tổng Số</span>
              <span class="text-2xl font-bold text-blue-600 dark:text-blue-400">{{ classStudents.length }}</span>
            </div>
            <div class="h-px bg-blue-200 dark:bg-blue-500/30"></div>
            <div class="flex justify-between items-center">
              <span class="text-sm font-semibold text-green-700 dark:text-green-300">🌟 Giỏi (≥8)</span>
              <span class="text-lg font-bold text-green-600 dark:text-green-400">{{ classStats.excellent }}</span>
            </div>
            <div class="flex justify-between items-center">
              <span class="text-sm font-semibold text-blue-700 dark:text-blue-300">⭐ Khá (7-<8)</span>
              <span class="text-lg font-bold text-blue-600 dark:text-blue-400">{{ classStats.good }}</span>
            </div>
            <div class="flex justify-between items-center">
              <span class="text-sm font-semibold text-yellow-700 dark:text-yellow-300">✓ Trung bình (6-<7)</span>
              <span class="text-lg font-bold text-yellow-600 dark:text-yellow-400">{{ classStats.average }}</span>
            </div>
            <div class="flex justify-between items-center">
              <span class="text-sm font-semibold text-orange-700 dark:text-orange-300">⚠ Yếu (<6)</span>
              <span class="text-lg font-bold text-orange-600 dark:text-orange-400">{{ classStats.weak }}</span>
            </div>
          </div>
        </div>

        <!-- TEACHER INFO CARD -->
        <div v-if="loadingDetails" class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 p-6 flex items-center justify-center">
          <RefreshCcw :size="24" class="animate-spin text-blue-500" />
        </div>
        
        <div v-else-if="classTeacher" class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 p-6">
          <h4 class="font-bold text-[#2B3674] dark:text-white mb-4">GV Chủ Nhiệm</h4>
          <div class="flex items-start gap-3">
            <div class="w-12 h-12 rounded-full bg-blue-600 dark:bg-blue-500 text-white flex items-center justify-center font-bold text-base shrink-0">
              {{ classTeacher.HoTen?.charAt(0).toUpperCase() }}
            </div>
            <div class="flex-1 min-w-0 space-y-2">
              <h5 class="font-bold text-[#2B3674] dark:text-white truncate">{{ classTeacher.HoTen }}</h5>
              <p class="text-xs text-gray-500 dark:text-gray-400">{{ classTeacher.MaGV }}</p>
              <p class="text-xs text-blue-600 dark:text-blue-400 font-semibold">{{ classTeacher.ChuyenMon }}</p>
              <div v-if="classTeacher.Email_PhuHuynh" class="pt-1 border-t border-gray-100 dark:border-white/10">
                <p class="text-xs text-gray-500 dark:text-gray-400">📧 Email:</p>
                <a :href="`mailto:${classTeacher.Email_PhuHuynh}`" class="text-xs text-blue-600 dark:text-blue-400 hover:underline break-all">{{ classTeacher.Email_PhuHuynh }}</a>
              </div>
            </div>
          </div>
        </div>

        <div v-else class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 p-6 flex items-center justify-center">
          <p class="text-sm text-gray-400 dark:text-gray-500">Chưa phân công GV</p>
        </div>
      </div>

      <!-- STUDENTS TABLE - FULL WIDTH -->
      <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 overflow-hidden">
        <div class="p-6 border-b border-gray-100 dark:border-white/5 flex items-center justify-between">
          <div>
            <h3 class="text-lg font-bold text-[#2B3674] dark:text-white">📚 Danh Sách Học Sinh</h3>
            <p class="text-sm text-gray-400 dark:text-gray-500 mt-1">Tổng: {{ classStudents.length }} học sinh</p>
          </div>
        </div>

        <div v-if="loadingDetails" class="flex justify-center py-8">
          <RefreshCcw :size="24" class="animate-spin text-blue-500" />
        </div>

        <div v-else-if="classStudents.length > 0" class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead class="bg-gradient-to-r from-blue-50 to-blue-100 dark:from-blue-500/10 dark:to-blue-500/5 text-[11px] font-bold text-gray-600 dark:text-gray-300 uppercase tracking-wider">
              <tr>
                <th class="px-6 py-3 text-left">STT</th>
                <th class="px-6 py-3 text-left">MÃ HỌC SINH</th>
                <th class="px-6 py-3 text-left">HỌ TÊN</th>
                <th class="px-6 py-3 text-left">NGÀY SINH</th>
                <th class="px-6 py-3 text-left">ĐỊA CHỈ</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(student, idx) in classStudents" :key="student.MaHS" @click="viewStudentGrades(student)" class="border-b border-gray-50 dark:border-white/5 hover:bg-blue-50/30 dark:hover:bg-blue-500/5 transition-colors cursor-pointer">
                <td class="px-6 py-4 font-bold text-gray-600 dark:text-gray-300">{{ idx + 1 }}</td>
                <td class="px-6 py-4">
                  <span class="font-semibold text-blue-600 dark:text-blue-400">{{ student.MaHS }}</span>
                </td>
                <td class="px-6 py-4">
                  <p class="font-bold text-[#2B3674] dark:text-white">{{ student.HoTen }}</p>
                </td>
                <td class="px-6 py-4 text-sm text-gray-600 dark:text-gray-300">
                  {{ student.NgaySinh ? new Date(student.NgaySinh).toLocaleDateString('vi-VN') : '-' }}
                </td>
                <td class="px-6 py-4 text-sm text-gray-600 dark:text-gray-300">
                  {{ student.DiaChi || '-' }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-else class="text-center py-16 text-gray-400 dark:text-gray-500">
          <Users :size="48" class="mx-auto mb-4 opacity-30" />
          <p class="font-semibold text-lg">Không có học sinh nào trong lớp</p>
        </div>
      </div>

      <!-- STUDENT GRADES PANEL -->
      <div v-if="selectedStudent" class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 overflow-hidden">
        <!-- Student Info Header -->
        <div class="p-6 border-b border-gray-100 dark:border-white/5 bg-gradient-to-r from-blue-50 to-blue-100 dark:from-blue-500/10 dark:to-blue-500/5">
          <div class="flex items-start justify-between">
            <div class="flex items-start gap-4">
              <div class="w-20 h-20 rounded-full bg-blue-600 dark:bg-blue-500 text-white flex items-center justify-center font-bold text-3xl shadow-lg">
                {{ selectedStudent.HoTen?.charAt(0).toUpperCase() }}
              </div>
              <div class="flex-1">
                <h2 class="text-2xl font-bold text-[#2B3674] dark:text-white">{{ selectedStudent.HoTen }}</h2>
                <p class="text-sm text-gray-600 dark:text-gray-300 mt-1">Mã: {{ selectedStudent.MaHS }} • Lớp: {{ selectedClass.MaLop }}</p>
                <p class="text-sm text-gray-600 dark:text-gray-300">📅 {{ selectedStudent.NgaySinh ? new Date(selectedStudent.NgaySinh).toLocaleDateString('vi-VN') : '-' }} • 📍 {{ selectedStudent.DiaChi || '-' }}</p>
              </div>
            </div>
            <button @click="selectedStudent = null" class="p-2 text-gray-400 hover:text-[#2B3674] dark:hover:text-white transition-colors rounded-lg hover:bg-gray-50 dark:hover:bg-white/5">
              <X :size="24" />
            </button>
          </div>
        </div>

        <!-- Term Selector - Grouped by School Year -->
        <div v-if="Object.keys(termsGroupedByYear).length > 0" class="p-4 border-b border-gray-100 dark:border-white/5 bg-gray-50/50 dark:bg-white/5">
          <div class="space-y-3">
            <div v-for="(year, yearKey) in termsGroupedByYear" :key="yearKey" class="space-y-2">
              <p class="text-xs font-bold text-gray-500 dark:text-gray-400 uppercase tracking-wide">Năm Học {{ yearKey }}-{{ parseInt(yearKey) + 1 }}</p>
              <div class="flex gap-2 flex-wrap">
                <!-- Semester buttons -->
                <button
                  v-for="term in year"
                  :key="term"
                  @click="selectedHocKy = term; updateStudentGrades()"
                  :class="[
                    'px-4 py-2 rounded-lg font-semibold text-sm transition-colors',
                    selectedHocKy === term 
                      ? 'bg-blue-600 dark:bg-blue-500 text-white' 
                      : 'bg-white dark:bg-[#0B1437] text-gray-600 dark:text-gray-300 border border-gray-200 dark:border-white/10 hover:border-blue-500 dark:hover:border-blue-500'
                  ]"
                >
                  {{ typeof term === 'string' && term.includes('-') ? `HK${term.split('-')[1]}` : `Học Kỳ ${term}` }}
                </button>
                <!-- Year average button -->
                <button
                  @click="selectedHocKy = `${yearKey}-YEAR`; updateStudentGrades()"
                  :class="[
                    'px-4 py-2 rounded-lg font-semibold text-sm transition-colors',
                    selectedHocKy === `${yearKey}-YEAR` 
                      ? 'bg-green-600 dark:bg-green-500 text-white' 
                      : 'bg-white dark:bg-[#0B1437] text-green-700 dark:text-green-400 border border-green-200 dark:border-green-500/20 hover:border-green-500 dark:hover:border-green-500'
                  ]"
                >
                   Cả Năm
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Grades Table -->
        <div v-if="studentGrades.length > 0" class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead class="bg-gray-50/50 dark:bg-white/5 text-[11px] font-bold text-gray-600 dark:text-gray-300 uppercase tracking-wider">
              <tr>
                <th class="px-6 py-3 text-left">MÔN HỌC</th>
                <th class="px-6 py-3 text-center">MIỆNG</th>
                <th class="px-6 py-3 text-center">15 PHÚT</th>
                <th class="px-6 py-3 text-center">GIỮA KỲ</th>
                <th class="px-6 py-3 text-center">CUỐI KỲ</th>
                <th class="px-6 py-3 text-center">ĐTB</th>
                <th class="px-6 py-3 text-center">XẾPLOẠI</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="grade in studentGrades" :key="grade.TenMon" class="border-b border-gray-50 dark:border-white/5 hover:bg-blue-50/30 dark:hover:bg-blue-500/5">
                <td class="px-6 py-4">
                  <p class="font-bold text-[#2B3674] dark:text-white">{{ grade.TenMon }}</p>
                </td>
                <td class="px-6 py-4 text-center">
                  <span class="inline-flex items-center justify-center w-8 h-8 rounded-lg bg-blue-100 dark:bg-blue-500/20 text-blue-600 dark:text-blue-400 font-bold">
                    {{ grade.DiemMieng }}
                  </span>
                </td>
                <td class="px-6 py-4 text-center">
                  <span class="inline-flex items-center justify-center w-8 h-8 rounded-lg bg-purple-100 dark:bg-purple-500/20 text-purple-600 dark:text-purple-400 font-bold">
                    {{ grade.Diem15p }}
                  </span>
                </td>
                <td class="px-6 py-4 text-center">
                  <span class="inline-flex items-center justify-center w-8 h-8 rounded-lg bg-teal-100 dark:bg-teal-500/20 text-teal-600 dark:text-teal-400 font-bold">
                    {{ grade.DiemGiuaKy }}
                  </span>
                </td>
                <td class="px-6 py-4 text-center">
                  <span class="inline-flex items-center justify-center w-8 h-8 rounded-lg bg-orange-100 dark:bg-orange-500/20 text-orange-600 dark:text-orange-400 font-bold">
                    {{ grade.DiemCuoiKy }}
                  </span>
                </td>
                <td class="px-6 py-4 text-center">
                  <span :class="[
                    'inline-flex items-center justify-center w-10 h-10 rounded-full font-bold text-white',
                    grade.DiemTBMon >= 9 ? 'bg-green-500' :
                    grade.DiemTBMon >= 8 ? 'bg-blue-500' :
                    grade.DiemTBMon >= 7 ? 'bg-cyan-500' :
                    grade.DiemTBMon >= 6 ? 'bg-yellow-500' :
                    grade.DiemTBMon >= 5 ? 'bg-orange-500' : 'bg-red-500'
                  ]">
                    {{ grade.DiemTBMon }}
                  </span>
                </td>
                <td class="px-6 py-4 text-center">
                  <span :class="[
                    'inline-block px-3 py-1 rounded-full text-xs font-bold',
                    grade.DiemTBMon >= 9 ? 'bg-green-100 text-green-700 dark:bg-green-500/20 dark:text-green-400' :
                    grade.DiemTBMon >= 8 ? 'bg-blue-100 text-blue-700 dark:bg-blue-500/20 dark:text-blue-400' :
                    grade.DiemTBMon >= 7 ? 'bg-cyan-100 text-cyan-700 dark:bg-cyan-500/20 dark:text-cyan-400' :
                    grade.DiemTBMon >= 6 ? 'bg-yellow-100 text-yellow-700 dark:bg-yellow-500/20 dark:text-yellow-400' :
                    grade.DiemTBMon >= 5 ? 'bg-orange-100 text-orange-700 dark:bg-orange-500/20 dark:text-orange-400' : 
                    'bg-red-100 text-red-700 dark:bg-red-500/20 dark:text-red-400'
                  ]">
                    {{ getGradeRating(grade.DiemTBMon) }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>

          <!-- GPA Summary -->
          <div class="p-6 bg-gradient-to-r from-blue-50 to-blue-100 dark:from-blue-500/10 dark:to-blue-500/5 border-t border-gray-100 dark:border-white/5">
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-3">
                <Award :size="24" class="text-blue-600 dark:text-blue-400" />
                <div>
                  <p class="text-sm font-semibold text-gray-600 dark:text-gray-300">
                    Điểm Trung Bình 
                    <span v-if="String(selectedHocKy).includes('-YEAR')">
                      Cả Năm Học {{ String(selectedHocKy).split('-')[0] }}-{{ parseInt(String(selectedHocKy).split('-')[0]) + 1 }}
                    </span>
                    <span v-else-if="String(selectedHocKy).includes('-')">
                      HK{{ String(selectedHocKy).split('-')[1] }} ({{ String(selectedHocKy).split('-')[0] }})
                    </span>
                    <span v-else>
                      Học Kỳ {{ selectedHocKy }}
                    </span>
                  </p>
                  <p class="text-2xl font-bold text-blue-600 dark:text-blue-400">{{ getAverageGPA() }}/10</p>
                </div>
              </div>
              <div class="text-right">
                <p class="text-sm font-semibold text-gray-600 dark:text-gray-300">Xếp Loại</p>
                <p :class="[
                  'text-xl font-bold',
                  getAverageGPA() >= 9 ? 'text-green-600 dark:text-green-400' :
                  getAverageGPA() >= 8 ? 'text-blue-600 dark:text-blue-400' :
                  getAverageGPA() >= 7 ? 'text-cyan-600 dark:text-cyan-400' :
                  getAverageGPA() >= 6 ? 'text-yellow-600 dark:text-yellow-400' :
                  getAverageGPA() >= 5 ? 'text-orange-600 dark:text-orange-400' : 'text-red-600 dark:text-red-400'
                ]">
                  {{ getGradeRating(getAverageGPA()) }}
                </p>
              </div>
            </div>
          </div>
        </div>

        <div v-else class="p-6 text-center text-gray-400 dark:text-gray-500">
          <Book :size="32" class="mx-auto mb-3 opacity-30" />
          <p class="font-semibold">Chưa có dữ liệu điểm số cho học kỳ này</p>
        </div>
      </div>
    </div>
  </div>
</template>
