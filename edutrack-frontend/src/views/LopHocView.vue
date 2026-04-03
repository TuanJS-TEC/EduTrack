<script setup>
import { ref, computed, onMounted } from 'vue'
import { Plus, Search, MoreHorizontal, Users, UserSquare, Calendar, Filter, RefreshCcw } from 'lucide-vue-next'
import { apiService } from '../services/api'

const search = ref('')
const gradeFilter = ref('All')


const summaryCards = [
  { id: 1, title: 'Tổng số lớp học', value: '2', subtitle: 'Toàn trường', icon: Users, color: 'text-blue-500 bg-blue-50 dark:text-blue-400 dark:bg-blue-500/10' },
  { id: 2, title: 'Năm học hiện tại', value: '2025-2026', subtitle: 'Học kỳ 1', icon: Calendar, color: 'text-purple-500 bg-purple-50 dark:text-purple-400 dark:bg-purple-500/10' },
]

const classesList = ref([])
const loading = ref(false)

const fetchClasses = async () => {
  loading.value = true
  try {
    const res = await apiService.getLopHocs()
    classesList.value = res.data
  } catch (error) {
    console.error("Lỗi khi tải dữ liệu Lớp học:", error)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchClasses()
})

const filteredClasses = computed(() => {
  return classesList.value.filter(c => {
    const searchLower = search.value.toLowerCase()
    return ((c.MaLop || '').toLowerCase().includes(searchLower) || 
           (c.TenLop || '').toLowerCase().includes(searchLower) ||
           (c.MaGVChuNhiem || '').toLowerCase().includes(searchLower) ||
           (c.TenGVChuNhiem || '').toLowerCase().includes(searchLower)) &&
           (gradeFilter.value === 'All' || String(c.KhoiLop) === gradeFilter.value)
  })
})
</script>

<template>
  <div class="space-y-6">
    <!-- PAGE HEADER -->
    <div class="flex justify-between items-end">
      <div>
        <h2 class="text-2xl font-bold text-[#2B3674] dark:text-white mb-1">Lớp Học</h2>
        <p class="text-sm text-gray-400 dark:text-gray-400">Monitor class capacities, assignments, and schedules.</p>
      </div>
      <div class="flex items-center gap-3">
        <button @click="fetchClasses" class="flex items-center gap-2 px-4 py-2 border border-blue-200 dark:border-blue-500/30 rounded-lg text-sm font-medium text-blue-600 dark:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-500/10 transition-colors">
          <RefreshCcw :size="16" :class="{ 'animate-spin': loading }" />
          Refresh
        </button>
        <button class="flex items-center gap-2 px-4 py-2 bg-[#1E88E5] hover:bg-blue-600 text-white rounded-lg text-sm font-bold transition-colors shadow-sm shadow-blue-500/30 dark:shadow-none hidden sm:flex">
          <Plus :size="16" />
          Create Class
        </button>
      </div>
    </div>

    <!-- SUMMARY CARDS -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
      <div v-for="card in summaryCards" :key="card.id" class="bg-white dark:bg-[#111C44] rounded-2xl p-6 shadow-sm border border-gray-100/50 dark:border-white/5 flex items-center gap-5">
        <div :class="['w-14 h-14 rounded-full flex items-center justify-center', card.color]">
          <component :is="card.icon" :size="24" stroke-width="2" />
        </div>
        <div>
          <p class="text-xs font-bold text-gray-400 dark:text-gray-400 tracking-wider mb-1">{{ card.title }}</p>
          <div class="flex items-end gap-2">
            <h3 class="text-2xl font-extrabold text-[#2B3674] dark:text-white leading-none">{{ card.value }}</h3>
            <span class="text-xs font-bold text-gray-400 dark:text-gray-500 mb-0.5">{{ card.subtitle }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- MAIN TABLE CARD -->
    <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 relative min-h-[400px]">
      
      <!-- TOOLBAR -->
      <div class="p-5 border-b border-gray-100 dark:border-white/5 flex flex-wrap items-center justify-between gap-4 bg-gray-50/50 dark:bg-white/5">
        <div class="relative flex-1 min-w-[200px] max-w-sm">
          <Search :size="18" class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 dark:text-gray-500" />
          <input 
            v-model="search"
            type="text" 
            placeholder="Tìm theo Lớp, Tên lớp, Mã GV..." 
            class="w-full pl-10 pr-4 py-2 bg-white dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 shadow-sm rounded-lg text-sm font-medium focus:border-blue-500 transition-all outline-none dark:text-white placeholder-gray-400 dark:placeholder-gray-500"
          />
        </div>
        
        <div class="flex items-center gap-3">
          <div class="relative">
            <select v-model="gradeFilter" class="appearance-none bg-white dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 shadow-sm text-gray-600 dark:text-gray-200 py-2 pl-4 pr-10 rounded-lg text-sm font-bold focus:outline-none focus:border-blue-500 relative cursor-pointer outline-none">
              <Filter :size="14" class="inline mr-2" />
              <option value="All">All Grades</option>
              <option value="10">Khối 10</option>
              <option value="11">Khối 11</option>
              <option value="12">Khối 12</option>
            </select>
          </div>
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
              <th class="py-4 px-6">MÃ LỚP / TÊN LỚP</th>
              <th class="py-4 px-3 text-center">NĂM HỌC</th>
              <th class="py-4 px-3">GV CHỦ NHIỆM</th>
              <th class="py-4 pr-6 pl-3 text-right">ACTIONS</th>
            </tr>
          </thead>
          <tbody class="text-sm font-medium">
            <tr v-for="cls in filteredClasses" :key="cls.MaLop" class="border-b border-gray-50 dark:border-white/5 hover:bg-gray-50/30 dark:hover:bg-white/5 transition-colors">
              <td class="py-4 px-6">
                <div class="flex items-center gap-3">
                  <div class="w-10 h-10 rounded-xl bg-blue-50 dark:bg-blue-500/10 border border-blue-100 dark:border-blue-500/20 flex flex-col items-center justify-center shrink-0">
                    <span class="text-[10px] font-bold text-blue-400 uppercase leading-none mb-0.5">Khối</span>
                    <strong class="text-blue-600 dark:text-blue-400 leading-none">{{ cls.KhoiLop }}</strong>
                  </div>
                  <div>
                    <h4 class="font-extrabold text-[#2B3674] dark:text-gray-100 text-base">{{ cls.MaLop }}</h4>
                    <p class="text-xs font-bold text-gray-400 dark:text-gray-500">{{ cls.TenLop }}</p>
                  </div>
                </div>
              </td>
              <td class="py-4 px-3 text-center font-bold text-[#1E88E5] dark:text-blue-400">
                {{ cls.NamHoc }}
              </td>
              <td class="py-4 px-3">
                <div class="flex items-center gap-2">
                  <div class="w-7 h-7 rounded-full bg-gradient-to-br from-blue-500 to-indigo-600 text-white flex items-center justify-center text-[10px] font-bold shrink-0">
                    {{ (cls.TenGVChuNhiem || cls.MaGVChuNhiem || '?').charAt(0) }}
                  </div>
                  <div>
                    <p class="font-bold text-[#2B3674] dark:text-gray-200 text-sm">{{ cls.TenGVChuNhiem || 'Chưa phân công' }}</p>
                    <p class="text-[11px] font-mono text-gray-400 dark:text-gray-500">{{ cls.MaGVChuNhiem || '' }}</p>
                  </div>
                </div>
              </td>
              <td class="py-4 pr-6 pl-3 text-right">
                <button class="p-2 text-gray-400 dark:text-gray-500 hover:text-[#2B3674] dark:hover:text-white transition-colors rounded-lg hover:bg-gray-50 dark:hover:bg-white/5">
                  <MoreHorizontal :size="18" />
                </button>
              </td>
            </tr>
            <tr v-if="filteredClasses.length === 0 && !loading">
              <td colspan="4" class="py-12 text-center text-gray-400 dark:text-gray-500 font-bold">
                Không tìm thấy danh sách Lớp học nào.
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
