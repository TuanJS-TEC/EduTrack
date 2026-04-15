import axios from 'axios'

/** Khớp năm học mặc định trong seed backend (DbSeeder) */
export const DEFAULT_NAM_HOC = '2025-2026'

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5245',
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('accessToken')
      localStorage.removeItem('refreshToken')
      localStorage.removeItem('username')
      localStorage.removeItem('role')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

function withDefaultNamHoc(payload) {
  if (!payload || typeof payload !== 'object') return payload
  return {
    ...payload,
    NamHoc: payload.NamHoc || DEFAULT_NAM_HOC,
  }
}

export const apiService = {
  // —— Auth ——
  login: (body) => api.post('/api/auth/login', body),
  refresh: (body) => api.post('/api/auth/refresh', body),
  revoke: () => api.post('/api/auth/revoke'),

  // —— Học sinh ——
  /** @param {string} [maLop] */
  getHocSinhs: (maLop) => api.get('/api/hocsinh', { params: { maLop: maLop || undefined } }),
  getHocSinhsPaged: (params) => api.get('/api/hocsinh/paged', { params }),
  getHocSinhById: (maHS) => api.get(`/api/hocsinh/${encodeURIComponent(maHS)}`),
  getHocSinhHoSo: (maHS) => api.get(`/api/hocsinh/${encodeURIComponent(maHS)}/ho-so`),
  getHocSinhLichSuHocTap: (maHS, params) =>
    api.get(`/api/hocsinh/${encodeURIComponent(maHS)}/lich-su-hoc-tap`, { params }),
  exportHocSinhExcel: (params) =>
    api.get('/api/hocsinh/export/excel', { params, responseType: 'blob' }),
  importHocSinhExcel: (formData) =>
    api.post('/api/hocsinh/import/excel', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }),
  createHocSinh: (data) => api.post('/api/hocsinh', data),
  updateHocSinh: (maHS, data) => api.put(`/api/hocsinh/${encodeURIComponent(maHS)}`, data),
  deleteHocSinh: (maHS) => api.delete(`/api/hocsinh/${encodeURIComponent(maHS)}`),

  // —— Giáo viên ——
  getGiaoViens: () => api.get('/api/giaovien'),
  getGiaoVienById: (maGV) => api.get(`/api/giaovien/${encodeURIComponent(maGV)}`),
  createGiaoVien: (data) => api.post('/api/giaovien', data),
  updateGiaoVien: (maGV, data) => api.put(`/api/giaovien/${encodeURIComponent(maGV)}`, data),
  deleteGiaoVien: (maGV) => api.delete(`/api/giaovien/${encodeURIComponent(maGV)}`),

  // —— Môn học ——
  /** @param {string} [maGV] lọc theo giáo viên phụ trách */
  getMonHocs: (maGV) => api.get('/api/monhoc', { params: { maGV: maGV || undefined } }),
  getMonHocById: (maMon) => api.get(`/api/monhoc/${encodeURIComponent(maMon)}`),
  createMonHoc: (data) => api.post('/api/monhoc', data),
  updateMonHoc: (maMon, data) => api.put(`/api/monhoc/${encodeURIComponent(maMon)}`, data),
  deleteMonHoc: (maMon) => api.delete(`/api/monhoc/${encodeURIComponent(maMon)}`),

  // —— Lớp học ——
  getLopHocs: () => api.get('/api/lophoc'),
  getLopHocById: (maLop) => api.get(`/api/lophoc/${encodeURIComponent(maLop)}`),
  createLopHoc: (data) => api.post('/api/lophoc', data),
  updateLopHoc: (maLop, data) => api.put(`/api/lophoc/${encodeURIComponent(maLop)}`, data),
  deleteLopHoc: (maLop) => api.delete(`/api/lophoc/${encodeURIComponent(maLop)}`),

  // —— Lịch học ——
  getLichHocByLop: (maLop) => api.get('/api/lichhoc', { params: { maLop } }),
  getLichHocByGV: (maGV) => api.get('/api/lichhoc', { params: { maGV } }),
  getLichHocById: (maLich) => api.get(`/api/lichhoc/${maLich}`),
  createLichHoc: (data) => api.post('/api/lichhoc', data),
  updateLichHoc: (maLich, data) => api.put(`/api/lichhoc/${maLich}`, data),
  deleteLichHoc: (maLich) => api.delete(`/api/lichhoc/${maLich}`),

  // —— Điểm số ——
  /**
   * GET /api/diemso — lọc: maHS, maMon, namHoc, hocKy (không có maLop)
   * @param {object} params
   */
  getDiemSos: (params) => api.get('/api/diemso', { params }),
  getDiemSoById: (maDiem) => api.get(`/api/diemso/${maDiem}`),
  getDiemSoAuditTrail: (maDiem) => api.get(`/api/diemso/${maDiem}/audit-trail`),
  /**
   * Bảng điểm theo lớp + môn — backend bắt buộc: maLop, maMon, namHoc, hocKy
   */
  getBangDiem: (maLop, maMon, hocKy, namHoc = DEFAULT_NAM_HOC) =>
    api.get('/api/diemso/bangdiem', { params: { maLop, maMon, namHoc, hocKy } }),
  getBangDiemExcel: (maLop, maMon, hocKy, namHoc = DEFAULT_NAM_HOC) =>
    api.get('/api/diemso/bangdiem/excel', {
      params: { maLop, maMon, namHoc, hocKy },
      responseType: 'blob',
    }),
  getBangDiemPdf: (maLop, maMon, hocKy, namHoc = DEFAULT_NAM_HOC) =>
    api.get('/api/diemso/bangdiem/pdf', {
      params: { maLop, maMon, namHoc, hocKy },
      responseType: 'blob',
    }),
  getDiemThongKe: (maLop, maMon, namHoc, hocKy, params = {}) =>
    api.get('/api/diemso/thong-ke', { params: { maLop, maMon, namHoc, hocKy, ...params } }),
  getDiemThongKeKhoi: (khoiLop, maMon, namHoc, hocKy, params = {}) =>
    api.get('/api/diemso/thong-ke/khoi', { params: { khoiLop, maMon, namHoc, hocKy, ...params } }),
  getTongHopHocSinh: (maHS, namHoc, hocKy) =>
    api.get('/api/diemso/tong-hop/hoc-sinh', { params: { maHS, namHoc, hocKy } }),
  getTongHopLop: (maLop, namHoc, hocKy) =>
    api.get('/api/diemso/tong-hop/lop', { params: { maLop, namHoc, hocKy } }),
  getTongHopKhoi: (khoiLop, namHoc, hocKy) =>
    api.get('/api/diemso/tong-hop/khoi', { params: { khoiLop, namHoc, hocKy } }),
  getTongHopTruong: (namHoc, hocKy) =>
    api.get('/api/diemso/tong-hop/truong', { params: { namHoc, hocKy } }),
  /** POST /api/diemso/upsert — tự gắn NamHoc mặc định nếu thiếu */
  upsertDiemSo: (data) => api.post('/api/diemso/upsert', withDefaultNamHoc(data)),
  bulkUpsertDiemSo: (data) => api.post('/api/diemso/bulk-upsert', withDefaultNamHoc(data)),
  importDiemSoExcel: (file, maLop, maMon, namHoc, hocKy) => {
    const fd = new FormData()
    fd.append('file', file)
    return api.post('/api/diemso/import/excel', fd, {
      params: { maLop, maMon, namHoc, hocKy },
      headers: { 'Content-Type': 'multipart/form-data' },
    })
  },
  deleteDiemSo: (maDiem) => api.delete(`/api/diemso/${maDiem}`),

  // —— Thông báo ——
  getThongBaos: (params) => api.get('/api/thongbao', { params }),
  getThongBaoById: (maTB) => api.get(`/api/thongbao/${maTB}`),
  createThongBao: (data) => api.post('/api/thongbao', data),
  updateThongBao: (maTB, data) => api.put(`/api/thongbao/${maTB}`, data),
  /** Đánh dấu đã đọc — PUT /api/thongbao/{id}/read */
  markThongBaoRead: (maTB) => api.put(`/api/thongbao/${maTB}/read`),
  /** @deprecated dùng markThongBaoRead — giữ tên cũ cho view hiện tại */
  markAsRead: (maTB) => api.put(`/api/thongbao/${maTB}/read`),
  deleteThongBao: (maTB) => api.delete(`/api/thongbao/${maTB}`),

  // —— Học phí ——
  getHocPhis: (params) => api.get('/api/hocphi', { params }),
  getHocPhiById: (maHocPhi) => api.get(`/api/hocphi/${maHocPhi}`),
  createHocPhi: (data) => api.post('/api/hocphi', data),
  updateHocPhi: (maHocPhi, data) => api.put(`/api/hocphi/${maHocPhi}`, data),
  deleteHocPhi: (maHocPhi) => api.delete(`/api/hocphi/${maHocPhi}`),

  // —— Kỳ học & workflow chốt điểm ——
  getKyHocs: () => api.get('/api/kyhoc'),
  getKyHocWorkflowLogs: (namHoc, hocKy) =>
    api.get('/api/kyhoc/workflow-log', { params: { namHoc, hocKy } }),
  upsertKyHoc: (body) => api.put('/api/kyhoc', body),
  submitKyHocReview: (body) => api.post('/api/kyhoc/submit-review', body),
  approveKyHoc: (body) => api.post('/api/kyhoc/approve', body),
  reopenKyHoc: (body) => api.post('/api/kyhoc/reopen', body),

  // —— Audit (báo cáo nội bộ) ——
  getAuditLogs: (params) => api.get('/api/audit-logs', { params }),

  // —— Báo cáo điều hành ——
  getReportDashboardBgh: (params) => api.get('/api/reports/dashboard/bgh', { params }),
  getReportDashboardKeToan: (params) => api.get('/api/reports/dashboard/ketoan', { params }),
  getReportDashboardGvcn: (params) => api.get('/api/reports/dashboard/gvcn', { params }),
  downloadReportOneClick: (params) =>
    api.get('/api/reports/download', { params, responseType: 'blob' }),

  // —— DSS ——
  getDssCanhBao: (hocKy, maLop, targetTb = 5.0, namHoc = DEFAULT_NAM_HOC) =>
    api.get('/api/dss/canh-bao-roi-mon', { params: { hocKy, maLop, targetTb, namHoc } }),
  getDssThongKeHocLuc: (hocKy, namHoc = DEFAULT_NAM_HOC) =>
    api.get('/api/dss/dashboard-hoc-luc', { params: { hocKy, namHoc } }),
  postDssWhatIf: (data) => api.post('/api/dss/what-if', data),
  getDssCanThiep: (hocKy, namHoc = DEFAULT_NAM_HOC, maLop) =>
    api.get('/api/dss/can-thiep', { params: { hocKy, namHoc, maLop } }),
  postDssMoPhong: (data) => api.post('/api/dss/mo-phong', data),

  // —— Người dùng (admin) ——
  getUsers: () => api.get('/api/users'),
  createUser: (data) => api.post('/api/users', data),
  setUserRoles: (userId, body) => api.post(`/api/users/${userId}/roles`, body),
  linkParentStudent: (userId, body) => api.post(`/api/users/${userId}/parent-link`, body),
}
