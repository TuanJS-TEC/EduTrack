<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Download, FileText, Search, Plus, Edit2, Eye, Trash2, RefreshCcw, X, AlertCircle, CheckCircle } from 'lucide-vue-next'
import { ElMessage } from 'element-plus'
import { apiService } from '../services/api'
import { useAuthStore } from '../stores/auth'
import { getFirstAccessibleRoute } from '../router/index.js'

const router = useRouter()
const auth = useAuthStore()

/** Xem danh sách: Teachers.View. Thêm/sửa/xóa: Admin hoặc BGH (khớp policy API). */
const canViewTeachers = computed(() => auth.hasPermission('Teachers.View'))
const canManageTeachers = computed(() => auth.isAdmin || auth.isBGH)

const search = ref('')
const chuyenMonFilter = ref('All')

const teachers = ref([])
const loading = ref(false)
const submitting = ref(false)

// Modal & Form
const showModal = ref(false)
const isEditMode = ref(false)
const selectedTeacher = ref(null)
const formData = ref({
  maGV: '',
  hoTen: '',
  chuyenMon: '',
  email: '',
  luongCoBan: ''
})

// Notification
const notification = ref(null)

const showNotification = (message, type = 'success') => {
  notification.value = { message, type }
  setTimeout(() => {
    notification.value = null
  }, 3000)
}

const chuyenMonOptions = [
  'Toán',
  'Vật lý',
  'Hóa học',
  'Sinh học',
  'Ngữ văn',
  'Lịch sử',
  'Địa lý',
  'Tiếng Anh'
]

const fetchTeachers = async () => {
  if (!canViewTeachers.value) return
  loading.value = true
  try {
    const res = await apiService.getGiaoViens()
    teachers.value = res.data
  } catch (error) {
    console.error("Lỗi khi tải dữ liệu Giáo Viên:", error)
    showNotification('Lỗi khi tải dữ liệu', 'error')
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  if (!canViewTeachers.value) {
    ElMessage.warning('Bạn không có quyền xem danh sách giáo viên.')
    router.replace(getFirstAccessibleRoute(auth.permissions, auth.roles))
    return
  }
  fetchTeachers()
})

const filteredTeachers = computed(() => {
  return teachers.value.filter(t => {
    return ((t.HoTen || '').toLowerCase().includes(search.value.toLowerCase()) || 
           (t.MaGV || '').toLowerCase().includes(search.value.toLowerCase()) ||
           (t.ChuyenMon || '').toLowerCase().includes(search.value.toLowerCase())) &&
           (chuyenMonFilter.value === 'All' || t.ChuyenMon === chuyenMonFilter.value)
  })
})

// ── Pagination ─────────────────────────────────────
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

// Reset về trang 1 khi filter thay đổi
watch([search, chuyenMonFilter], () => { currentPage.value = 1 })

const getInitials = (name) => {
  if (!name) return 'GV'
  const parts = name.replace(/(Dr\.|Mr\.|Ms\.|Prof\.)\s/g, '').split(' ')
  if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase()
  return name.charAt(0).toUpperCase()
}

// Generate a random stable color based on ID
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

// Modal Functions
const openAddModal = () => {
  if (!canManageTeachers.value) {
    showNotification('Chỉ Admin hoặc BGH được thêm giáo viên.', 'error')
    return
  }
  isEditMode.value = false
  formData.value = {
    maGV: '',
    hoTen: '',
    chuyenMon: '',
    email: '',
    luongCoBan: ''
  }
  showModal.value = true
}

const openEditModal = (teacher) => {
  if (!canManageTeachers.value) {
    showNotification('Chỉ Admin hoặc BGH được sửa giáo viên.', 'error')
    return
  }
  isEditMode.value = true
  selectedTeacher.value = teacher
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
  selectedTeacher.value = null
  formData.value = {
    maGV: '',
    hoTen: '',
    chuyenMon: '',
    email: '',
    luongCoBan: ''
  }
}

const handleSubmit = async () => {
  if (!canManageTeachers.value) {
    showNotification('Chỉ Admin hoặc BGH được thêm/sửa giáo viên.', 'error')
    return
  }
  if (!formData.value.maGV || !formData.value.hoTen) {
    showNotification('Vui lòng điền đầy đủ thông tin', 'error')
    return
  }

  submitting.value = true
  try {
    const payload = {
      maGV: formData.value.maGV,
      hoTen: formData.value.hoTen,
      chuyenMon: formData.value.chuyenMon || null,
      email: formData.value.email || null,
      luongCoBan: formData.value.luongCoBan ? parseFloat(formData.value.luongCoBan) : null
    }

    if (isEditMode.value) {
      await apiService.updateGiaoVien(formData.value.maGV, payload)
      showNotification('Cập nhật giáo viên thành công!', 'success')
    } else {
      await apiService.createGiaoVien(payload)
      showNotification('Thêm giáo viên thành công!', 'success')
    }

    closeModal()
    await fetchTeachers()
  } catch (error) {
    console.error('Lỗi:', error)
    showNotification(error.response?.data?.message || 'Có lỗi xảy ra', 'error')
  } finally {
    submitting.value = false
  }
}

const handleDelete = async (maGV, hoTen) => {
  if (!canManageTeachers.value) {
    showNotification('Bạn không có quyền xóa giáo viên.', 'error')
    return
  }
  if (!confirm(`Bạn có chắc chắn muốn xóa giáo viên ${hoTen}?`)) {
    return
  }

  try {
    await apiService.deleteGiaoVien(maGV)
    showNotification('Xóa giáo viên thành công!', 'success')
    await fetchTeachers()
  } catch (error) {
    console.error('Lỗi:', error)
    showNotification(error.response?.data?.message || 'Có lỗi xảy ra', 'error')
  }
}
</script>

<template>
  <div class="space-y-6">
    <!-- PAGE HEADER -->
    <div class="flex justify-between items-end">
      <div>
        <h2 class="text-2xl font-bold text-[#2B3674] dark:text-white mb-1">Giáo Viên</h2>
        <p class="text-sm text-gray-400 dark:text-gray-400">Manage faculty members, subjects, and assignments.</p>
      </div>
      <div class="flex items-center gap-3">
        <button @click="fetchTeachers" class="flex items-center gap-2 px-4 py-2 border border-blue-200 dark:border-blue-500/30 rounded-lg text-sm font-medium text-blue-600 dark:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-500/10 transition-colors">
          <RefreshCcw :size="16" :class="{ 'animate-spin': loading }" />
          Refresh
        </button>
        <button class="flex items-center gap-2 px-4 py-2 border border-gray-200 dark:border-white/10 rounded-lg text-sm font-medium text-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors hidden sm:flex">
          <Download :size="16" />
          Export
        </button>
      </div>
    </div>

    <!-- MAIN TABLE CARD -->
    <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 relative min-h-[400px]">
      
      <!-- TOOLBAR -->
      <div class="p-5 border-b border-gray-100 dark:border-white/5 flex flex-wrap items-center justify-between gap-4 bg-gray-50/50 dark:bg-white/5">
        <div class="relative flex-1 min-w-[200px] max-w-lg">
          <Search :size="18" class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 dark:text-gray-500" />
          <input 
            v-model="search"
            type="text" 
            placeholder="Tìm theo tên hoặc ID..." 
            class="w-full pl-10 pr-4 py-2 bg-white dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 shadow-sm rounded-lg text-sm focus:border-blue-500 transition-all outline-none dark:text-white placeholder-gray-400 dark:placeholder-gray-500"
          />
        </div>
        
        <div class="flex items-center gap-3">
          <div class="relative">
            <select v-model="chuyenMonFilter" class="appearance-none bg-white dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 shadow-sm text-gray-700 dark:text-gray-200 py-2 pl-4 pr-10 rounded-lg text-sm font-medium focus:outline-none focus:border-blue-500 relative cursor-pointer outline-none w-40">
              <option value="All">Mọi chuyên môn</option>
              <option v-for="cm in chuyenMonOptions" :key="cm" :value="cm">{{ cm }}</option>
            </select>
          </div>

          <button
            v-if="canManageTeachers"
            @click="openAddModal"
            class="flex items-center gap-2 px-4 py-2 bg-[#1E88E5] hover:bg-blue-600 text-white rounded-lg text-sm font-medium transition-colors shadow-sm shadow-blue-500/30 dark:shadow-none whitespace-nowrap"
          >
            <Plus :size="16" />
            Add Teacher
          </button>
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
              <th class="py-4 pl-6 pr-3">TEACHER</th>
              <th class="py-4 px-3">ID</th>
              <th class="py-4 px-3">CHUYÊN MÔN</th>
              <th class="py-4 px-3">LƯƠNG CƠ BẢN</th>
              <th class="py-4 px-3 text-center">TRẠNG THÁI</th>
              <th class="py-4 pr-6 pl-3 text-right">ACTIONS</th>
            </tr>
          </thead>
          <tbody class="text-sm font-medium">
            <tr v-for="teacher in pagedTeachers" :key="teacher.MaGV" class="border-b border-gray-50 dark:border-white/5 hover:bg-gray-50/50 dark:hover:bg-white/5 transition-colors group">
              <td class="py-4 pl-6 pr-3">
                <div class="flex items-center gap-3">
                  <div :class="['w-9 h-9 rounded-full flex items-center justify-center font-bold text-sm tracking-tight shrink-0', getColor(teacher.MaGV)]">
                    {{ getInitials(teacher.HoTen) }}
                  </div>
                  <div>
                    <p class="font-bold text-[#2B3674] dark:text-gray-100">{{ teacher.HoTen }}</p>
                    <p class="text-[11px] text-gray-400 dark:text-gray-500 truncate max-w-[200px]">{{ teacher.Email || 'Chưa có Email' }}</p>
                  </div>
                </div>
              </td>
              <td class="py-4 px-3 text-xs font-mono font-bold text-gray-500 dark:text-gray-400">{{ teacher.MaGV }}</td>
              <td class="py-4 px-3 font-bold text-[#2B3674] dark:text-gray-200">Khoa {{ teacher.ChuyenMon || 'Chung' }}</td>
              <td class="py-4 px-3 text-[#1E88E5] dark:text-blue-400 font-extrabold">{{ formatCurrency(teacher.LuongCoBan) }}</td>
              <td class="py-4 px-3 text-center">
                <span 
                  class="inline-flex items-center px-2.5 py-1 rounded text-[11px] font-bold uppercase tracking-wider"
                  :class="{
                    'bg-green-50 dark:bg-green-500/10 text-green-600 dark:text-green-400 border border-green-200 dark:border-transparent': true,
                  }"
                >
                  Active
                </span>
              </td>
              <td class="py-4 pr-6 pl-3">
                <div class="flex items-center justify-end gap-2 opacity-100 lg:opacity-0 group-hover:opacity-100 transition-opacity">
                  <button
                    v-if="canManageTeachers"
                    @click="openEditModal(teacher)"
                    class="p-1.5 text-gray-400 dark:text-gray-500 hover:text-blue-500 hover:bg-blue-50 dark:hover:bg-blue-500/10 rounded-md transition-colors"
                    title="Edit"
                  >
                    <Edit2 :size="16" />
                  </button>
                  <button
                    v-if="canManageTeachers"
                    @click="handleDelete(teacher.MaGV, teacher.HoTen)"
                    class="p-1.5 text-gray-400 dark:text-gray-500 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-500/10 rounded-md transition-colors"
                    title="Delete"
                  >
                    <Trash2 :size="16" />
                  </button>
                </div>
              </td>
            </tr>
            <tr v-if="pagedTeachers.length === 0 && !loading">
              <td colspan="6" class="py-12 text-center text-gray-500 dark:text-gray-400 font-medium">
                Không tìm thấy Giáo viên nào.
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- FOOTER / PAGINATION -->
      <div class="p-5 border-t border-gray-100 dark:border-white/5 flex items-center justify-between text-sm text-gray-500 dark:text-gray-400 bg-gray-50/30 dark:bg-white-[0.02]">
        <div>
          Đang hiển thị <span class="font-bold text-[#2B3674] dark:text-white">{{ pagedTeachers.length }}</span> / <span class="font-bold text-[#2B3674] dark:text-white">{{ filteredTeachers.length }}</span> bản ghi
        </div>
        <div class="flex items-center gap-1">
          <button @click="prevPage" :disabled="currentPage === 1" class="w-8 h-8 flex items-center justify-center rounded text-gray-400 dark:text-gray-500 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors disabled:opacity-50 disabled:cursor-not-allowed" title="Previous">&lt;</button>
          <template v-for="page in pageNumbers" :key="page">
            <span v-if="page === '..'" class="px-2">...</span>
            <button v-else @click="goToPage(page)" :class="['w-8 h-8 flex items-center justify-center rounded font-medium transition-colors', currentPage === page ? 'bg-[#1E88E5] text-white shadow-sm shadow-blue-500/30 dark:shadow-none' : 'text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-white/5']">{{ page }}</button>
          </template>
          <button @click="nextPage" :disabled="currentPage === totalPages" class="w-8 h-8 flex items-center justify-center rounded text-gray-400 dark:text-gray-500 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors disabled:opacity-50 disabled:cursor-not-allowed" title="Next">&gt;</button>
        </div>
      </div>
    </div>

    <!-- MODAL: ADD/EDIT TEACHER -->
    <teleport to="body">
      <div v-if="showModal" class="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
        <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-xl max-w-md w-full max-h-[90vh] overflow-y-auto border border-gray-100 dark:border-white/10">
          <!-- Header -->
          <div class="p-6 border-b border-gray-100 dark:border-white/5 flex items-center justify-between">
            <h3 class="text-lg font-bold text-[#2B3674] dark:text-white">
              {{ isEditMode ? 'Sửa Giáo Viên' : 'Thêm Giáo Viên Mới' }}
            </h3>
            <button @click="closeModal" class="p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors">
              <X :size="20" />
            </button>
          </div>

          <!-- Body -->
          <form @submit.prevent="handleSubmit" class="p-6 space-y-4">
            <!-- Mã GV -->
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Mã Giáo Viên <span class="text-red-500">*</span>
              </label>
              <input 
                v-model="formData.maGV"
                :disabled="isEditMode"
                type="text" 
                placeholder="VD: GV001" 
                class="w-full px-4 py-2 border border-gray-200 dark:border-white/10 rounded-lg bg-white dark:bg-[#0B1437] text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:outline-none focus:border-blue-500 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              />
            </div>

            <!-- Họ Tên -->
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Họ và Tên <span class="text-red-500">*</span>
              </label>
              <input 
                v-model="formData.hoTen"
                type="text" 
                placeholder="VD: Nguyễn Văn A" 
                class="w-full px-4 py-2 border border-gray-200 dark:border-white/10 rounded-lg bg-white dark:bg-[#0B1437] text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:outline-none focus:border-blue-500 transition-colors"
              />
            </div>

            <!-- Chuyên Môn -->
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Chuyên Môn
              </label>
              <select 
                v-model="formData.chuyenMon"
                class="w-full px-4 py-2 border border-gray-200 dark:border-white/10 rounded-lg bg-white dark:bg-[#0B1437] text-gray-900 dark:text-white focus:outline-none focus:border-blue-500 transition-colors"
              >
                <option value="">-- Chọn chuyên môn --</option>
                <option v-for="cm in chuyenMonOptions" :key="cm" :value="cm">{{ cm }}</option>
              </select>
            </div>

            <!-- Email -->
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Email
              </label>
              <input 
                v-model="formData.email"
                type="email" 
                placeholder="VD: giaovien@example.com" 
                class="w-full px-4 py-2 border border-gray-200 dark:border-white/10 rounded-lg bg-white dark:bg-[#0B1437] text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:outline-none focus:border-blue-500 transition-colors"
              />
            </div>

            <!-- Lương Cơ Bản -->
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Lương Cơ Bản (VND)
              </label>
              <input 
                v-model="formData.luongCoBan"
                type="number" 
                placeholder="VD: 5000000" 
                class="w-full px-4 py-2 border border-gray-200 dark:border-white/10 rounded-lg bg-white dark:bg-[#0B1437] text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:outline-none focus:border-blue-500 transition-colors"
              />
            </div>

            <!-- Buttons -->
            <div class="flex gap-3 pt-4">
              <button 
                type="button"
                @click="closeModal"
                class="flex-1 px-4 py-2 border border-gray-200 dark:border-white/10 rounded-lg text-gray-700 dark:text-gray-300 font-medium hover:bg-gray-50 dark:hover:bg-white/5 transition-colors"
              >
                Hủy
              </button>
              <button 
                type="submit"
                :disabled="submitting"
                class="flex-1 px-4 py-2 bg-[#1E88E5] hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed text-white rounded-lg font-medium transition-colors"
              >
                {{ submitting ? 'Đang lưu...' : (isEditMode ? 'Cập nhật' : 'Thêm mới') }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </teleport>

    <!-- NOTIFICATION -->
    <teleport to="body">
      <transition name="fade">
        <div v-if="notification" class="fixed top-4 right-4 z-[60]">
          <div 
            :class="[
              'flex items-center gap-3 px-4 py-3 rounded-lg shadow-lg border',
              notification.type === 'success' 
                ? 'bg-green-50 dark:bg-green-500/10 border-green-200 dark:border-green-500/30 text-green-800 dark:text-green-300'
                : 'bg-red-50 dark:bg-red-500/10 border-red-200 dark:border-red-500/30 text-red-800 dark:text-red-300'
            ]"
          >
            <CheckCircle v-if="notification.type === 'success'" :size="20" />
            <AlertCircle v-else :size="20" />
            <p class="font-medium">{{ notification.message }}</p>
          </div>
        </div>
      </transition>
    </teleport>
  </div>
</template>

<style scoped>
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from, .fade-leave-to {
  opacity: 0;
}
</style>
