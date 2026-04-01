import axios from 'axios'

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5245',
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken')
  if (token) config.headers.Authorization = `Bearer ${token}`
  
  console.log(`📤 [API REQUEST] ${config.method?.toUpperCase()} ${config.url}`, {
    params: config.params,
    data: config.data,
    headers: config.headers
  })
  return config
})

// Response interceptor - log dữ liệu nhận về
api.interceptors.response.use(
  (response) => {
    console.log(`📥 [API RESPONSE] ${response.status} ${response.config.url}`, {
      data: response.data,
      headers: response.headers
    })
    return response
  },
  (error) => {
    console.error(`❌ [API ERROR] ${error.config?.method?.toUpperCase()} ${error.config?.url}`, {
      status: error.response?.status,
      data: error.response?.data,
      message: error.message
    })
    return Promise.reject(error)
  }
)

export const apiService = {
  // HocSinh
  getHocSinhs: (maLop) => api.get('/api/hocsinh', { params: { maLop } }),
  getHocSinhById: (id) => api.get(`/api/hocsinh/${id}`),
  createHocSinh: (data) => api.post('/api/hocsinh', data),
  updateHocSinh: (id, data) => api.put(`/api/hocsinh/${id}`, data),
  deleteHocSinh: (id) => api.delete(`/api/hocsinh/${id}`),

  // GiaoVien
  getGiaoViens: () => api.get('/api/giaovien'),
  getGiaoVienById: (id) => api.get(`/api/giaovien/${id}`),
  createGiaoVien: (data) => api.post('/api/giaovien', data),
  updateGiaoVien: (id, data) => api.put(`/api/giaovien/${id}`, data),
  deleteGiaoVien: (id) => api.delete(`/api/giaovien/${id}`),

  // MonHoc
  getMonHocs: () => api.get('/api/monhoc'),

  // LopHoc
  getLopHocs: () => api.get('/api/lophoc'),
  getLopHocById: (id) => api.get(`/api/lophoc/${id}`),
  
  // DiemSo
  getDiemSos: (hocKy, maLop, maMon) => api.get('/api/diemso', { params: { hocKy, maLop, maMon } }),
  getBangDiem: (maLop, maMon, hocKy) => api.get('/api/diemso/bangdiem', { params: { maLop, maMon, hocKy } }),
  saveDiemSo: (data) => api.post('/api/diemso', data),
  upsertDiemSo: (data) => api.post('/api/diemso/upsert', data),

  // ThongBao
  getThongBaos: () => api.get('/api/thongbao'),
  markAsRead: (id) => api.put(`/api/thongbao/${id}`),

  // DSs (Decision Support System)
  getDssCanhBao: (hocKy, maLop, targetTb) => api.get('/api/dss/canh-bao-roi-mon', { params: { hocKy, maLop, targetTb } }),
  getDssThongKeHocLuc: (hocKy, namHoc) => api.get('/api/dss/dashboard-hoc-luc', { params: { hocKy, namHoc } }),
  postDssWhatIf: (data) => api.post('/api/dss/what-if', data)
}

