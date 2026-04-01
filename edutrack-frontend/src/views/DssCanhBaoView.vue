<script setup>
import { ref, computed, onMounted } from 'vue'
import { AlertCircle, AlertTriangle, ShieldCheck, Download, Search, Navigation, RefreshCw } from 'lucide-vue-next'
import { apiService } from '../services/api'

const search = ref('')
const classFilter = ref('All')
const termFilter = ref(1)

const classes = ref([])
const students = ref([])
const loading = ref(false)
const totalSafeStudents = ref(620) // Mocked for now unless we implement dashboard-hoc-luc combo

// Fetch classes
const fetchClasses = async () => {
  try {
    const res = await apiService.getLopHocs()
    classes.value = res.data
  } catch (error) {
    console.error("Lỗi lấy danh sách lớp:", error)
  }
}

// Fetch warnings
const fetchWarnings = async () => {
  loading.value = true
  try {
    // Nếu chọn 'All' thì ko pass maLop
    const maLop = classFilter.value === 'All' ? null : classFilter.value
    const res = await apiService.getDssCanhBao(termFilter.value, maLop, 5.0)
    
    // Map backend data to frontend model
    students.value = res.data.map(item => {
      return {
        id: item.MaHS,
        name: item.HoTen,
        classId: item.MaLop,
        subject: item.TenMon,
        currentAvg: item.DiemTBMon, // null nếu chưa có data
        requiredFinal: item.CkCanThiet,
        riskLevel: (item.MucDo || '').toLowerCase() // "do", "vang", "xanh"
      }
    })

  } catch (error) {
    console.error("Lỗi lấy cảnh báo:", error)
  } finally {
    loading.value = false
  }
}

const atRiskStudents = computed(() => {
  return students.value
    .filter(s => s.name.toLowerCase().includes(search.value.toLowerCase()) || s.subject.toLowerCase().includes(search.value.toLowerCase()))
    .sort((a, b) => {
      // Sort Red first, then Yellow
      if (a.riskLevel === 'do' && b.riskLevel !== 'do') return -1
      if (a.riskLevel !== 'do' && b.riskLevel === 'do') return 1
      return b.requiredFinal - a.requiredFinal
    })
})

// Export to CSV
const exportToCSV = () => {
  if (atRiskStudents.value.length === 0) {
    alert('Không có dữ liệu để xuất!')
    return
  }
  
  // CSV headers
  const headers = ['Mã HS', 'Tên Học Sinh', 'Lớp', 'Môn Học', 'Quá Trình TB', 'Cuối Kỳ Cần ≥', 'Mức Độ']
  
  // CSV rows
  const rows = atRiskStudents.value.map(hs => [
    hs.id,
    hs.name,
    hs.classId,
    hs.subject,
    hs.currentAvg ? hs.currentAvg.toFixed(1) : '-',
    hs.requiredFinal > 10 ? 'N/A' : hs.requiredFinal.toFixed(1),
    hs.riskLevel === 'do' ? 'NGUY CƠ CAO' : hs.riskLevel === 'vang' ? 'RỦI RO' : 'AN TOÀN'
  ])
  
  // Combine headers and rows
  const csv = [
    headers.join(','),
    ...rows.map(row => row.map(cell => `"${cell}"`).join(','))
  ].join('\n')
  
  // Add BOM for proper UTF-8 encoding in Excel
  const BOM = '\uFEFF'
  const blob = new Blob([BOM + csv], { type: 'text/csv;charset=utf-8;' })
  
  // Create download link
  const link = document.createElement('a')
  const url = URL.createObjectURL(blob)
  link.setAttribute('href', url)
  link.setAttribute('download', `CanhBao_RoiMon_${new Date().toISOString().split('T')[0]}.csv`)
  link.style.visibility = 'hidden'
  
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}

onMounted(async () => {
  await fetchClasses()
  await fetchWarnings()
})

</script>

<template>
  <div class="space-y-6">
    <!-- PAGE HEADER -->
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-end gap-4">
      <div>
        <h2 class="text-2xl font-bold text-[#2B3674] dark:text-white mb-1 flex items-center gap-2">
          <Navigation class="text-red-500" :size="24" />
          Radar Cảnh Báo Học Tập
        </h2>
        <p class="text-sm text-gray-400 dark:text-gray-400">Hệ thống Phát hiện sớm (Early Warning System) lấy dữ liệu trực tiếp từ Bảng Điểm hệ thống.</p>
      </div>
      <button @click="exportToCSV" class="flex items-center gap-2 px-4 py-2 bg-white dark:bg-[#111C44] border border-gray-200 dark:border-white/10 text-gray-700 dark:text-gray-200 rounded-lg text-sm font-bold shadow-sm hover:bg-gray-50 dark:hover:bg-white/5 transition-colors">
        <Download :size="16" /> Xuất Danh Sách
      </button>
    </div>

    <!-- SUMMARY CARDS -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
      
      <!-- RED ALERT -->
      <div class="bg-red-50 dark:bg-red-500/10 rounded-2xl p-6 border border-red-200 dark:border-red-500/30 flex justify-between items-center relative overflow-hidden group hover:shadow-lg transition-shadow">
        <div class="absolute -right-4 -bottom-4 opacity-10 group-hover:opacity-20 transition-opacity">
          <AlertCircle :size="120" class="text-red-600 dark:text-red-400" />
        </div>
        <div>
          <h3 class="text-sm font-extrabold text-red-600 dark:text-red-400 uppercase tracking-widest mb-1 flex items-center gap-1.5">
            <AlertCircle :size="16" /> Rủi ro cao (Đỏ)
          </h3>
          <p class="text-[11px] font-bold text-red-500/80 dark:text-red-400/80 mb-4 max-w-[200px]">Bắt buộc thi CK > 7.0 hoặc không thể cứu vãn</p>
          <div class="text-5xl font-black text-red-600 dark:text-red-400 leading-none">{{ totalRed }} <span class="text-sm">Mốc</span></div>
        </div>
      </div>

      <!-- YELLOW ALERT -->
      <div class="bg-yellow-50 dark:bg-yellow-500/10 rounded-2xl p-6 border border-yellow-200 dark:border-yellow-500/30 flex justify-between items-center relative overflow-hidden group hover:shadow-lg transition-shadow">
        <div class="absolute -right-4 -bottom-4 opacity-10 group-hover:opacity-20 transition-opacity">
          <AlertTriangle :size="120" class="text-yellow-600 dark:text-yellow-400" />
        </div>
        <div>
          <h3 class="text-sm font-extrabold text-yellow-600 dark:text-yellow-400 uppercase tracking-widest mb-1 flex items-center gap-1.5">
            <AlertTriangle :size="16" /> Lưu ý (Vàng)
          </h3>
          <p class="text-[11px] font-bold text-yellow-600/80 dark:text-yellow-400/80 mb-4 max-w-[200px]">Bắt buộc thi CK > 5.0 để đạt mục tiêu phẩy 5.0</p>
          <div class="text-5xl font-black text-yellow-600 dark:text-yellow-400 leading-none">{{ totalYellow }} <span class="text-sm">Mốc</span></div>
        </div>
      </div>

      <!-- GREEN SAFE -->
      <div class="bg-emerald-50 dark:bg-emerald-500/10 rounded-2xl p-6 border border-emerald-200 dark:border-emerald-500/30 flex justify-between items-center relative overflow-hidden group hover:shadow-lg transition-shadow">
        <div class="absolute -right-4 -bottom-4 opacity-10 group-hover:opacity-20 transition-opacity">
          <ShieldCheck :size="120" class="text-emerald-600 dark:text-emerald-400" />
        </div>
        <div>
          <h3 class="text-sm font-extrabold text-emerald-600 dark:text-emerald-400 uppercase tracking-widest mb-1 flex items-center gap-1.5">
            <ShieldCheck :size="16" /> An Toàn (Xanh)
          </h3>
          <p class="text-[11px] font-bold text-emerald-600/80 dark:text-emerald-400/80 mb-4 max-w-[200px]">Điểm quá trình tốt. Chỉ cần < 5.0 là đủ phẩy.</p>
          <div class="text-2xl font-black text-emerald-600 dark:text-emerald-400 leading-none mt-2">Đa số <span class="text-sm">Học sinh</span></div>
        </div>
      </div>

    </div>

    <!-- MAIN TABLE UI -->
    <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 overflow-hidden">
      
      <!-- TOOLBAR -->
      <div class="p-5 border-b border-gray-100 dark:border-white/5 flex flex-wrap items-center justify-between gap-4 bg-gray-50/50 dark:bg-white/5">
        <div class="relative w-full sm:max-w-xs">
          <Search :size="18" class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 dark:text-gray-500" />
          <input 
            v-model="search"
            type="text" 
            placeholder="Tìm theo tên hoặc môn học..." 
            class="w-full pl-10 pr-4 py-2 bg-white dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 rounded-lg text-sm font-medium focus:border-red-400 focus:ring-1 focus:ring-red-400/20 transition-all outline-none dark:text-white placeholder-gray-400 dark:placeholder-gray-500 shadow-sm"
          />
        </div>
        
        <div class="flex items-center gap-3">
          <select v-model="termFilter" @change="fetchWarnings" class="appearance-none bg-white dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 text-gray-600 dark:text-gray-200 py-2.5 px-4 rounded-lg text-sm font-bold focus:outline-none focus:border-red-400 relative cursor-pointer outline-none shadow-sm transition-colors">
            <option :value="1">Học Kỳ 1</option>
            <option :value="2">Học Kỳ 2</option>
          </select>
          <select v-model="classFilter" @change="fetchWarnings" class="appearance-none bg-white dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 text-gray-600 dark:text-gray-200 py-2.5 px-4 rounded-lg text-sm font-bold focus:outline-none focus:border-red-400 relative cursor-pointer outline-none shadow-sm transition-colors">
            <option value="All">Toàn Trường</option>
            <option v-for="c in classes" :key="c.MaLop" :value="c.MaLop">Lớp {{ c.TenLop }}</option>
          </select>
          <button @click="fetchWarnings" class="p-2.5 bg-gray-100 dark:bg-white/5 text-gray-600 dark:text-gray-300 rounded-lg hover:bg-gray-200 dark:hover:bg-white/10 transition-colors">
            <RefreshCw :size="18" :class="{'animate-spin text-blue-500': loading}" />
          </button>
        </div>
      </div>

      <!-- THE TABLE -->
      <div class="overflow-x-auto relative min-h-[400px]">
        
        <!-- Loading overlay -->
        <div v-if="loading" class="absolute inset-0 bg-white/50 dark:bg-[#111C44]/50 backdrop-blur-sm z-10 flex items-center justify-center">
            <RefreshCw class="animate-spin text-red-500" :size="32" />
        </div>

        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="text-[11px] font-bold text-gray-400 uppercase tracking-widest border-b border-gray-100 dark:border-white/5 bg-gray-50/20 dark:bg-white/[0.02]">
              <th class="py-4 px-6">Học Sinh</th>
              <th class="py-4 px-4 text-center">Lớp</th>
              <th class="py-4 px-4">Môn Học</th>
              <th class="py-4 px-4 text-center">Quá trình TB</th>
              <th class="py-4 px-4 text-center">Cuối Kỳ Cần ≥</th>
              <th class="py-4 px-6 pl-4 text-right">Mức Độ</th>
            </tr>
          </thead>
          <tbody class="text-sm font-bold">
            <tr v-for="hs in atRiskStudents" :key="hs.id + hs.subject" class="border-b border-gray-50 dark:border-white/5 hover:bg-gray-50/50 dark:hover:bg-white/5 transition-colors">
              <td class="py-3 px-6">
                <div class="flex items-center gap-3">
                  <div class="w-8 h-8 rounded-full bg-red-100 dark:bg-red-500/20 text-red-500 dark:text-red-400 flex items-center justify-center text-[10px] font-black shrink-0">
                    {{ hs.name.charAt(0) }}
                  </div>
                  <div>
                    <h4 class="text-[13px] text-[#2B3674] dark:text-white leading-tight opacity-90">{{ hs.name }}</h4>
                    <p class="text-[10px] text-gray-400 font-mono">{{ hs.id }}</p>
                  </div>
                </div>
              </td>
              <td class="py-3 px-4 text-center text-gray-500 dark:text-gray-400">{{ hs.classId }}</td>
              <td class="py-3 px-4 text-[#2B3674] dark:text-gray-300">{{ hs.subject }}</td>
              <td class="py-3 px-4 text-center">
                <span class="inline-flex items-center justify-center px-3 py-1.5 rounded-lg bg-gray-100 dark:bg-white/5 text-gray-600 dark:text-gray-300 border border-gray-200 dark:border-white/10 font-bold text-xs" v-if="hs.currentAvg">
                  {{ hs.currentAvg.toFixed(1) }}
                </span>
                <span v-else class="text-xs text-gray-400">-</span>
              </td>
              <td class="py-3 px-4 text-center">
                <div class="flex items-center justify-center gap-1.5 font-black text-base" :class="hs.requiredFinal > 7 ? 'text-red-500 dark:text-red-400' : 'text-orange-500 dark:text-orange-400'">
                  <span>{{ hs.requiredFinal > 10 ? 'N/A' : hs.requiredFinal.toFixed(1) }}</span>
                  <AlertTriangle v-if="hs.requiredFinal > 9" :size="14" />
                </div>
              </td>
              <td class="py-3 px-6 pl-4 text-right">
                <span v-if="hs.riskLevel === 'do'" class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-[11px] font-black uppercase tracking-wider bg-red-50 text-red-600 border border-red-200 dark:bg-red-500/10 dark:text-red-400 dark:border-red-500/30">
                  <AlertCircle :size="12" /> NGUY CƠ CAO
                </span>
                <span v-else-if="hs.riskLevel === 'vang'" class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-[11px] font-black uppercase tracking-wider bg-yellow-50 text-yellow-600 border border-yellow-200 dark:bg-yellow-500/10 dark:text-yellow-400 dark:border-yellow-500/30">
                  <AlertTriangle :size="12" /> RỦI RO
                </span>
              </td>
            </tr>
            <tr v-if="atRiskStudents.length === 0 && !loading">
              <td colspan="6" class="py-16 text-center text-gray-400 dark:text-gray-500 font-bold">
                <ShieldCheck :size="48" class="mx-auto mb-3 text-emerald-500 opacity-50" />
                Tuyệt vời! Không tìm thấy dữ liệu báo động học tập cho phạm vi này.
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
