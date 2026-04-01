<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { BookOpen, User, MapPin, RefreshCcw, School, ChevronDown } from 'lucide-vue-next'
import { apiService } from '../services/api'

// ── Tabs ─────────────────────────────────────────────
const activeTab = ref('lop') // 'lop' | 'giaovien'

// ── Dropdown data ─────────────────────────────────────
const lopList    = ref([])
const giaoVienList = ref([])
const selectedLop = ref('')
const selectedGV  = ref('')

// ── Schedule data ─────────────────────────────────────
const lichHocList = ref([])
const loading = ref(false)

// ── Constants ─────────────────────────────────────────
const THUS = [
  { thu: 2, label: 'Thứ 2' },
  { thu: 3, label: 'Thứ 3' },
  { thu: 4, label: 'Thứ 4' },
  { thu: 5, label: 'Thứ 5' },
  { thu: 6, label: 'Thứ 6' },
  { thu: 7, label: 'Thứ 7' },
]
const TIETS = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]

// ── Subject colors ────────────────────────────────────
const MON_COLORS = [
  'bg-blue-50 dark:bg-blue-500/10 border-blue-200 dark:border-blue-500/30 text-blue-700 dark:text-blue-300',
  'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-300',
  'bg-purple-50 dark:bg-purple-500/10 border-purple-200 dark:border-purple-500/30 text-purple-700 dark:text-purple-300',
  'bg-orange-50 dark:bg-orange-500/10 border-orange-200 dark:border-orange-500/30 text-orange-700 dark:text-orange-300',
  'bg-indigo-50 dark:bg-indigo-500/10 border-indigo-200 dark:border-indigo-500/30 text-indigo-700 dark:text-indigo-300',
  'bg-rose-50 dark:bg-rose-500/10 border-rose-200 dark:border-rose-500/30 text-rose-700 dark:text-rose-300',
  'bg-amber-50 dark:bg-amber-500/10 border-amber-200 dark:border-amber-500/30 text-amber-700 dark:text-amber-300',
  'bg-cyan-50 dark:bg-cyan-500/10 border-cyan-200 dark:border-cyan-500/30 text-cyan-700 dark:text-cyan-300',
]
const monColorMap = {}
const getMonColor = (maMon) => {
  if (!monColorMap[maMon]) {
    const idx = Object.keys(monColorMap).length % MON_COLORS.length
    monColorMap[maMon] = MON_COLORS[idx]
  }
  return monColorMap[maMon]
}

// ── Cell lookup: thu → tiet → item ───────────────────
const scheduleMap = computed(() => {
  const map = {}
  for (const item of lichHocList.value) {
    const thu = item.Thu
    const bd  = item.TietBD
    const kt  = item.TietKT ?? bd
    if (!thu || !bd) continue
    for (let t = bd; t <= kt; t++) {
      const key = `${thu}-${t}`
      if (!map[key]) map[key] = []
      map[key].push(item)
    }
  }
  return map
})

const getCellItems = (thu, tiet) => scheduleMap.value[`${thu}-${tiet}`] || []

// ── Stats ─────────────────────────────────────────────
const totalMons = computed(() => new Set(lichHocList.value.map(x => x.MaMon)).size)
const totalTiets = computed(() => lichHocList.value.reduce((acc, x) => {
  const bd = x.TietBD ?? 0; const kt = x.TietKT ?? bd
  return acc + (kt - bd + 1)
}, 0))

// ── Fetch ─────────────────────────────────────────────
const fetchSchedule = async () => {
  const key = activeTab.value === 'lop' ? selectedLop.value : selectedGV.value
  if (!key) { lichHocList.value = []; return }
  loading.value = true
  try {
    const res = activeTab.value === 'lop'
      ? await apiService.getLichHocByLop(key)
      : await apiService.getLichHocByGV(key)
    lichHocList.value = res.data
  } catch (e) {
    console.error('Lỗi tải lịch:', e)
  } finally {
    loading.value = false
  }
}

const fetchMasterData = async () => {
  try {
    const [lopRes, gvRes] = await Promise.all([
      apiService.getLopHocs(),
      apiService.getGiaoViens(),
    ])
    lopList.value    = lopRes.data
    giaoVienList.value = gvRes.data
    // auto-select first
    if (lopList.value.length)    { selectedLop.value = lopList.value[0].MaLop; fetchSchedule() }
    if (giaoVienList.value.length) selectedGV.value = giaoVienList.value[0].MaGV
  } catch (e) {
    console.error('Lỗi tải danh sách:', e)
  }
}

onMounted(fetchMasterData)
watch([activeTab, selectedLop, selectedGV], fetchSchedule)
</script>

<template>
  <div class="space-y-6">
    <!-- PAGE HEADER -->
    <div class="flex justify-between items-end">
      <div>
        <h2 class="text-2xl font-bold text-[#2B3674] dark:text-white mb-1">Thời Khóa Biểu</h2>
        <p class="text-sm text-gray-400">Xem lịch học theo lớp hoặc theo giáo viên phụ trách.</p>
      </div>
      <button @click="fetchSchedule" class="flex items-center gap-2 px-4 py-2 border border-blue-200 dark:border-blue-500/30 rounded-lg text-sm font-medium text-blue-600 dark:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-500/10 transition-colors">
        <RefreshCcw :size="16" :class="{ 'animate-spin': loading }" />
        Tải lại
      </button>
    </div>

    <!-- TABS + SELECTOR -->
    <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 p-5">
      <div class="flex flex-wrap items-center gap-4">
        <!-- Tab switcher -->
        <div class="flex bg-gray-100 dark:bg-white/5 rounded-xl p-1 gap-1">
          <button
            @click="activeTab = 'lop'"
            :class="['flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-bold transition-all', activeTab === 'lop' ? 'bg-white dark:bg-[#1E2A5E] text-[#1E88E5] shadow-sm' : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200']"
          >
            <School :size="15" />
            Theo Lớp
          </button>
          <button
            @click="activeTab = 'giaovien'"
            :class="['flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-bold transition-all', activeTab === 'giaovien' ? 'bg-white dark:bg-[#1E2A5E] text-[#1E88E5] shadow-sm' : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200']"
          >
            <User :size="15" />
            Theo Giáo Viên
          </button>
        </div>

        <!-- Selector dropdown -->
        <div class="relative flex-1 max-w-xs">
          <ChevronDown :size="16" class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none" />
          <select
            v-if="activeTab === 'lop'"
            v-model="selectedLop"
            class="w-full appearance-none bg-gray-50 dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 rounded-xl py-2.5 pl-4 pr-10 text-sm font-bold text-[#2B3674] dark:text-white focus:outline-none focus:border-blue-500 transition-colors"
          >
            <option value="">-- Chọn lớp --</option>
            <option v-for="lop in lopList" :key="lop.MaLop" :value="lop.MaLop">
              {{ lop.TenLop }} ({{ lop.NamHoc }})
            </option>
          </select>
          <select
            v-else
            v-model="selectedGV"
            class="w-full appearance-none bg-gray-50 dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 rounded-xl py-2.5 pl-4 pr-10 text-sm font-bold text-[#2B3674] dark:text-white focus:outline-none focus:border-blue-500 transition-colors"
          >
            <option value="">-- Chọn giáo viên --</option>
            <option v-for="gv in giaoVienList" :key="gv.MaGV" :value="gv.MaGV">
              {{ gv.HoTen }} ({{ gv.MaGV }})
            </option>
          </select>
        </div>

        <!-- Quick stats -->
        <div class="flex items-center gap-4 ml-auto text-sm">
          <div class="text-center">
            <p class="font-extrabold text-[#2B3674] dark:text-white text-lg leading-none">{{ lichHocList.length }}</p>
            <p class="text-gray-400 text-xs font-bold">Buổi học</p>
          </div>
          <div class="w-px h-8 bg-gray-200 dark:bg-white/10"></div>
          <div class="text-center">
            <p class="font-extrabold text-[#2B3674] dark:text-white text-lg leading-none">{{ totalMons }}</p>
            <p class="text-gray-400 text-xs font-bold">Môn học</p>
          </div>
          <div class="w-px h-8 bg-gray-200 dark:bg-white/10"></div>
          <div class="text-center">
            <p class="font-extrabold text-[#2B3674] dark:text-white text-lg leading-none">{{ totalTiets }}</p>
            <p class="text-gray-400 text-xs font-bold">Số tiết</p>
          </div>
        </div>
      </div>
    </div>

    <!-- SCHEDULE TABLE -->
    <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 overflow-hidden relative">

      <!-- Loading overlay -->
      <div v-if="loading" class="absolute inset-0 bg-white/60 dark:bg-[#111C44]/60 z-20 flex items-center justify-center backdrop-blur-sm rounded-2xl">
        <RefreshCcw :size="36" class="animate-spin text-blue-500" />
      </div>

      <!-- Empty state -->
      <div v-if="!loading && lichHocList.length === 0" class="py-20 text-center text-gray-400 dark:text-gray-500">
        <BookOpen :size="48" class="mx-auto mb-4 opacity-30" />
        <p class="font-bold text-lg">Chưa có lịch học nào</p>
        <p class="text-sm mt-1">Hãy chọn {{ activeTab === 'lop' ? 'lớp học' : 'giáo viên' }} ở trên để xem lịch.</p>
      </div>

      <!-- Timetable grid -->
      <div v-else class="overflow-x-auto">
        <table class="w-full border-collapse min-w-[700px]">
          <thead>
            <tr>
              <th class="w-16 py-3 px-2 text-center text-xs font-bold text-gray-400 dark:text-gray-500 uppercase tracking-wider border-b border-r border-gray-100 dark:border-white/5 bg-gray-50/70 dark:bg-white/3">
                Tiết
              </th>
              <th
                v-for="d in THUS" :key="d.thu"
                class="py-3 px-2 text-center text-xs font-bold text-gray-400 dark:text-gray-500 uppercase tracking-wider border-b border-r border-gray-100 dark:border-white/5 bg-gray-50/70 dark:bg-white/3"
              >
                {{ d.label }}
              </th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="tiet in TIETS" :key="tiet" class="group">
              <!-- Tiết label -->
              <td class="py-1.5 px-2 text-center border-b border-r border-gray-100 dark:border-white/5 bg-gray-50/40 dark:bg-white/[0.02]">
                <span class="inline-flex items-center justify-center w-7 h-7 rounded-lg bg-blue-50 dark:bg-blue-500/10 text-blue-500 dark:text-blue-400 font-extrabold text-xs">
                  {{ tiet }}
                </span>
              </td>
              <!-- Cells -->
              <td
                v-for="d in THUS" :key="d.thu"
                class="py-1.5 px-1.5 border-b border-r border-gray-100 dark:border-white/5 align-top min-w-[110px] max-w-[160px]"
              >
                <div
                  v-for="item in getCellItems(d.thu, tiet)"
                  :key="`${item.MaLich}-${tiet}`"
                  :class="['rounded-lg border p-2 mb-1 last:mb-0 cursor-pointer hover:shadow-md transition-shadow text-left', getMonColor(item.MaMon)]"
                >
                  <p class="font-extrabold text-[11px] leading-tight truncate">{{ item.TenMon || item.MaMon }}</p>

                  <!-- Lịch theo lớp → hiện GV -->
                  <p v-if="activeTab === 'lop'" class="text-[10px] font-bold opacity-75 flex items-center gap-1 mt-0.5 truncate">
                    <User :size="9" />{{ item.TenGV || item.MaGV || '—' }}
                  </p>

                  <!-- Lịch theo GV → hiện Lớp -->
                  <p v-else class="text-[10px] font-bold opacity-75 flex items-center gap-1 mt-0.5 truncate">
                    <School :size="9" />{{ item.TenLop || item.MaLop || '—' }}
                  </p>

                  <p v-if="item.Phong" class="text-[10px] font-bold opacity-60 flex items-center gap-1 truncate">
                    <MapPin :size="9" />{{ item.Phong }}
                  </p>
                </div>
                <!-- Empty placeholder -->
                <div v-if="getCellItems(d.thu, tiet).length === 0" class="h-3"></div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Legend -->
      <div v-if="lichHocList.length > 0" class="p-4 border-t border-gray-100 dark:border-white/5 flex flex-wrap gap-3">
        <div
          v-for="maMon in [...new Set(lichHocList.map(x => x.MaMon))]"
          :key="maMon"
          class="flex items-center gap-1.5"
        >
          <div :class="['w-3 h-3 rounded-sm border', getMonColor(maMon)]"></div>
          <span class="text-xs font-bold text-gray-500 dark:text-gray-400">
            {{ lichHocList.find(x => x.MaMon === maMon)?.TenMon || maMon }}
          </span>
        </div>
      </div>
    </div>
  </div>
</template>
