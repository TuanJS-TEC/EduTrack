import { ref, computed, watch, onMounted } from 'vue'
import { apiService } from '../services/api'

export function useGiaoVien() {
  const search = ref('')
  const chuyenMonFilter = ref('All')
  const teachers = ref([])
  const loading = ref(false)
  const submitting = ref(false)

  // Modal & Form State
  const showModal = ref(false)
  const isEditMode = ref(false)
  const formData = ref({
    maGV: '',
    hoTen: '',
    chuyenMon: '',
    email: '',
    luongCoBan: ''
  })

  const chuyenMonOptions = [
    'Toán', 'Vật lý', 'Hóa học', 'Sinh học', 'Ngữ văn', 'Lịch sử', 'Địa lý', 'Tiếng Anh'
  ]

  // ── API Actions ────────────────────────────────────
  const fetchTeachers = async () => {
    loading.value = true
    try {
      const res = await apiService.getGiaoViens()
      teachers.value = res.data
    } catch (error) {
      console.error("Lỗi khi tải dữ liệu Giáo Viên:", error)
    } finally {
      loading.value = false
    }
  }

  onMounted(fetchTeachers)

  // ── Filters & Pagination ─────────────────────────
  const filteredTeachers = computed(() => {
    const q = search.value.toLowerCase()
    return teachers.value.filter(t => {
      const matchSearch = (t.HoTen || '').toLowerCase().includes(q) || 
                         (t.MaGV || '').toLowerCase().includes(q)
      const matchFilter = (chuyenMonFilter.value === 'All' || t.ChuyenMon === chuyenMonFilter.value)
      return matchSearch && matchFilter
    })
  })

  const PAGE_SIZE = 5
  const currentPage = ref(1)
  const totalPages = computed(() => Math.max(1, Math.ceil(filteredTeachers.value.length / PAGE_SIZE)))

  const pagedTeachers = computed(() => {
    const start = (currentPage.value - 1) * PAGE_SIZE
    return filteredTeachers.value.slice(start, start + PAGE_SIZE)
  })

  const pageNumbers = computed(() => {
    const total = totalPages.value
    if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1)
    const cur = currentPage.value
    const pages = new Set([1, total, cur, cur - 1, cur + 1].filter(p => p >= 1 && p <= total))
    return [...pages].sort((a, b) => a - b)
  })

  const goToPage = (p) => { currentPage.value = Math.max(1, Math.min(p, totalPages.value)) }
  const prevPage = () => goToPage(currentPage.value - 1)
  const nextPage = () => goToPage(currentPage.value + 1)

  watch([search, chuyenMonFilter], () => { currentPage.value = 1 })

  // ── CRUD Logic ─────────────────────────────────────
  const openAdd = () => {
    isEditMode.value = false
    formData.value = { maGV: '', hoTen: '', chuyenMon: '', email: '', luongCoBan: '' }
    showModal.value = true
  }

  const openEdit = (teacher) => {
    isEditMode.value = true
    formData.value = {
      maGV: teacher.MaGV,
      hoTen: teacher.HoTen,
      chuyenMon: teacher.ChuyenMon || '',
      email: teacher.Email || '',
      luongCoBan: teacher.LuongCoBan || ''
    }
    showModal.value = true
  }

  const closeModal = () => {
    showModal.value = false
  }

  const handleSubmit = async () => {
    if (!formData.value.maGV || !formData.value.hoTen) {
      alert('Vui lòng điền đủ: Mã GV, Họ tên!')
      return
    }
    submitting.value = true
    try {
      const payload = {
        ...formData.value,
        luongCoBan: formData.value.luongCoBan ? parseFloat(formData.value.luongCoBan) : null
      }
      if (isEditMode.value) {
        await apiService.updateGiaoVien(formData.value.maGV, payload)
        // Update local list
        const idx = teachers.value.findIndex(t => t.MaGV === formData.value.maGV)
        if (idx !== -1) {
            // Map keys back to backend format (MaGV, HoTen...) if needed or just fetch
            teachers.value[idx] = { 
                ...teachers.value[idx], 
                HoTen: formData.value.hoTen,
                ChuyenMon: formData.value.chuyenMon,
                Email: formData.value.email,
                LuongCoBan: formData.value.luongCoBan
            }
        }
      } else {
        const res = await apiService.createGiaoVien(payload)
        teachers.value.push(res.data)
      }
      closeModal()
    } catch (error) {
      console.error('Lỗi khi lưu:', error)
      alert(error.response?.data?.message || 'Có lỗi xảy ra')
    } finally {
      submitting.value = false
    }
  }

  const handleDelete = async (teacher) => {
    if (!confirm(`Bạn có chắc muốn xóa giáo viên ${teacher.HoTen}?`)) return
    try {
      await apiService.deleteGiaoVien(teacher.MaGV)
      teachers.value = teachers.value.filter(t => t.MaGV !== teacher.MaGV)
    } catch (error) {
      console.error('Lỗi khi xóa:', error)
      alert('Xóa thất bại!')
    }
  }

  // ── UI Helpers ─────────────────────────────────────
  const getInitials = (name) => {
    if (!name) return 'GV'
    const parts = name.replace(/(Dr\.|Mr\.|Ms\.|Prof\.)\s/g, '').split(' ')
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase()
    return name.charAt(0).toUpperCase()
  }

  const getColor = (id) => {
    const colors = [
      'bg-green-100 dark:bg-green-500/20 text-green-600 dark:text-green-400',
      'bg-teal-100 dark:bg-teal-500/20 text-teal-600 dark:text-teal-400',
      'bg-blue-100 dark:bg-blue-500/20 text-blue-600 dark:text-blue-400',
      'bg-indigo-100 dark:bg-indigo-500/20 text-indigo-600 dark:text-indigo-400',
      'bg-emerald-100 dark:bg-emerald-500/20 text-emerald-600 dark:text-emerald-400',
      'bg-orange-100 dark:bg-orange-500/20 text-orange-600 dark:text-orange-400',
      'bg-cyan-100 dark:bg-cyan-500/20 text-cyan-600 dark:text-cyan-400'
    ]
    let hash = 0
    for (let i = 0; i < (id || '').length; i++) {
      hash = id.charCodeAt(i) + ((hash << 5) - hash)
    }
    return colors[Math.abs(hash) % colors.length]
  }

  const formatCurrency = (value) => {
    if (!value) return '0 ₫'
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value)
  }

  return {
    search, chuyenMonFilter, teachers, loading, submitting,
    showModal, isEditMode, formData, chuyenMonOptions,
    filteredTeachers, pagedTeachers, currentPage, totalPages, pageNumbers,
    fetchTeachers, openAdd, openEdit, closeModal, handleSubmit, handleDelete,
    getInitials, getColor, formatCurrency, goToPage, prevPage, nextPage
  }
}
