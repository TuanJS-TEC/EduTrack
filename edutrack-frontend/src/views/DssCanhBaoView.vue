<script setup>
import { ref, computed, onMounted } from 'vue'
import { AlertCircle, AlertTriangle, ShieldCheck, Download, Search, Navigation, RefreshCw } from 'lucide-vue-next'
import { apiService } from '../services/api'

// ── ECharts ──────────────────────────────────────────────────────────
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { BarChart } from 'echarts/charts'
import { TitleComponent, TooltipComponent, LegendComponent, GridComponent } from 'echarts/components'
use([CanvasRenderer, BarChart, TitleComponent, TooltipComponent, LegendComponent, GridComponent])

// ── State ─────────────────────────────────────────────────────────────
const search     = ref('')
const classFilter = ref('All')
const termFilter  = ref(1)
const classes    = ref([])
const students   = ref([])
const loading    = ref(false)

// ── Fetch danh sách lớp ───────────────────────────────────────────────
const fetchClasses = async () => {
  try {
    const res = await apiService.getLopHocs()
    classes.value = res.data
  } catch (error) {
    console.error('Lỗi lấy danh sách lớp:', error)
  }
}

// ── Fetch cảnh báo ────────────────────────────────────────────────────
const fetchWarnings = async () => {
  loading.value = true
  try {
    const maLop = classFilter.value === 'All' ? null : classFilter.value
    const res = await apiService.getDssCanhBao(termFilter.value, maLop, 5.0)

    // API trả PascalCase (do PropertyNamingPolicy = null trên backend)
    students.value = res.data.map(item => ({
      id:            item.MaHS,
      name:          item.HoTen,
      classId:       item.MaLop,
      subject:       item.TenMon,
      diemMieng:     item.DiemMieng,
      diem15p:       item.Diem15p,
      diemGiuaKy:    item.DiemGiuaKy,
      currentAvg:    item.DiemTBMon ?? null,
      requiredFinal: item.CkCanThiet,
      riskLevel:     item.MucDo,   // 'Do' | 'Vang' | 'Xanh'
    }))
  } catch (error) {
    console.error('Lỗi lấy cảnh báo:', error)
  } finally {
    loading.value = false
  }
}

// ── Filtered + sorted list ────────────────────────────────────────────
const atRiskStudents = computed(() => {
  const q = search.value.toLowerCase()
  return students.value
    .filter(s =>
      (s.name  || '').toLowerCase().includes(q) ||
      (s.subject || '').toLowerCase().includes(q)
    )
    .filter(s => s.riskLevel !== 'Xanh')   // chỉ hiện Đỏ + Vàng
    .sort((a, b) => {
      if (a.riskLevel === 'Do' && b.riskLevel !== 'Do') return -1
      if (a.riskLevel !== 'Do' && b.riskLevel === 'Do') return 1
      return (b.requiredFinal ?? 0) - (a.requiredFinal ?? 0)
    })
})

const totalRed    = computed(() => students.value.filter(s => s.riskLevel === 'Do').length)
const totalYellow = computed(() => students.value.filter(s => s.riskLevel === 'Vang').length)
const totalSafe   = computed(() => students.value.filter(s => s.riskLevel === 'Xanh').length)

// ── ECharts: Bar chart rủi ro theo lớp ───────────────────────────────
const barOption = computed(() => {
  if (!students.value.length) return {}

  // Group by classId
  const classMap = {}
  students.value.forEach(s => {
    if (!classMap[s.classId]) classMap[s.classId] = { Do: 0, Vang: 0, Xanh: 0 }
    classMap[s.classId][s.riskLevel] = (classMap[s.classId][s.riskLevel] || 0) + 1
  })
  const lopList = Object.keys(classMap).sort()

  return {
    tooltip: {
      trigger: 'axis',
      axisPointer: { type: 'shadow' },
      backgroundColor: 'rgba(15,23,42,0.88)',
      borderColor: 'rgba(255,255,255,0.1)',
      textStyle: { color: '#f1f5f9', fontSize: 12 }
    },
    legend: {
      data: ['Nguy cơ cao (Đỏ)', 'Lưu ý (Vàng)', 'An toàn (Xanh)'],
      bottom: 0,
      textStyle: { color: '#94a3b8', fontSize: 11, fontWeight: 'bold' },
      itemWidth: 10, itemHeight: 10,
    },
    grid: { left: 12, right: 12, top: 12, bottom: 40, containLabel: true },
    xAxis: {
      type: 'category',
      data: lopList,
      axisLabel: { color: '#94a3b8', fontWeight: 'bold', fontSize: 12 },
      axisLine: { lineStyle: { color: '#e2e8f0' } },
      axisTick: { show: false },
    },
    yAxis: {
      type: 'value', minInterval: 1,
      axisLabel: { color: '#94a3b8', fontSize: 10 },
      splitLine: { lineStyle: { color: '#f1f5f9', type: 'dashed' } },
    },
    series: [
      {
        name: 'Nguy cơ cao (Đỏ)',
        type: 'bar', stack: 'total', barMaxWidth: 60,
        itemStyle: { color: '#ef4444', borderRadius: [0, 0, 0, 0] },
        data: lopList.map(l => classMap[l]?.Do || 0),
        label: { show: true, position: 'inside', color: '#fff', fontWeight: 'bold', fontSize: 11,
          formatter: v => v.value > 0 ? v.value : '' }
      },
      {
        name: 'Lưu ý (Vàng)',
        type: 'bar', stack: 'total', barMaxWidth: 60,
        itemStyle: { color: '#f59e0b' },
        data: lopList.map(l => classMap[l]?.Vang || 0),
        label: { show: true, position: 'inside', color: '#fff', fontWeight: 'bold', fontSize: 11,
          formatter: v => v.value > 0 ? v.value : '' }
      },
      {
        name: 'An toàn (Xanh)',
        type: 'bar', stack: 'total', barMaxWidth: 60,
        itemStyle: { color: '#22c55e', borderRadius: [6, 6, 0, 0] },
        data: lopList.map(l => classMap[l]?.Xanh || 0),
        label: { show: true, position: 'inside', color: '#fff', fontWeight: 'bold', fontSize: 11,
          formatter: v => v.value > 0 ? v.value : '' }
      },
    ]
  }
})

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
        <p class="text-sm text-gray-400">Hệ thống Phát hiện sớm (Early Warning System) — tính trực tiếp từ bảng điểm.</p>
      </div>
      <button class="flex items-center gap-2 px-4 py-2 bg-white dark:bg-[#111C44] border border-gray-200 dark:border-white/10 text-gray-700 dark:text-gray-200 rounded-lg text-sm font-bold shadow-sm hover:bg-gray-50 dark:hover:bg-white/5 transition-colors">
        <Download :size="16" /> Xuất Danh Sách
      </button>
    </div>

    <!-- SUMMARY CARDS -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6">

      <!-- RED -->
      <div class="bg-red-50 dark:bg-red-500/10 rounded-2xl p-6 border border-red-200 dark:border-red-500/30 relative overflow-hidden group hover:shadow-lg transition-shadow">
        <div class="absolute -right-4 -bottom-4 opacity-10 group-hover:opacity-20 transition-opacity">
          <AlertCircle :size="120" class="text-red-600" />
        </div>
        <h3 class="text-sm font-extrabold text-red-600 dark:text-red-400 uppercase tracking-widest mb-1 flex items-center gap-1.5">
          <AlertCircle :size="16" /> Rủi ro cao (Đỏ)
        </h3>
        <p class="text-[11px] font-bold text-red-500/80 dark:text-red-400/80 mb-4">Cần CK &gt; 7.0 hoặc không thể cứu vãn</p>
        <div class="text-5xl font-black text-red-600 dark:text-red-400 leading-none">
          {{ totalRed }} <span class="text-sm font-bold">lượt môn</span>
        </div>
      </div>

      <!-- YELLOW -->
      <div class="bg-yellow-50 dark:bg-yellow-500/10 rounded-2xl p-6 border border-yellow-200 dark:border-yellow-500/30 relative overflow-hidden group hover:shadow-lg transition-shadow">
        <div class="absolute -right-4 -bottom-4 opacity-10 group-hover:opacity-20 transition-opacity">
          <AlertTriangle :size="120" class="text-yellow-600" />
        </div>
        <h3 class="text-sm font-extrabold text-yellow-600 dark:text-yellow-400 uppercase tracking-widest mb-1 flex items-center gap-1.5">
          <AlertTriangle :size="16" /> Lưu ý (Vàng)
        </h3>
        <p class="text-[11px] font-bold text-yellow-600/80 dark:text-yellow-400/80 mb-4">Cần CK &gt; 5.0 để đạt mục tiêu phẩy 5.0</p>
        <div class="text-5xl font-black text-yellow-600 dark:text-yellow-400 leading-none">
          {{ totalYellow }} <span class="text-sm font-bold">lượt môn</span>
        </div>
      </div>

      <!-- GREEN -->
      <div class="bg-emerald-50 dark:bg-emerald-500/10 rounded-2xl p-6 border border-emerald-200 dark:border-emerald-500/30 relative overflow-hidden group hover:shadow-lg transition-shadow">
        <div class="absolute -right-4 -bottom-4 opacity-10 group-hover:opacity-20 transition-opacity">
          <ShieldCheck :size="120" class="text-emerald-600" />
        </div>
        <h3 class="text-sm font-extrabold text-emerald-600 dark:text-emerald-400 uppercase tracking-widest mb-1 flex items-center gap-1.5">
          <ShieldCheck :size="16" /> An Toàn (Xanh)
        </h3>
        <p class="text-[11px] font-bold text-emerald-600/80 dark:text-emerald-400/80 mb-4">Điểm quá trình tốt, chỉ cần &lt; 5.0 là đủ phẩy.</p>
        <div class="text-5xl font-black text-emerald-600 dark:text-emerald-400 leading-none">
          {{ totalSafe }} <span class="text-sm font-bold">lượt môn</span>
        </div>
      </div>
    </div>

    <!-- ECHART: Rủi ro theo lớp -->
    <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 p-6">
      <div class="flex items-center justify-between mb-4">
        <div>
          <h3 class="text-base font-bold text-[#2B3674] dark:text-white">Phân bố Rủi ro Theo Lớp</h3>
          <p class="text-xs text-gray-400 mt-0.5">Số lượt môn học theo mức độ nguy cơ</p>
        </div>
        <button @click="fetchWarnings"
          class="p-1.5 rounded-lg text-gray-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-500/10 transition-colors">
          <RefreshCw :size="16" :class="{'animate-spin text-red-500': loading}" />
        </button>
      </div>

      <!-- Loading -->
      <div v-if="loading" class="h-[220px] flex items-center justify-center">
        <RefreshCw :size="32" class="animate-spin text-red-400 opacity-50" />
      </div>
      <!-- No data -->
      <div v-else-if="!students.length" class="h-[220px] flex flex-col items-center justify-center gap-2 border-2 border-dashed border-gray-100 dark:border-white/10 rounded-xl">
        <ShieldCheck :size="36" class="text-emerald-400 opacity-50" />
        <p class="text-sm text-gray-400 font-medium">Không có dữ liệu — thử điều chỉnh bộ lọc</p>
      </div>
      <!-- Chart -->
      <v-chart v-else :option="barOption" :style="{ height: '220px', width: '100%' }" :autoresize="true" />
    </div>

    <!-- MAIN TABLE -->
    <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 overflow-hidden">

      <!-- TOOLBAR -->
      <div class="p-5 border-b border-gray-100 dark:border-white/5 flex flex-wrap items-center justify-between gap-4 bg-gray-50/50 dark:bg-white/5">
        <div class="relative w-full sm:max-w-xs">
          <Search :size="18" class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
          <input
            v-model="search" type="text"
            placeholder="Tìm theo tên hoặc môn học..."
            class="w-full pl-10 pr-4 py-2 bg-white dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 rounded-lg text-sm font-medium focus:border-red-400 transition-all outline-none dark:text-white placeholder-gray-400 shadow-sm"
          />
        </div>
        <div class="flex items-center gap-3">
          <select v-model="termFilter" @change="fetchWarnings"
            class="appearance-none bg-white dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 text-gray-600 dark:text-gray-200 py-2.5 px-4 rounded-lg text-sm font-bold focus:outline-none cursor-pointer shadow-sm">
            <option :value="1">Học Kỳ 1</option>
            <option :value="2">Học Kỳ 2</option>
          </select>
          <select v-model="classFilter" @change="fetchWarnings"
            class="appearance-none bg-white dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 text-gray-600 dark:text-gray-200 py-2.5 px-4 rounded-lg text-sm font-bold focus:outline-none cursor-pointer shadow-sm">
            <option value="All">Toàn Trường</option>
            <option v-for="c in classes" :key="c.MaLop" :value="c.MaLop">Lớp {{ c.TenLop }}</option>
          </select>
          <button @click="fetchWarnings"
            class="p-2.5 bg-gray-100 dark:bg-white/5 text-gray-600 dark:text-gray-300 rounded-lg hover:bg-gray-200 dark:hover:bg-white/10 transition-colors">
            <RefreshCw :size="18" :class="{'animate-spin text-red-500': loading}" />
          </button>
        </div>
      </div>

      <!-- TABLE -->
      <div class="overflow-x-auto relative min-h-[300px]">
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
            <tr v-for="hs in atRiskStudents" :key="hs.id + hs.subject"
              class="border-b border-gray-50 dark:border-white/5 hover:bg-gray-50/50 dark:hover:bg-white/5 transition-colors">

              <td class="py-3 px-6">
                <div class="flex items-center gap-3">
                  <div :class="[
                    'w-8 h-8 rounded-full flex items-center justify-center text-[10px] font-black shrink-0',
                    hs.riskLevel === 'Do'
                      ? 'bg-red-100 dark:bg-red-500/20 text-red-500 dark:text-red-400'
                      : 'bg-yellow-100 dark:bg-yellow-500/20 text-yellow-600 dark:text-yellow-400'
                  ]">
                    {{ (hs.name || '?').charAt(0) }}
                  </div>
                  <div>
                    <h4 class="text-[13px] text-[#2B3674] dark:text-white leading-tight">{{ hs.name }}</h4>
                    <p class="text-[10px] text-gray-400 font-mono">{{ hs.id }}</p>
                  </div>
                </div>
              </td>

              <td class="py-3 px-4 text-center text-gray-500 dark:text-gray-400">{{ hs.classId }}</td>
              <td class="py-3 px-4 text-[#2B3674] dark:text-gray-300">{{ hs.subject }}</td>

              <td class="py-3 px-4 text-center">
                <span v-if="hs.currentAvg != null"
                  class="inline-flex items-center justify-center px-3 py-1.5 rounded-lg bg-gray-100 dark:bg-white/5 text-gray-600 dark:text-gray-300 border border-gray-200 dark:border-white/10 font-bold text-xs">
                  {{ Number(hs.currentAvg).toFixed(1) }}
                </span>
                <span v-else class="text-xs text-gray-400">—</span>
              </td>

              <td class="py-3 px-4 text-center">
                <div class="flex items-center justify-center gap-1.5 font-black text-base"
                  :class="hs.requiredFinal > 7 ? 'text-red-500 dark:text-red-400' : 'text-orange-500 dark:text-orange-400'">
                  <span>{{ hs.requiredFinal > 10 ? 'N/A' : Number(hs.requiredFinal).toFixed(1) }}</span>
                  <AlertTriangle v-if="hs.requiredFinal > 9" :size="14" />
                </div>
              </td>

              <td class="py-3 px-6 pl-4 text-right">
                <span v-if="hs.riskLevel === 'Do'"
                  class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-[11px] font-black uppercase tracking-wider bg-red-50 text-red-600 border border-red-200 dark:bg-red-500/10 dark:text-red-400 dark:border-red-500/30">
                  <AlertCircle :size="12" /> NGUY CƠ CAO
                </span>
                <span v-else-if="hs.riskLevel === 'Vang'"
                  class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-[11px] font-black uppercase tracking-wider bg-yellow-50 text-yellow-600 border border-yellow-200 dark:bg-yellow-500/10 dark:text-yellow-400 dark:border-yellow-500/30">
                  <AlertTriangle :size="12" /> LƯU Ý
                </span>
              </td>
            </tr>

            <!-- Empty state -->
            <tr v-if="atRiskStudents.length === 0 && !loading">
              <td colspan="6" class="py-16 text-center text-gray-400 dark:text-gray-500 font-bold">
                <ShieldCheck :size="48" class="mx-auto mb-3 text-emerald-500 opacity-50" />
                Tuyệt vời! Không có học sinh nào trong diện cảnh báo.
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
