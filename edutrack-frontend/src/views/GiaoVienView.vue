<script setup>
import { ref, computed, onMounted } from 'vue'
import { Download, FileText, Search, Plus, Edit2, Eye, Trash2, RefreshCcw } from 'lucide-vue-next'
import { apiService } from '../services/api'

const search = ref('')
const chuyenMonFilter = ref('All')

const teachers = ref([])
const loading = ref(false)

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

onMounted(() => {
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
              <option value="Toán">Toán</option>
              <option value="Vật lý">Vật lý</option>
              <option value="Hóa học">Hóa học</option>
              <option value="Sinh học">Sinh học</option>
              <option value="Ngữ văn">Ngữ văn</option>
              <option value="Lịch sử">Lịch sử</option>
              <option value="Địa lý">Địa lý</option>
              <option value="Tiếng Anh">Tiếng Anh</option>
            </select>
          </div>

          <button class="flex items-center gap-2 px-4 py-2 bg-[#1E88E5] hover:bg-blue-600 text-white rounded-lg text-sm font-medium transition-colors shadow-sm shadow-blue-500/30 dark:shadow-none whitespace-nowrap">
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
            <tr v-for="teacher in filteredTeachers" :key="teacher.MaGV" class="border-b border-gray-50 dark:border-white/5 hover:bg-gray-50/50 dark:hover:bg-white/5 transition-colors group">
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
                  <button class="p-1.5 text-gray-400 dark:text-gray-500 hover:text-blue-500 hover:bg-blue-50 dark:hover:bg-blue-500/10 rounded-md transition-colors" title="Edit">
                    <Edit2 :size="16" />
                  </button>
                  <button class="p-1.5 text-gray-400 dark:text-gray-500 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-500/10 rounded-md transition-colors" title="Delete">
                    <Trash2 :size="16" />
                  </button>
                </div>
              </td>
            </tr>
            <tr v-if="filteredTeachers.length === 0 && !loading">
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
          Đang hiển thị <span class="font-bold text-[#2B3674] dark:text-white">{{ filteredTeachers.length }}</span> bản ghi
        </div>
        </div>
        <div class="flex items-center gap-1">
          <button class="w-8 h-8 flex items-center justify-center rounded text-gray-400 dark:text-gray-500 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors" disabled>&lt;</button>
          <button class="w-8 h-8 flex items-center justify-center rounded bg-[#1E88E5] text-white font-medium shadow-sm shadow-blue-500/30 dark:shadow-none">1</button>
          <button class="w-8 h-8 flex items-center justify-center rounded text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors font-medium">2</button>
          <button class="w-8 h-8 flex items-center justify-center rounded text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors">&gt;</button>
        </div>
      </div>
    </div>
  </div>
</template>
