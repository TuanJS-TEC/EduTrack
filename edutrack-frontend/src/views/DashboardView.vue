<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { Users, BookOpen, UserSquare, TrendingUp, ArrowUpRight, Calendar, RefreshCw, ChevronDown } from 'lucide-vue-next'
import { apiService } from '../services/api'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const auth = useAuthStore()

/** Phím tắt dashboard → đúng route; nút tắt nếu không đủ quyền (trùng meta router). */
const canQuickHocSinh = computed(() => auth.hasPermission('Students.View'))
const canQuickGiaoVien = computed(() => auth.hasPermission('Teachers.View'))
const canQuickLopLich = computed(() => auth.isAdmin || auth.isBGH || auth.isTeacher)

function goQuick(path) {
  router.push(path)
}

// ── ECharts ──────────────────────────────────────────────────────────
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { PieChart, BarChart } from 'echarts/charts'
import {
  TitleComponent, TooltipComponent, LegendComponent,
  GridComponent, DatasetComponent
} from 'echarts/components'

use([
  CanvasRenderer, PieChart, BarChart,
  TitleComponent, TooltipComponent, LegendComponent,
  GridComponent, DatasetComponent
])

// ── Kỳ học & Năm học động ─────────────────────────────────────────────
/** Năm học detect từ dữ liệu LopHoc thực tế (không hardcode). */
const namHoc = ref('2025-2026')
const selectedHocKy = ref(1)
const loadingMeta = ref(false)

const fetchNamHoc = async () => {
  loadingMeta.value = true
  try {
    const res = await apiService.getLopHocs()
    const lops = res.data ?? []
    if (lops.length > 0 && lops[0].NamHoc) {
      namHoc.value = lops[0].NamHoc
    }
  } catch (e) {
    console.warn('Không lấy được năm học từ lớp, dùng mặc định:', e)
  } finally {
    loadingMeta.value = false
  }
}

// ── Dữ liệu từ API ───────────────────────────────────────────────────
const hocLuc = ref(null)
const loadingChart = ref(false)

const fetchDashboard = async () => {
  loadingChart.value = true
  try {
    const res = await apiService.getDssThongKeHocLuc(selectedHocKy.value, namHoc.value)
    hocLuc.value = res.data
  } catch (e) {
    console.error('Lỗi tải dashboard học lực:', e)
  } finally {
    loadingChart.value = false
  }
}

/** Label kỳ học hiện tại hiển thị trên UI */
const kyHocLabel = computed(() => `Học kỳ ${selectedHocKy.value} · Năm học ${namHoc.value}`)

onMounted(async () => {
  await fetchNamHoc()
  await fetchDashboard()
})

/** Khi đổi kỳ → tự reload chart */
watch(selectedHocKy, fetchDashboard)

// ── ECharts options ───────────────────────────────────────────────────
// Biểu đồ Tròn: Phân bố xếp loại
const pieOption = computed(() => {
  if (!hocLuc.value) return {}
  const d = hocLuc.value
  return {
    tooltip: {
      trigger: 'item',
      formatter: '{b}: {c} HS ({d}%)',
      backgroundColor: 'rgba(15,23,42,0.85)',
      borderColor: 'rgba(255,255,255,0.1)',
      textStyle: { color: '#f1f5f9', fontSize: 12 }
    },
    legend: {
      orient: 'vertical',
      right: 10,
      top: 'center',
      textStyle: { color: '#94a3b8', fontSize: 11, fontWeight: 'bold' },
      itemWidth: 10,
      itemHeight: 10,
    },
    series: [{
      name: 'Xếp loại',
      type: 'pie',
      radius: ['42%', '72%'],
      center: ['40%', '50%'],
      avoidLabelOverlap: true,
      itemStyle: { borderRadius: 6, borderColor: 'transparent', borderWidth: 2 },
      label: { show: false },
      emphasis: {
        label: { show: true, fontSize: 13, fontWeight: 'bold', color: '#1e293b' },
        itemStyle: { shadowBlur: 10, shadowOffsetX: 0, shadowColor: 'rgba(0,0,0,0.3)' }
      },
      data: [
        { value: d.Gioi,      name: 'Giỏi ≥8.0',      itemStyle: { color: '#22c55e' } },
        { value: d.Kha,       name: 'Khá 6.5-7.9',    itemStyle: { color: '#3b82f6' } },
        { value: d.TrungBinh, name: 'TB 5.0-6.4',     itemStyle: { color: '#f59e0b' } },
        { value: d.Yeu,       name: 'Yếu 3.5-4.9',    itemStyle: { color: '#f97316' } },
        { value: d.Kem,       name: 'Kém <3.5',        itemStyle: { color: '#ef4444' } },
      ].filter(i => i.value > 0)
    }]
  }
})

// Biểu đồ Cột: Điểm TB theo lớp
const barOption = computed(() => {
  if (!hocLuc.value?.TheoLop?.length) return {}
  const lops = hocLuc.value.TheoLop
  return {
    tooltip: {
      trigger: 'axis',
      backgroundColor: 'rgba(15,23,42,0.85)',
      borderColor: 'rgba(255,255,255,0.1)',
      textStyle: { color: '#f1f5f9', fontSize: 12 },
      formatter: (params) => {
        const p = params[0]
        return `<b>${p.name}</b><br/>Điểm TB: <b style="color:#60a5fa">${p.value ?? 'N/A'}</b>`
      }
    },
    grid: { left: 16, right: 16, top: 20, bottom: 36 },
    xAxis: {
      type: 'category',
      data: lops.map(l => l.TenLop),
      axisLabel: { color: '#94a3b8', fontWeight: 'bold', fontSize: 11 },
      axisLine: { lineStyle: { color: '#e2e8f0' } },
      axisTick: { show: false }
    },
    yAxis: {
      type: 'value',
      min: 0, max: 10,
      splitNumber: 5,
      axisLabel: { color: '#94a3b8', fontSize: 10 },
      splitLine: { lineStyle: { color: '#f1f5f9', type: 'dashed' } }
    },
    series: [{
      type: 'bar',
      data: lops.map(l => ({
        value: l.TbChung != null && l.TbChung !== undefined ? Number(Number(l.TbChung).toFixed(1)) : null,
        itemStyle: {
          color: l.TbChung == null || l.TbChung === undefined
            ? '#cbd5e1'
            : l.TbChung >= 8 ? '#22c55e'
              : l.TbChung >= 6.5 ? '#3b82f6'
                : l.TbChung >= 5 ? '#f59e0b'
                  : '#ef4444',
          borderRadius: [6, 6, 0, 0]
        }
      })),
      barMaxWidth: 48,
      label: {
        show: true,
        position: 'top',
        formatter: (p) => (p.value != null && p.value !== '' ? p.value : '—'),
        color: '#475569',
        fontWeight: 'bold',
        fontSize: 11
      }
    }]
  }
})

// Stats tổng hợp
const stats = computed(() => {
  const d = hocLuc.value
  const total = d?.TongHocSinh ?? 0
  return [
    {
      id: 1, title: 'TỔNG HỌC SINH',
      value: d ? d.TongHocSinh.toString() : '—',
      trend: d ? `${d.Gioi + d.Kha} Giỏi/Khá` : '...',
      trendUp: true, icon: Users, iconBg: 'bg-blue-50 text-blue-500',
    },
    {
      id: 2, title: 'HỌC SINH GIỎI',
      value: d ? d.Gioi.toString() : '—',
      trend: d ? `${total > 0 ? Math.round(d.Gioi * 100 / total) : 0}% tổng số` : '...',
      trendUp: true, icon: BookOpen, iconBg: 'bg-teal-50 text-teal-500',
    },
    {
      id: 3, title: 'CẦN QUAN TÂM',
      value: d ? (d.Yeu + d.Kem).toString() : '—',
      trend: d && (d.Yeu + d.Kem) > 0 ? 'Yếu + Kém' : 'Tốt lắm!',
      trendUp: !(d && (d.Yeu + d.Kem) > 0), icon: UserSquare, iconBg: 'bg-red-50 text-red-400',
    },
    {
      id: 4, title: 'HỌC SINH KHÁ',
      value: d ? d.Kha.toString() : '—',
      trend: d ? `${total > 0 ? Math.round(d.Kha * 100 / total) : 0}% tổng số` : '...',
      trendUp: true, icon: TrendingUp, iconBg: 'bg-indigo-50 text-indigo-500',
    },
  ]
})
</script>


<template>
  <div class="space-y-6">
    <!-- TOP STAT CARDS (4 Cards) -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
      <div
        v-for="stat in stats" :key="stat.id"
        class="bg-white dark:bg-[#111C44] rounded-2xl p-6 shadow-sm flex items-start justify-between border border-gray-100/50 dark:border-white/5 hover:shadow-md transition-shadow"
      >
        <div>
          <p class="text-xs font-bold text-gray-400 dark:text-gray-400 tracking-wider mb-2">{{ stat.title }}</p>
          <h3 class="text-3xl font-extrabold text-[#2B3674] dark:text-white">{{ stat.value }}</h3>
          <div class="flex items-center mt-2 text-sm">
            <span :class="stat.trendUp ? 'text-green-500 font-bold flex items-center' : 'text-red-500 font-bold flex items-center'">
              <ArrowUpRight v-if="stat.trendUp" :size="16" class="mr-0.5" />
              {{ stat.trend }}
            </span>
          </div>
        </div>
        <div :class="['w-12 h-12 rounded-full flex items-center justify-center', stat.iconBg]">
          <component :is="stat.icon" :size="24" stroke-width="2" />
        </div>
      </div>
    </div>

    <!-- CHARTS ROW -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">

      <!-- BIỂU ĐỒ CỘT: Điểm TB theo Lớp (chiếm 2/3) -->
      <div class="bg-white dark:bg-[#111C44] rounded-2xl p-6 shadow-sm border border-gray-100/50 dark:border-white/5 lg:col-span-2">
        <div class="flex justify-between items-start mb-4">
          <div>
            <h3 class="text-lg font-bold text-[#2B3674] dark:text-white">Điểm Trung Bình Theo Lớp</h3>
            <p class="text-sm text-gray-400 dark:text-gray-400">{{ kyHocLabel }}</p>
          </div>
          <div class="flex items-center gap-2">
            <!-- Selector kỳ học -->
            <div class="flex rounded-lg border border-gray-200 dark:border-white/10 overflow-hidden text-xs font-bold">
              <button
                v-for="ky in [1, 2]" :key="ky"
                @click="selectedHocKy = ky"
                :class="[
                  'px-3 py-1.5 transition-colors',
                  selectedHocKy === ky
                    ? 'bg-blue-500 text-white'
                    : 'bg-white dark:bg-[#0B1437] text-gray-500 dark:text-gray-400 hover:bg-gray-50 dark:hover:bg-white/5'
                ]"
              >HK{{ ky }}</button>
            </div>
            <button @click="fetchDashboard"
              class="p-1.5 rounded-lg text-gray-400 hover:text-blue-500 hover:bg-blue-50 dark:hover:bg-blue-500/10 transition-colors">
              <RefreshCw :size="16" :class="{'animate-spin text-blue-500': loadingChart}" />
            </button>
          </div>
        </div>

        <!-- Loading -->
        <div v-if="loadingChart" class="h-[260px] flex items-center justify-center">
          <RefreshCw :size="32" class="animate-spin text-blue-400 opacity-50" />
        </div>

        <!-- No data -->
        <div v-else-if="!hocLuc?.TheoLop?.length" class="h-[260px] flex flex-col items-center justify-center gap-2 border-2 border-dashed border-gray-100 dark:border-white/10 rounded-xl">
          <BookOpen :size="32" class="text-gray-300 dark:text-gray-600" />
          <p class="text-sm text-gray-400 dark:text-gray-500 font-medium">Chưa có dữ liệu điểm</p>
        </div>

        <!-- Chart -->
        <v-chart v-else
          :option="barOption"
          :style="{ height: '260px', width: '100%' }"
          :autoresize="true"
        />
      </div>

      <!-- BIỂU ĐỒ TRÒN: Phân bố xếp loại (chiếm 1/3) -->
      <div class="bg-white dark:bg-[#111C44] rounded-2xl p-6 shadow-sm border border-gray-100/50 dark:border-white/5">
        <div class="mb-4">
          <h3 class="text-lg font-bold text-[#2B3674] dark:text-white">Phân Bố Học Lực</h3>
          <p class="text-sm text-gray-400 dark:text-gray-400">Tỷ lệ xếp loại toàn trường</p>
        </div>

        <!-- Loading -->
        <div v-if="loadingChart" class="h-[260px] flex items-center justify-center">
          <RefreshCw :size="32" class="animate-spin text-blue-400 opacity-50" />
        </div>

        <!-- No data -->
        <div v-else-if="!hocLuc" class="h-[260px] flex flex-col items-center justify-center gap-2 border-2 border-dashed border-gray-100 dark:border-white/10 rounded-xl">
          <BookOpen :size="32" class="text-gray-300 dark:text-gray-600" />
          <p class="text-sm text-gray-400 dark:text-gray-500 font-medium">Chưa có dữ liệu</p>
        </div>

        <!-- Chart -->
        <v-chart v-else
          :option="pieOption"
          :style="{ height: '260px', width: '100%' }"
          :autoresize="true"
        />
      </div>
    </div>

    <!-- QUICK ACTIONS + SUMMARY ROW -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">

      <!-- Summary học lực -->
      <div v-if="hocLuc" class="bg-white dark:bg-[#111C44] rounded-2xl p-6 shadow-sm border border-gray-100/50 dark:border-white/5 lg:col-span-2">
        <h3 class="text-sm font-bold text-gray-400 uppercase tracking-wider mb-4">Chi tiết xếp loại · HK{{ selectedHocKy }} · {{ namHoc }}</h3>
        <div class="grid grid-cols-5 gap-3">
          <div v-for="item in [
            { label: 'Giỏi', value: hocLuc.Gioi, color: 'bg-green-50 dark:bg-green-500/10 text-green-600 dark:text-green-400 border-green-200 dark:border-green-500/30' },
            { label: 'Khá', value: hocLuc.Kha, color: 'bg-blue-50 dark:bg-blue-500/10 text-blue-600 dark:text-blue-400 border-blue-200 dark:border-blue-500/30' },
            { label: 'TB', value: hocLuc.TrungBinh, color: 'bg-amber-50 dark:bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-200 dark:border-amber-500/30' },
            { label: 'Yếu', value: hocLuc.Yeu, color: 'bg-orange-50 dark:bg-orange-500/10 text-orange-600 dark:text-orange-400 border-orange-200 dark:border-orange-500/30' },
            { label: 'Kém', value: hocLuc.Kem, color: 'bg-red-50 dark:bg-red-500/10 text-red-600 dark:text-red-400 border-red-200 dark:border-red-500/30' },
          ]" :key="item.label"
            :class="['rounded-xl p-4 text-center border', item.color]">
            <div class="text-2xl font-black leading-none">{{ item.value }}</div>
            <div class="text-[11px] font-bold uppercase tracking-wider mt-1 opacity-80">{{ item.label }}</div>
          </div>
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="bg-white dark:bg-[#111C44] rounded-2xl p-6 shadow-sm border border-gray-100/50 dark:border-white/5">
        <div class="flex justify-between items-center mb-4">
          <h3 class="text-lg font-bold text-[#2B3674] dark:text-white">Quick Actions</h3>
          <span class="text-xs text-gray-400 dark:text-gray-400">4 shortcuts</span>
        </div>
        <div class="grid grid-cols-2 gap-3">
          <button
            type="button"
            :disabled="!canQuickHocSinh"
            class="flex flex-col items-center justify-center p-4 border border-blue-100 dark:border-blue-500/10 bg-blue-50/50 dark:bg-blue-500/5 rounded-xl text-center group transition-colors"
            :class="canQuickHocSinh ? 'hover:bg-blue-50 dark:hover:bg-blue-500/10 cursor-pointer' : 'opacity-40 cursor-not-allowed'"
            @click="canQuickHocSinh && goQuick('/hoc-sinh')"
          >
            <Users :size="20" class="text-blue-500 dark:text-blue-400 mb-2 group-hover:scale-110 transition-transform" />
            <span class="text-xs font-bold text-[#2B3674] dark:text-gray-200">Học Sinh</span>
          </button>
          <button
            type="button"
            :disabled="!canQuickGiaoVien"
            class="flex flex-col items-center justify-center p-4 border border-teal-100 dark:border-teal-500/10 bg-teal-50/50 dark:bg-teal-500/5 rounded-xl text-center group transition-colors"
            :class="canQuickGiaoVien ? 'hover:bg-teal-50 dark:hover:bg-teal-500/10 cursor-pointer' : 'opacity-40 cursor-not-allowed'"
            @click="canQuickGiaoVien && goQuick('/giao-vien')"
          >
            <UserSquare :size="20" class="text-teal-500 dark:text-teal-400 mb-2 group-hover:scale-110 transition-transform" />
            <span class="text-xs font-bold text-[#2B3674] dark:text-gray-200">Giáo Viên</span>
          </button>
          <button
            type="button"
            :disabled="!canQuickLopLich"
            class="flex flex-col items-center justify-center p-4 border border-indigo-100 dark:border-indigo-500/10 bg-indigo-50/50 dark:bg-indigo-500/5 rounded-xl text-center group transition-colors"
            :class="canQuickLopLich ? 'hover:bg-indigo-50 dark:hover:bg-indigo-500/10 cursor-pointer' : 'opacity-40 cursor-not-allowed'"
            @click="canQuickLopLich && goQuick('/lop-hoc')"
          >
            <BookOpen :size="20" class="text-indigo-500 dark:text-indigo-400 mb-2 group-hover:scale-110 transition-transform" />
            <span class="text-xs font-bold text-[#2B3674] dark:text-gray-200">Lớp Học</span>
          </button>
          <button
            type="button"
            :disabled="!canQuickLopLich"
            class="flex flex-col items-center justify-center p-4 border border-purple-100 dark:border-purple-500/10 bg-purple-50/50 dark:bg-purple-500/5 rounded-xl text-center group transition-colors"
            :class="canQuickLopLich ? 'hover:bg-purple-50 dark:hover:bg-purple-500/10 cursor-pointer' : 'opacity-40 cursor-not-allowed'"
            @click="canQuickLopLich && goQuick('/lich-hoc')"
          >
            <Calendar :size="20" class="text-purple-500 dark:text-purple-400 mb-2 group-hover:scale-110 transition-transform" />
            <span class="text-xs font-bold text-[#2B3674] dark:text-gray-200">Lịch Học</span>
          </button>
        </div>
      </div>
    </div>

  </div>
</template>
