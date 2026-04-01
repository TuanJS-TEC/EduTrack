<script setup>
import { ref, computed, onMounted } from 'vue'
import { Download, Upload, Search, Plus, Edit2, Eye, Trash2, RefreshCcw } from 'lucide-vue-next'
import { apiService } from '../services/api'

const search = ref('')
const gradeFilter = ref('All')
const statusFilter = ref('All')

const students = ref([])
const loading = ref(false)

const fetchStudents = async () => {
  loading.value = true
  try {
    const res = await apiService.getHocSinhs()
    students.value = res.data
  } catch (error) {
    console.error("Lỗi khi tải dữ liệu Học Sinh:", error)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchStudents()
})

const filteredStudents = computed(() => {
  return students.value.filter(s => {
    return ((s.HoTen || '').toLowerCase().includes(search.value.toLowerCase()) || 
           (s.MaHS || '').toLowerCase().includes(search.value.toLowerCase())) &&
           (gradeFilter.value === 'All' || (s.MaLop || '').startsWith(gradeFilter.value)) &&
           (statusFilter.value === 'All' || s.TrangThai === statusFilter.value)
  })
})

const getGpaColor = (gpa) => {
  if (gpa >= 8.0) return 'text-green-500 dark:text-green-400'
  if (gpa >= 6.5) return 'text-orange-400 dark:text-orange-300'
  return 'text-red-500 dark:text-red-400'
}

// Generate a random stable color based on ID
const getColor = (id) => {
  const colors = [
    'bg-blue-100 dark:bg-blue-500/20 text-blue-600 dark:text-blue-400',
    'bg-indigo-100 dark:bg-indigo-500/20 text-indigo-600 dark:text-indigo-400',
    'bg-emerald-100 dark:bg-emerald-500/20 text-emerald-600 dark:text-emerald-400',
    'bg-cyan-100 dark:bg-cyan-500/20 text-cyan-600 dark:text-cyan-400',
    'bg-purple-100 dark:bg-purple-500/20 text-purple-600 dark:text-purple-400',
    'bg-pink-100 dark:bg-pink-500/20 text-pink-600 dark:text-pink-400'
  ]
  let hash = 0
  for (let i = 0; i < (id || '').length; i++) {
    hash = id.charCodeAt(i) + ((hash << 5) - hash)
  }
  return colors[Math.abs(hash) % colors.length]
}
</script>

<template>
  <div class="space-y-6">
    <!-- PAGE HEADER -->
    <div class="flex justify-between items-end">
      <div>
        <h2 class="text-2xl font-bold text-[#2B3674] dark:text-white mb-1">Students</h2>
        <p class="text-sm text-gray-400 dark:text-gray-400">Manage all enrolled students and their records.</p>
      </div>
      <div class="flex items-center gap-3">
        <button @click="fetchStudents" class="flex items-center gap-2 px-4 py-2 border border-blue-200 dark:border-blue-500/30 rounded-lg text-sm font-medium text-blue-600 dark:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-500/10 transition-colors">
          <RefreshCcw :size="16" :class="{ 'animate-spin': loading }" />
          Refresh
        </button>
        <button class="flex items-center gap-2 px-4 py-2 border border-gray-200 dark:border-white/10 rounded-lg text-sm font-medium text-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors hidden sm:flex">
          <Download :size="16" /> Export
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
            placeholder="Search by name or ID..." 
            class="w-full pl-10 pr-4 py-2 bg-white dark:bg-[#0B1437] border-transparent rounded-lg text-sm focus:border-blue-500 shadow-sm transition-all outline-none dark:text-white placeholder-gray-400 dark:placeholder-gray-500 border border-gray-200 dark:border-white/10"
          />
        </div>
        
        <div class="flex items-center gap-3">
          <div class="relative">
            <select v-model="gradeFilter" class="appearance-none bg-white dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 text-gray-700 dark:text-gray-200 py-2 pl-4 pr-10 rounded-lg text-sm font-medium focus:outline-none focus:border-blue-500 relative cursor-pointer outline-none shadow-sm">
              <option value="All">All Grades</option>
              <option value="10">Grade 10</option>
              <option value="11">Grade 11</option>
              <option value="12">Grade 12</option>
            </select>
          </div>
          
          <div class="relative">
            <select v-model="statusFilter" class="appearance-none bg-white dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 text-gray-700 dark:text-gray-200 py-2 pl-4 pr-10 rounded-lg text-sm font-medium focus:outline-none focus:border-blue-500 relative cursor-pointer outline-none shadow-sm">
              <option value="All">All Status</option>
              <option value="Đang học">Đang học</option>
              <option value="Đã nghỉ">Đã nghỉ</option>
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
              <th class="py-4 pl-6 pr-3 w-12"><input type="checkbox" class="rounded border-gray-300 dark:border-gray-600 dark:bg-transparent text-blue-500 focus:ring-blue-500 w-4 h-4 cursor-pointer" /></th>
              <th class="py-4 px-3">HỌC SINH</th>
              <th class="py-4 px-3 text-center">ID</th>
              <th class="py-4 px-3 text-center">LỚP</th>
              <th class="py-4 px-3 text-center">SĐT PHỤ HUYNH</th>
              <th class="py-4 px-3 text-center">NĂM SINH</th>
              <th class="py-4 px-3 text-center">TRẠNG THÁI</th>
              <th class="py-4 pr-6 pl-3 text-right">HÀNH ĐỘNG</th>
            </tr>
          </thead>
          <tbody class="text-sm font-medium">
            <tr v-for="student in filteredStudents" :key="student.MaHS" class="border-b border-gray-50 dark:border-white/5 hover:bg-gray-50/50 dark:hover:bg-white/5 transition-colors group">
              <td class="py-4 pl-6 pr-3"><input type="checkbox" class="rounded border-gray-300 dark:border-gray-600 dark:bg-transparent text-blue-500 focus:ring-blue-500 w-4 h-4 cursor-pointer" /></td>
              <td class="py-4 px-3">
                <div class="flex items-center gap-3">
                  <div :class="['w-9 h-9 rounded-full flex items-center justify-center font-bold text-sm shrink-0', getColor(student.MaHS)]">
                    {{ (student.HoTen || 'A').charAt(0) }}
                  </div>
                  <div>
                    <p class="font-bold text-[#2B3674] dark:text-gray-100">{{ student.HoTen }}</p>
                    <p class="text-[11px] text-gray-400 dark:text-gray-500 truncate max-w-[150px]">{{ student.Email_PhuHuynh || 'Chưa cập nhật Email' }}</p>
                  </div>
                </div>
              </td>
              <td class="py-4 px-3 text-center text-xs font-mono font-bold text-gray-500 dark:text-gray-400">{{ student.MaHS }}</td>
              <td class="py-4 px-3 text-center font-bold text-[#1E88E5] dark:text-blue-400">{{ student.MaLop || '-' }}</td>
              <td class="py-4 px-3 text-center text-gray-600 dark:text-gray-300">{{ student.SDT_PhuHuynh || '-' }}</td>
              <td class="py-4 px-3 text-center text-gray-600 dark:text-gray-400 text-sm">{{ new Date(student.NgaySinh).toLocaleDateString() }}</td>
              <td class="py-4 px-3 text-center">
                <span 
                  class="inline-flex items-center px-2.5 py-1 rounded text-[11px] font-bold uppercase tracking-wider"
                  :class="(student.TrangThai || 'Đang học') === 'Đang học' ? 'bg-green-50 dark:bg-green-500/10 text-green-600 dark:text-green-400 border border-green-200 dark:border-transparent' : 'bg-gray-100 dark:bg-white/10 text-gray-500 dark:text-gray-400 border border-gray-200 dark:border-transparent'"
                >
                  {{ student.TrangThai || 'Đang học' }}
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
            <tr v-if="filteredStudents.length === 0 && !loading">
              <td colspan="8" class="py-12 text-center text-gray-500 dark:text-gray-400 font-medium">
                Không tìm thấy Hồ sơ Học sinh nào.
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- FOOTER / PAGINATION -->
      <div class="p-5 border-t border-gray-100 dark:border-white/5 flex items-center justify-between text-sm text-gray-500 dark:text-gray-400 bg-gray-50/30 dark:bg-white-[0.02]">
        <div>
          Đang hiển thị <span class="font-bold text-[#2B3674] dark:text-white">{{ filteredStudents.length }}</span> bản ghi
        </div>
        </div>
        <div class="flex items-center gap-1">
          <button class="w-8 h-8 flex items-center justify-center rounded text-gray-400 dark:text-gray-500 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors" disabled>&lt;</button>
          <button class="w-8 h-8 flex items-center justify-center rounded bg-[#1E88E5] text-white font-medium shadow-sm shadow-blue-500/30 dark:shadow-none">1</button>
          <button class="w-8 h-8 flex items-center justify-center rounded text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors font-medium">2</button>
          <button class="w-8 h-8 flex items-center justify-center rounded text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors font-medium">3</button>
          <button class="w-8 h-8 flex items-center justify-center rounded text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors">&gt;</button>
        </div>
      </div>
    </div>
  
</template>
