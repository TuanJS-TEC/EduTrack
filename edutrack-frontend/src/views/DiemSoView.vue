<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { Download, Upload, Search, Filter, Hash, User, Bookmark, RefreshCcw, Save } from 'lucide-vue-next'
import { apiService } from '../services/api'

// Filter states
const search = ref('')
const classFilter = ref('')
const subjectFilter = ref('')
const termFilter = ref(1) // 1 hoặc 2

// // Mock data
// const mockClasses = [
//   { MaLop: '10A', TenLop: '10A' },
//   { MaLop: '10B', TenLop: '10B' },
//   { MaLop: '10C', TenLop: '10C' },
//   { MaLop: '11A', TenLop: '11A' },
//   { MaLop: '11B', TenLop: '11B' },
//   { MaLop: '12A', TenLop: '12A' },
//   { MaLop: '12B', TenLop: '12B' },
//   { MaLop: '9A', TenLop: '9A' },
// ]

// const mockSubjects = [
//   { MaMon: 'MON001', TenMon: 'Toán' },
//   { MaMon: 'MON002', TenMon: 'Vật Lý' },
//   { MaMon: 'MON003', TenMon: 'Hóa Học' },
//   { MaMon: 'MON004', TenMon: 'Tiếng Anh' },
//   { MaMon: 'MON005', TenMon: 'Ngữ Văn' },
// ]

// const mockBangDiem = {
//   '10A_MON001_1': [
//     { MaHS: 'HS001', HoTen: 'Trần Minh Quân', MaMon: 'MON001', HocKy: 1, DiemMieng: 8, Diem15p: 7.5, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, XepLoai: 'Khá' },
//     { MaHS: 'HS002', HoTen: 'Phạm Ngọc Linh', MaMon: 'MON001', HocKy: 1, DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7, DiemCuoiKy: 8, DiemTBMon: 7.6, XepLoai: 'Khá' },
//     { MaHS: 'HS003', HoTen: 'Lê Đức Hưng', MaMon: 'MON001', HocKy: 1, DiemMieng: 9, Diem15p: 8.5, DiemGiuaKy: 9, DiemCuoiKy: 9.5, DiemTBMon: 9.1, XepLoai: 'Giỏi' },
//     { MaHS: 'HS004', HoTen: 'Vương Thu Hà', MaMon: 'MON001', HocKy: 1, DiemMieng: 6.5, Diem15p: 6, DiemGiuaKy: 6.5, DiemCuoiKy: 7, DiemTBMon: 6.6, XepLoai: 'Khá' },
//     { MaHS: 'HS005', HoTen: 'Phan Huy Hoàng', MaMon: 'MON001', HocKy: 1, DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8, DiemTBMon: 8.0, XepLoai: 'Khá' },
//   ],
//   '10A_MON002_1': [
//     { MaHS: 'HS001', HoTen: 'Trần Minh Quân', MaMon: 'MON002', HocKy: 1, DiemMieng: 7, Diem15p: 7, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.6, XepLoai: 'Khá' },
//     { MaHS: 'HS002', HoTen: 'Phạm Ngọc Linh', MaMon: 'MON002', HocKy: 1, DiemMieng: 8, Diem15p: 8.5, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.6, XepLoai: 'Khá' },
//     { MaHS: 'HS003', HoTen: 'Lê Đức Hưng', MaMon: 'MON002', HocKy: 1, DiemMieng: 8.5, Diem15p: 8, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.6, XepLoai: 'Khá' },
//     { MaHS: 'HS004', HoTen: 'Vương Thu Hà', MaMon: 'MON002', HocKy: 1, DiemMieng: 7, Diem15p: 6, DiemGiuaKy: 6.5, DiemCuoiKy: 7, DiemTBMon: 6.6, XepLoai: 'Khá' },
//     { MaHS: 'HS005', HoTen: 'Phan Huy Hoàng', MaMon: 'MON002', HocKy: 1, DiemMieng: 7.5, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, XepLoai: 'Khá' },
//   ],
//   '10A_MON003_1': [
//     { MaHS: 'HS001', HoTen: 'Trần Minh Quân', MaMon: 'MON003', HocKy: 1, DiemMieng: 9, Diem15p: 8.5, DiemGiuaKy: 9, DiemCuoiKy: 9, DiemTBMon: 8.9, XepLoai: 'Giỏi' },
//     { MaHS: 'HS002', HoTen: 'Phạm Ngọc Linh', MaMon: 'MON003', HocKy: 1, DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, XepLoai: 'Khá' },
//     { MaHS: 'HS003', HoTen: 'Lê Đức Hưng', MaMon: 'MON003', HocKy: 1, DiemMieng: 8, Diem15p: 8.5, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.3, XepLoai: 'Khá' },
//     { MaHS: 'HS004', HoTen: 'Vương Thu Hà', MaMon: 'MON003', HocKy: 1, DiemMieng: 5, Diem15p: 5.5, DiemGiuaKy: 5, DiemCuoiKy: 5.5, DiemTBMon: 5.2, XepLoai: 'Trung Bình' },
//     { MaHS: 'HS005', HoTen: 'Phan Huy Hoàng', MaMon: 'MON003', HocKy: 1, DiemMieng: 8.5, Diem15p: 8, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.6, XepLoai: 'Khá' },
//   ],
//   '10A_MON004_1': [
//     { MaHS: 'HS001', HoTen: 'Trần Minh Quân', MaMon: 'MON004', HocKy: 1, DiemMieng: 8.5, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.3, XepLoai: 'Khá' },
//     { MaHS: 'HS002', HoTen: 'Phạm Ngọc Linh', MaMon: 'MON004', HocKy: 1, DiemMieng: 9, Diem15p: 9, DiemGiuaKy: 9, DiemCuoiKy: 9, DiemTBMon: 9.0, XepLoai: 'Giỏi' },
//     { MaHS: 'HS003', HoTen: 'Lê Đức Hưng', MaMon: 'MON004', HocKy: 1, DiemMieng: 7.5, Diem15p: 7, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.6, XepLoai: 'Khá' },
//     { MaHS: 'HS004', HoTen: 'Vương Thu Hà', MaMon: 'MON004', HocKy: 1, DiemMieng: 8, Diem15p: 7.5, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, XepLoai: 'Khá' },
//     { MaHS: 'HS005', HoTen: 'Phan Huy Hoàng', MaMon: 'MON004', HocKy: 1, DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, XepLoai: 'Khá' },
//   ],
//   '10A_MON005_1': [
//     { MaHS: 'HS001', HoTen: 'Trần Minh Quân', MaMon: 'MON005', HocKy: 1, DiemMieng: 7.5, Diem15p: 7, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.6, XepLoai: 'Khá' },
//     { MaHS: 'HS002', HoTen: 'Phạm Ngọc Linh', MaMon: 'MON005', HocKy: 1, DiemMieng: 8, Diem15p: 8, DiemGiuaKy: 8, DiemCuoiKy: 8.5, DiemTBMon: 8.1, XepLoai: 'Khá' },
//     { MaHS: 'HS003', HoTen: 'Lê Đức Hưng', MaMon: 'MON005', HocKy: 1, DiemMieng: 8.5, Diem15p: 8, DiemGiuaKy: 8.5, DiemCuoiKy: 9, DiemTBMon: 8.6, XepLoai: 'Khá' },
//     { MaHS: 'HS004', HoTen: 'Vương Thu Hà', MaMon: 'MON005', HocKy: 1, DiemMieng: 7, Diem15p: 7, DiemGiuaKy: 7, DiemCuoiKy: 7, DiemTBMon: 7.0, XepLoai: 'Khá' },
//     { MaHS: 'HS005', HoTen: 'Phan Huy Hoàng', MaMon: 'MON005', HocKy: 1, DiemMieng: 7, Diem15p: 7.5, DiemGiuaKy: 7.5, DiemCuoiKy: 8, DiemTBMon: 7.7, XepLoai: 'Khá' },
//   ],
// }

// Dropdown data
const classes = ref([])
const subjects = ref([])

// Table Data
const reportCards = ref([])
const loading = ref(false)
const saving = ref(false)
const updateCache = ref({}) // Để lưu các ID môn đang được edit trước khi Save

// Load initial dropdowns
const initFilters = async () => {
  try {
    const [classRes, subRes] = await Promise.all([
      apiService.getLopHocs(),
      apiService.getMonHocs()
    ])
    classes.value = classRes.data
    subjects.value = subRes.data
  } catch (error) {
    console.error("Lỗi tải filters:", error)
    // Use mock data as fallback
    classes.value = mockClasses
    subjects.value = mockSubjects
  }

  if (classes.value.length > 0) classFilter.value = classes.value[0].MaLop
  if (subjects.value.length > 0) subjectFilter.value = subjects.value[0].MaMon

  if (classFilter.value && subjectFilter.value) {
    await fetchBangDiem()
  }
}

// Fetch Bang Diem for matrix
const fetchBangDiem = async () => {
  if (!classFilter.value || !subjectFilter.value) return
  loading.value = true
  try {
    const res = await apiService.getBangDiem(classFilter.value, subjectFilter.value, termFilter.value)
    // deep clone so we can track changes
    reportCards.value = res.data.map(item => ({ ...item }))
    updateCache.value = {}
  } catch (error) {
    console.error("Lỗi lấy Bảng điểm:", error)
    // Use mock data as fallback
    const key = `${classFilter.value}_${subjectFilter.value}_${termFilter.value}`
    if (mockBangDiem[key]) {
      reportCards.value = mockBangDiem[key].map(item => ({ ...item }))
    } else {
      reportCards.value = []
    }
    updateCache.value = {}
  } finally {
    loading.value = false
  }
}

// Re-fetch when filter changes
watch([classFilter, subjectFilter, termFilter], () => {
  fetchBangDiem()
})

onMounted(() => {
  initFilters()
})

// UI Helpers
const gradeConfig = {
  'Giỏi': { color: 'bg-green-100 dark:bg-green-500/20 text-green-700 dark:text-green-400 border-green-200 dark:border-green-500/30' },
  'Khá': { color: 'bg-blue-100 dark:bg-blue-500/20 text-blue-700 dark:text-blue-400 border-blue-200 dark:border-blue-500/30' },
  'Trung Bình': { color: 'bg-yellow-100 dark:bg-yellow-500/20 text-yellow-700 dark:text-yellow-400 border-yellow-200 dark:border-yellow-500/30' },
  'Yếu': { color: 'bg-orange-100 dark:bg-orange-500/20 text-orange-700 dark:text-orange-400 border-orange-200 dark:border-orange-500/30' },
  'Kém': { color: 'bg-red-100 dark:bg-red-500/20 text-red-700 dark:text-red-400 border-red-200 dark:border-red-500/30' },
  'Chưa xếp loại': { color: 'bg-gray-100 dark:bg-white/10 text-gray-500 dark:text-gray-400 border-gray-200 dark:border-transparent' }
}

const getGradeColor = (loai) => {
  return gradeConfig[loai]?.color || gradeConfig['Chưa xếp loại'].color
}

const filteredReports = computed(() => {
  return reportCards.value.filter(s => {
    return (s.HoTen || '').toLowerCase().includes(search.value.toLowerCase()) || 
           (s.MaHS || '').toLowerCase().includes(search.value.toLowerCase())
  })
})

const classAverage = computed(() => {
  const validGrades = reportCards.value.filter(x => x.DiemTBMon !== null && x.DiemTBMon !== undefined)
  if (validGrades.length === 0) return 0
  const sum = validGrades.reduce((acc, student) => acc + student.DiemTBMon, 0)
  return (sum / validGrades.length).toFixed(1)
})

const getInitials = (name) => {
  if (!name) return 'HS'
  const parts = name.split(' ')
  return parts[parts.length - 1].charAt(0).toUpperCase()
}

// Mark student row as dirty when user edits
const markDirty = (student) => {
  updateCache.value[student.MaHS] = student
}

// Batch save using Upsert API
const saveChanges = async () => {
  const dsToSave = Object.values(updateCache.value)
  if (dsToSave.length === 0) return
  
  saving.value = true
  try {
    for (const data of dsToSave) {
      await apiService.upsertDiemSo({
        MaHS: data.MaHS,
        MaMon: data.MaMon,
        HocKy: data.HocKy,
        DiemMieng: data.DiemMieng,
        Diem15p: data.Diem15p,
        DiemGiuaKy: data.DiemGiuaKy,
        DiemCuoiKy: data.DiemCuoiKy
      })
    }
    // Refresh to get exactly calculated scores and grades
    await fetchBangDiem()
  } catch (error) {
    console.error("Lỗi khi lưu điểm:", error)
    alert("Có lỗi khi lưu bảng điểm. Vui lòng kiểm tra Console!")
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <div class="space-y-6">
    <!-- PAGE HEADER -->
    <div class="flex flex-wrap justify-between items-end gap-4">
      <div>
        <h2 class="text-2xl font-bold text-[#2B3674] dark:text-white mb-1">Bảng Điểm (Ma Trận)</h2>
        <p class="text-sm text-gray-400 dark:text-gray-400">Quản lý và cập nhật điểm số học sinh theo từng Lớp và Môn học.</p>
      </div>
      <div class="flex items-center gap-3">
        <button @click="fetchBangDiem" class="flex items-center gap-2 px-4 py-2 border border-blue-200 dark:border-blue-500/30 rounded-lg text-sm font-medium text-blue-600 dark:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-500/10 transition-colors bg-white dark:bg-[#111C44]">
          <RefreshCcw :size="16" :class="{ 'animate-spin': loading }" />
          Làm mới
        </button>
        <button 
          @click="saveChanges"
          :disabled="Object.keys(updateCache).length === 0 || saving"
          class="flex items-center gap-2 px-4 py-2 bg-[#1E88E5] hover:bg-blue-600 disabled:bg-gray-300 dark:disabled:bg-gray-700 disabled:cursor-not-allowed text-white rounded-lg text-sm font-bold transition-colors shadow-sm shadow-blue-500/30 dark:shadow-none"
        >
          <Save :size="16" :class="{ 'opacity-50': Object.keys(updateCache).length === 0 }" />
          {{ saving ? 'Đang lưu...' : (Object.keys(updateCache).length > 0 ? `Lưu Điểm (${Object.keys(updateCache).length})` : 'Lưu Điểm') }}
        </button>
      </div>
    </div>

    <!-- MAIN DASHBOARD CONTENT -->
    <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 flex flex-col min-h-[500px] relative">
      
      <!-- LOADING OVERLAY -->
      <div v-if="loading && reportCards.length === 0" class="absolute inset-0 bg-white/50 dark:bg-[#111C44]/50 z-20 flex items-center justify-center backdrop-blur-sm rounded-2xl">
        <RefreshCcw :size="32" class="animate-spin text-blue-500" />
      </div>

      <!-- TOOLBAR - ROW 1 -->
      <div class="p-5 border-b border-gray-100/50 dark:border-white/5 flex flex-wrap items-center justify-between gap-4">
        
        <div class="flex flex-wrap items-center gap-3">
          <div class="relative min-w-[140px]">
            <select v-model="classFilter" class="appearance-none w-full bg-blue-50 dark:bg-blue-500/10 border border-blue-100 dark:border-transparent text-[#1E88E5] dark:text-blue-400 py-2.5 pl-4 pr-10 rounded-xl text-sm font-extrabold focus:outline-none relative cursor-pointer outline-none transition-colors">
              <Hash :size="14" class="inline mr-1 -mt-0.5" />
              <option v-for="c in classes" :key="c.MaLop" :value="c.MaLop">Lớp {{ c.TenLop }}</option>
              <option v-if="classes.length === 0" value="">Chưa có Lớp</option>
            </select>
          </div>
          
          <div class="relative min-w-[140px]">
            <select v-model="subjectFilter" class="appearance-none w-full bg-gray-50 dark:bg-white/5 border border-gray-100 dark:border-transparent text-gray-600 dark:text-gray-200 py-2.5 pl-4 pr-10 rounded-xl text-sm font-bold focus:outline-none relative cursor-pointer outline-none transition-colors">
              <Bookmark :size="14" class="inline mr-1 -mt-0.5" />
              <option v-for="s in subjects" :key="s.MaMon" :value="s.MaMon">Môn {{ s.TenMon }}</option>
              <option v-if="subjects.length === 0" value="">Chưa có Môn</option>
            </select>
          </div>

          <div class="relative">
            <select v-model="termFilter" class="appearance-none bg-gray-50 dark:bg-white/5 border border-gray-100 dark:border-transparent text-gray-600 dark:text-gray-200 py-2.5 pl-4 pr-10 rounded-xl text-sm font-bold focus:outline-none relative cursor-pointer outline-none w-32 transition-colors">
              <option :value="1">Học Kỳ 1</option>
              <option :value="2">Học Kỳ 2</option>
            </select>
          </div>
        </div>

        <div class="flex items-center gap-4 bg-gray-50 dark:bg-white/5 px-4 py-2 rounded-xl border border-gray-100 dark:border-white/5">
          <div class="flex items-center gap-2 pr-4">
            <span class="text-xs font-bold text-gray-400 dark:text-gray-500">TRUNG BÌNH CỦA BẢNG ĐIỂM:</span>
            <span class="text-xl font-extrabold text-[#2B3674] dark:text-white">{{ classAverage }}</span>
          </div>
        </div>

      </div>

      <!-- SEARCH BAR ROW -->
      <div class="p-3 bg-gray-50/50 dark:bg-transparent border-b border-gray-100/50 dark:border-white/5 flex flex-wrap items-center justify-between gap-4">
        <div class="relative w-full max-w-sm ml-2">
          <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 dark:text-gray-500" />
          <input 
            v-model="search"
            type="text" 
            placeholder="Tìm học sinh..." 
            class="w-full pl-9 pr-4 py-1.5 bg-white dark:bg-[#111C44] border border-gray-200 dark:border-white/10 rounded-md text-sm font-medium focus:border-blue-500 focus:ring-1 focus:ring-blue-500/20 transition-all outline-none dark:text-white placeholder-gray-400 dark:placeholder-gray-500"
          />
        </div>

        <div class="flex items-center flex-wrap gap-2 text-[10px] font-bold text-gray-400 dark:text-gray-500 mr-2 uppercase tracking-wide">
          <span>Xếp Loại:</span>
          <span class="px-1.5 py-0.5 rounded" :class="gradeConfig['Giỏi'].color">Giỏi (≥8.0)</span>
          <span class="px-1.5 py-0.5 rounded" :class="gradeConfig['Khá'].color">Khá (≥6.5)</span>
          <span class="px-1.5 py-0.5 rounded" :class="gradeConfig['Trung Bình'].color">TB (≥5.0)</span>
          <span class="px-1.5 py-0.5 rounded" :class="gradeConfig['Yếu'].color">Yếu (≥3.5)</span>
          <span class="px-1.5 py-0.5 rounded" :class="gradeConfig['Kém'].color">Kém (<3.5)</span>
        </div>
      </div>

      <!-- GRADES MATRIX (Horizontal scroll enabled for complex tables) -->
      <div class="overflow-x-auto flex-1 pb-10">
        <table class="w-full text-center border-collapse whitespace-nowrap min-w-[800px]">
          <thead>
            <tr class="bg-gray-50 dark:bg-white/5 border-b border-gray-200 dark:border-white/10 text-[11px] font-bold text-[#2B3674] dark:text-white uppercase tracking-wider">
              <th class="py-4 px-6 text-left border-r border-gray-200 dark:border-white/10 w-64 shadow-sm">
                THÔNG TIN HỌC SINH
              </th>
              <th class="py-4 px-4 border-r border-gray-200 dark:border-white/10 w-28 text-center text-gray-500 dark:text-gray-300">
                Đ. MIỆNG
              </th>
              <th class="py-4 px-4 border-r border-gray-200 dark:border-white/10 w-28 text-center text-gray-500 dark:text-gray-300">
                Đ. 15 PHÚT
              </th>
              <th class="py-4 px-4 border-r border-gray-200 dark:border-white/10 w-32 text-center text-[#1E88E5] dark:text-blue-400 font-extrabold bg-blue-50/30 dark:bg-blue-500/5">
                ĐIỂM GIỮA KỲ
              </th>
              <th class="py-4 px-4 border-r border-gray-200 dark:border-white/10 w-32 text-center text-red-500 dark:text-red-400 font-extrabold bg-red-50/50 dark:bg-red-500/5">
                ĐIỂM CUỐI KỲ
              </th>
              <th class="py-4 px-6 bg-gradient-to-b from-transparent to-blue-50/50 dark:to-blue-500/5 w-48 shadow-[-2px_0_5px_rgba(0,0,0,0.02)]">
                <span class="text-[12px] font-extrabold">ĐIỂM TRUNG BÌNH</span>
              </th>
            </tr>
          </thead>
          <tbody class="text-sm font-bold">
            <tr v-for="student in filteredReports" :key="student.MaHS" class="border-b border-gray-100 dark:border-white/5 transition-colors group relative" :class="updateCache[student.MaHS] ? 'bg-orange-50/20 dark:bg-orange-500/5' : 'hover:bg-blue-50/30 dark:hover:bg-blue-500/5'">
              
              <!-- Left Frozen Column Header -->
              <td class="py-3 px-6 text-left border-r border-gray-100 dark:border-white/5 sticky left-0 z-10" :class="updateCache[student.MaHS] ? 'bg-orange-50/20 dark:bg-[#152044]' : 'bg-white group-hover:bg-[#f8fafc] dark:bg-[#111C44] dark:group-hover:bg-[#14234e]'">
                <div class="flex items-center gap-3">
                  <div class="w-8 h-8 rounded bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300 flex justify-center items-center text-xs">
                    {{ getInitials(student.HoTen) }}
                  </div>
                  <div>
                    <h4 class="text-[#2B3674] dark:text-white text-[13px] leading-tight group-hover:text-blue-600 dark:group-hover:text-blue-400 transition-colors">{{ student.HoTen }}</h4>
                    <span class="text-[10px] font-mono text-gray-400 dark:text-gray-500">{{ student.MaHS }}</span>
                  </div>
                  <div v-if="updateCache[student.MaHS]" class="w-2 h-2 rounded-full bg-orange-500 ml-auto mr-1" title="Unsaved Changes"></div>
                </div>
              </td>
              
              <!-- Editable Inputs -->
              <td class="border-r border-gray-100 dark:border-white/10 p-2 align-middle">
                <input 
                  type="number" step="0.1" min="0" max="10"
                  v-model.number="student.DiemMieng" 
                  @input="markDirty(student)"
                  class="w-full h-10 text-center bg-gray-50 dark:bg-[#0B1437] border-2 border-transparent focus:border-blue-500 rounded-lg text-sm font-bold focus:bg-white dark:focus:bg-[#111C44] transition-all outline-none"
                  :class="student.DiemMieng !== null ? 'text-gray-700 dark:text-gray-200' : 'text-gray-300 dark:text-gray-600'"
                />
              </td>
              <td class="border-r border-gray-100 dark:border-white/10 p-2 align-middle">
                <input 
                  type="number" step="0.1" min="0" max="10"
                  v-model.number="student.Diem15p" 
                  @input="markDirty(student)"
                  class="w-full h-10 text-center bg-gray-50 dark:bg-[#0B1437] border-2 border-transparent focus:border-blue-500 rounded-lg text-sm font-bold focus:bg-white dark:focus:bg-[#111C44] transition-all outline-none"
                  :class="student.Diem15p !== null ? 'text-gray-700 dark:text-gray-200' : 'text-gray-300 dark:text-gray-600'"
                />
              </td>

              <!-- Major Exams Matrix -->
              <td class="border-r border-gray-100 dark:border-white/10 p-2 align-middle bg-blue-50/10 dark:bg-blue-500/[0.02]">
                <input 
                  type="number" step="0.1" min="0" max="10"
                  v-model.number="student.DiemGiuaKy" 
                  @input="markDirty(student)"
                  class="w-full h-10 text-center bg-transparent border-2 border-dashed border-blue-200 dark:border-blue-500/30 rounded-lg text-sm font-extrabold focus:border-blue-500 focus:bg-white dark:focus:bg-[#111C44] transition-all outline-none"
                  :class="student.DiemGiuaKy !== null ? 'text-[#1E88E5] dark:text-blue-400' : 'text-gray-400 dark:text-gray-500'"
                />
              </td>
              <td class="border-r border-gray-100 dark:border-white/10 p-2 align-middle bg-red-50/30 dark:bg-red-500/[0.05]">
                <input 
                  type="number" step="0.1" min="0" max="10"
                  v-model.number="student.DiemCuoiKy" 
                  @input="markDirty(student)"
                  class="w-full h-10 text-center bg-transparent border-2 border-solid border-red-200 dark:border-red-500/30 rounded-lg text-sm font-extrabold focus:border-red-500 focus:bg-white dark:focus:bg-[#111C44] transition-all outline-none"
                  :class="student.DiemCuoiKy !== null ? 'text-red-600 dark:text-red-400' : 'text-gray-400 dark:text-gray-500'"
                />
              </td>

              <!-- TOTAL AVG & RATING -->
              <td class="bg-gradient-to-b from-transparent to-blue-50/30 dark:to-blue-500/[0.05] px-4 py-0 border-l border-transparent dark:border-white/5 relative shadow-[-2px_0_5px_rgba(0,0,0,0.01)] align-middle">
                <div class="flex items-center justify-between gap-2">
                  <div class="flex flex-col items-start leading-tight">
                    <span class="text-xs text-gray-400 dark:text-gray-500 font-medium font-sans">TBM</span>
                    <span class="text-xl font-extrabold" :class="student.DiemTBMon !== null ? 'text-[#2B3674] dark:text-white' : 'text-gray-300 dark:text-gray-600'">
                      {{ student.DiemTBMon !== null ? student.DiemTBMon.toFixed(1) : '-' }}
                    </span>
                  </div>
                  <span 
                    :class="['px-2 py-1 flex justify-center items-center rounded text-xs font-bold border shadow-sm w-full max-w-[80px]', getGradeColor(student.XepLoai)]"
                  >
                    {{ student.XepLoai || 'Trống' }}
                  </span>
                </div>
                <!-- Dynamic Progress Line -->
                <div class="absolute bottom-0 left-0 h-1 bg-gray-100 dark:bg-gray-700 w-full opacity-60">
                  <div class="h-full transition-all duration-300 rounded-r-full" 
                       :class="(student.XepLoai === 'Kém' || student.XepLoai === 'Yếu') ? 'bg-red-500' : (student.XepLoai === 'Giỏi' ? 'bg-green-500' : 'bg-[#1E88E5]')"
                       :style="`width: ${student.DiemTBMon ? (student.DiemTBMon * 10) : 0}%`"></div>
                </div>
              </td>
            </tr>

            <!-- Empty State -->
            <tr v-if="filteredReports.length === 0 && !loading">
              <td colspan="6" class="py-16 text-center text-gray-400 dark:text-gray-500 bg-gray-50/50 dark:bg-transparent">
                <p class="font-bold text-base mb-1">Không có học sinh trong lớp/môn này</p>
                <p class="text-xs font-medium">Hoặc bạn chưa đổi sang bộ lọc phù hợp.</p>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
