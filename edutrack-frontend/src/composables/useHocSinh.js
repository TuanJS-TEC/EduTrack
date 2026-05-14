import { ref, computed, watch, onMounted } from 'vue'
import { apiService } from '../services/api'
import { useAuthStore } from '../stores/auth'
import * as XLSX from 'xlsx'

export function useHocSinh() {
  const auth = useAuthStore()
  const canEditStudents = () => auth.hasPermission('Students.Edit')
  const search = ref('')
  const gradeFilter = ref('All')
  const statusFilter = ref('All')
  const students = ref([])
  const loading = ref(false)

  // ── Fetch data ─────────────────────────────────────
  const fetchStudents = async () => {
    loading.value = true
    try {
      const res = await apiService.getHocSinhs()
      students.value = res.data
    } catch (error) {
      console.error('Lỗi khi tải dữ liệu Học Sinh:', error)
    } finally {
      loading.value = false
    }
  }

  onMounted(fetchStudents)

  // ── Filtered list ──────────────────────────────────
  const filteredStudents = computed(() => {
    const q = search.value.toLowerCase()
    return students.value.filter(s => {
      const matchSearch = (s.HoTen || '').toLowerCase().includes(q)
        || (s.MaHS || '').toLowerCase().includes(q)
      const matchGrade = gradeFilter.value === 'All' || (s.MaLop || '').startsWith(gradeFilter.value)
      const matchStatus = statusFilter.value === 'All' || (s.TrangThai || 'Đang học') === statusFilter.value
      return matchSearch && matchGrade && matchStatus
    })
  })

  // ── Pagination ─────────────────────────────────────
  const PAGE_SIZE = 3
  const currentPage = ref(1)
  const totalPages = computed(() => Math.max(1, Math.ceil(filteredStudents.value.length / PAGE_SIZE)))

  const pagedStudents = computed(() => {
    const start = (currentPage.value - 1) * PAGE_SIZE
    return filteredStudents.value.slice(start, start + PAGE_SIZE)
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

  // Reset về trang 1 khi filter thay đổi
  watch([search, gradeFilter, statusFilter], () => { currentPage.value = 1 })

  // ── Helpers ────────────────────────────────────────
  const getGpaColor = (gpa) => {
    if (gpa >= 8.0) return 'text-green-500 dark:text-green-400'
    if (gpa >= 6.5) return 'text-orange-400 dark:text-orange-300'
    return 'text-red-500 dark:text-red-400'
  }

  const getHanhKiemClass = (hk) => {
    const map = {
      'Tốt': 'bg-green-50 dark:bg-green-500/10 text-green-600 dark:text-green-400 border border-green-200 dark:border-transparent',
      'Khá': 'bg-blue-50 dark:bg-blue-500/10 text-blue-600 dark:text-blue-400 border border-blue-200 dark:border-transparent',
      'Trung bình': 'bg-orange-50 dark:bg-orange-500/10 text-orange-600 dark:text-orange-400 border border-orange-200 dark:border-transparent',
      'Yếu': 'bg-red-50 dark:bg-red-500/10 text-red-600 dark:text-red-400 border border-red-200 dark:border-transparent',
    }
    return map[hk] ?? 'bg-gray-100 dark:bg-white/10 text-gray-500 dark:text-gray-400 border border-gray-200 dark:border-transparent'
  }

  const AVATAR_COLORS = [
    'bg-blue-100   dark:bg-blue-500/20   text-blue-600   dark:text-blue-400',
    'bg-indigo-100 dark:bg-indigo-500/20 text-indigo-600 dark:text-indigo-400',
    'bg-emerald-100 dark:bg-emerald-500/20 text-emerald-600 dark:text-emerald-400',
    'bg-cyan-100   dark:bg-cyan-500/20   text-cyan-600   dark:text-cyan-400',
    'bg-purple-100 dark:bg-purple-500/20 text-purple-600 dark:text-purple-400',
    'bg-pink-100   dark:bg-pink-500/20   text-pink-600   dark:text-pink-400',
  ]

  const getAvatarColor = (id = '') => {
    let hash = 0
    for (let i = 0; i < id.length; i++) {
      hash = id.charCodeAt(i) + ((hash << 5) - hash)
    }
    return AVATAR_COLORS[Math.abs(hash) % AVATAR_COLORS.length]
  }

  const formatDate = (dateStr) => {
    if (!dateStr) return '-'
    const d = new Date(dateStr)
    return isNaN(d) ? '-' : d.toLocaleDateString('vi-VN')
  }

  // ── Export Excel ─────────────────────────────────
  const exportExcel = () => {
    const rows = filteredStudents.value.map((s, idx) => ({
      'STT':            idx + 1,
      'Mã Học Sinh':    s.MaHS,
      'Họ và Tên':      s.HoTen,
      'Ngày Sinh':      s.NgaySinh ? new Date(s.NgaySinh).toLocaleDateString('vi-VN') : '',
      'Lớp':            s.MaLop ?? '',
      'Trạng Thái':    s.TrangThai ?? 'Đang học',
      'Điểm TB':        s.DiemTB != null ? Number(s.DiemTB).toFixed(2) : '',
      'Hạnh Kiểm':    s.HanhKiem ?? '',
      'SĐT Phụ Huynh': s.SDT_PhuHuynh ?? '',
      'Email Phụ Huynh': s.Email_PhuHuynh ?? '',
      'Địa Chỉ':       s.DiaChi ?? '',
    }))

    const ws = XLSX.utils.json_to_sheet(rows)

    // Độ rộng cột
    ws['!cols'] = [
      { wch: 5 }, { wch: 12 }, { wch: 25 }, { wch: 14 },
      { wch: 8 }, { wch: 12 }, { wch: 10 }, { wch: 14 },
      { wch: 16 }, { wch: 28 }, { wch: 30 },
    ]

    const wb = XLSX.utils.book_new()
    XLSX.utils.book_append_sheet(wb, ws, 'Danh sách Học Sinh')

    const now = new Date()
    const stamp = `${now.getFullYear()}${String(now.getMonth()+1).padStart(2,'0')}${String(now.getDate()).padStart(2,'0')}`
    XLSX.writeFile(wb, `DanhSachHocSinh_${stamp}.xlsx`)
  }

  // ── Edit / Delete ──────────────────────────────────
  const showEditModal  = ref(false)
  const editForm       = ref({})
  const saving         = ref(false)

  const openEdit = (student) => {
    if (!canEditStudents()) return
    editForm.value = {
      MaHS:          student.MaHS,
      HoTen:         student.HoTen         ?? '',
      NgaySinh:      student.NgaySinh       ? student.NgaySinh.substring(0, 10) : '',
      DiaChi:        student.DiaChi         ?? '',
      MaLop:         student.MaLop          ?? '',
      TrangThai:     student.TrangThai       ?? 'Đang học',
      Email_PhuHuynh: student.Email_PhuHuynh ?? '',
      SDT_PhuHuynh:  student.SDT_PhuHuynh   ?? '',
    }
    showEditModal.value = true
  }

  const saveEdit = async () => {
    if (!canEditStudents()) return
    saving.value = true
    try {
      await apiService.updateHocSinh(editForm.value.MaHS, {
        ...editForm.value,
        NgaySinh: editForm.value.NgaySinh ? new Date(editForm.value.NgaySinh).toISOString() : null,
      })
      const idx = students.value.findIndex(s => s.MaHS === editForm.value.MaHS)
      if (idx !== -1) students.value[idx] = { ...students.value[idx], ...editForm.value }
      showEditModal.value = false
    } catch (e) {
      console.error('Lỗi cập nhật:', e)
      alert('Cập nhật thất bại, thử lại!')
    } finally {
      saving.value = false
    }
  }

  const confirmDelete = async (student) => {
    if (!canEditStudents()) return
    if (!confirm(`Xóa học sinh "${student.HoTen}" (${student.MaHS})?\nHành động này không thể hoàn tác.`)) return
    try {
      await apiService.deleteHocSinh(student.MaHS)
      students.value = students.value.filter(s => s.MaHS !== student.MaHS)
    } catch (e) {
      console.error('Lỗi xóa:', e)
      alert('Xóa thất bại! Kiểm tra lại.')
    }
  }

  // ── Add Student ─────────────────────────────────────
  const showAddModal = ref(false)
  const addForm = ref({})
  const adding = ref(false)

  const EMPTY_FORM = () => ({
    MaHS: '', HoTen: '', NgaySinh: '', DiaChi: '',
    MaLop: '', TrangThai: 'Đang học',
    Email_PhuHuynh: '', SDT_PhuHuynh: '',
  })

  const openAdd = () => {
    if (!canEditStudents()) return
    addForm.value = EMPTY_FORM()
    showAddModal.value = true
  }

  const saveAdd = async () => {
    if (!canEditStudents()) return
    if (!addForm.value.MaHS || !addForm.value.HoTen || !addForm.value.MaLop) {
      alert('Vui lòng điền đủ: Mã HS, Họ tên, Lớp!')
      return
    }
    adding.value = true
    try {
      const payload = {
        ...addForm.value,
        NgaySinh: addForm.value.NgaySinh ? new Date(addForm.value.NgaySinh).toISOString() : null,
      }
      const res = await apiService.createHocSinh(payload)
      students.value.push(res.data)
      showAddModal.value = false
    } catch (e) {
      const msg = e?.response?.data?.message ?? 'Thêm mới thất bại!'
      alert(msg)
    } finally {
      adding.value = false
    }
  }

  // ── Bulk Select ─────────────────────────────────────
  const selectedIds = ref(new Set())

  const isAllSelected = computed(() =>
    pagedStudents.value.length > 0 &&
    pagedStudents.value.every(s => selectedIds.value.has(s.MaHS))
  )

  const toggleAll = () => {
    if (isAllSelected.value) {
      pagedStudents.value.forEach(s => selectedIds.value.delete(s.MaHS))
    } else {
      pagedStudents.value.forEach(s => selectedIds.value.add(s.MaHS))
    }
    selectedIds.value = new Set(selectedIds.value) // trigger reactivity
  }

  const toggleOne = (maHS) => {
    const s = new Set(selectedIds.value)
    s.has(maHS) ? s.delete(maHS) : s.add(maHS)
    selectedIds.value = s
  }

  const bulkDelete = async () => {
    if (!canEditStudents()) return
    const count = selectedIds.value.size
    if (!count) return
    if (!confirm(`Xóa ${count} học sinh đã chọn?\nHành động này không thể hoàn tác.`)) return
    try {
      await Promise.all([...selectedIds.value].map(id => apiService.deleteHocSinh(id)))
      students.value = students.value.filter(s => !selectedIds.value.has(s.MaHS))
      selectedIds.value = new Set()
    } catch (e) {
      alert('Xóa hàng loạt thất bại, thử lại!')
    }
  }

  return {
    search, gradeFilter, statusFilter,
    students, loading,
    filteredStudents, pagedStudents,
    currentPage, totalPages, pageNumbers,
    goToPage, prevPage, nextPage,
    fetchStudents,
    getGpaColor, getHanhKiemClass, getAvatarColor, formatDate,
    showEditModal, editForm, saving,
    openEdit, saveEdit, confirmDelete,
    showAddModal, addForm, adding,
    openAdd, saveAdd,
    selectedIds, isAllSelected, toggleAll, toggleOne, bulkDelete,
    exportExcel,
  }
}
