<script setup>
import { Download, Search, Plus, Edit2, Trash2, RefreshCcw, X } from 'lucide-vue-next'
import { useGiaoVien } from '../composables/useGiaoVien'

const {
  search, chuyenMonFilter, loading, submitting,
  showModal, isEditMode, formData, chuyenMonOptions,
  filteredTeachers, pagedTeachers, currentPage, totalPages, pageNumbers,
  fetchTeachers, openAdd, openEdit, closeModal, handleSubmit, handleDelete,
  getInitials, getColor, formatCurrency, goToPage, prevPage, nextPage
} = useGiaoVien()
</script>

<template>
  <div class="space-y-6 p-8">
    <!-- PAGE HEADER -->
    <div class="flex justify-between items-end">
      <div>
        <h2 class="text-2xl font-bold text-[#2B3674] dark:text-white mb-1">Giáo Viên</h2>
        <p class="text-sm text-gray-400 dark:text-gray-400">Quản lý hồ sơ, chuyên môn và thông tin toàn bộ giáo viên.</p>
      </div>
      <div class="flex items-center gap-3">
        <button @click="fetchTeachers" class="flex items-center gap-2 px-4 py-2 border border-blue-200 dark:border-blue-500/30 rounded-lg text-sm font-medium text-blue-600 dark:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-500/10 transition-colors">
          <RefreshCcw :size="16" :class="{ 'animate-spin': loading }" />
          Tải lại
        </button>
        <button @click="openAdd" class="flex items-center gap-2 px-4 py-2 bg-[#1E88E5] hover:bg-blue-600 text-white rounded-lg text-sm font-bold transition-colors shadow-sm shadow-blue-500/30 dark:shadow-none whitespace-nowrap">
          <Plus :size="16" />
          Thêm Giáo Viên
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
          <select v-model="chuyenMonFilter" class="bg-white dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 shadow-sm text-gray-700 dark:text-gray-200 py-2 px-4 rounded-lg text-sm font-medium focus:outline-none focus:border-blue-500 cursor-pointer outline-none w-44">
            <option value="All">Mọi chuyên môn</option>
            <option v-for="cm in chuyenMonOptions" :key="cm" :value="cm">{{ cm }}</option>
          </select>
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
              <th class="py-4 pl-6 pr-3">GIÁO VIÊN</th>
              <th class="py-4 px-3">ID</th>
              <th class="py-4 px-3">CHUYÊN MÔN</th>
              <th class="py-4 px-3">LƯƠNG CƠ BẢN</th>
              <th class="py-4 px-3 text-center">TRẠNG THÁI</th>
              <th class="py-4 pr-6 pl-3 text-right">HÀNH ĐỘNG</th>
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
                <span class="inline-flex items-center px-2.5 py-1 rounded text-[11px] font-bold uppercase tracking-wider bg-green-50 dark:bg-green-500/10 text-green-600 dark:text-green-400 border border-green-200 dark:border-transparent">
                  Active
                </span>
              </td>
              <td class="py-4 pr-6 pl-3">
                <div class="flex items-center justify-end gap-2">
                  <button @click="openEdit(teacher)" class="p-1.5 text-gray-400 dark:text-gray-500 hover:text-blue-500 hover:bg-blue-50 dark:hover:bg-blue-500/10 rounded-md transition-colors" title="Sửa">
                    <Edit2 :size="16" />
                  </button>
                  <button @click="handleDelete(teacher)" class="p-1.5 text-gray-400 dark:text-gray-500 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-500/10 rounded-md transition-colors" title="Xóa">
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
      <div class="p-5 border-t border-gray-100 dark:border-white/5 flex items-center justify-between text-sm text-gray-500 dark:text-gray-400 bg-gray-50/30 dark:bg-transparent">
        <div>
          Hiển thị <span class="font-bold text-[#2B3674] dark:text-white">{{ pagedTeachers.length }}</span> / <span class="font-bold text-[#2B3674] dark:text-white">{{ filteredTeachers.length }}</span> giáo viên
        </div>
        <div class="flex items-center gap-1">
          <button @click="prevPage" :disabled="currentPage === 1" class="w-8 h-8 flex items-center justify-center rounded text-gray-400 dark:text-gray-500 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors disabled:opacity-50 disabled:cursor-not-allowed" title="Trang trước">&lt;</button>
          <template v-for="page in pageNumbers" :key="page">
            <span v-if="page === '..'" class="px-2">...</span>
            <button v-else @click="goToPage(page)" :class="['w-8 h-8 flex items-center justify-center rounded font-medium transition-colors', currentPage === page ? 'bg-[#1E88E5] text-white shadow-sm shadow-blue-500/30 dark:shadow-none' : 'text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-white/5']">{{ page }}</button>
          </template>
          <button @click="nextPage" :disabled="currentPage === totalPages" class="w-8 h-8 flex items-center justify-center rounded text-gray-400 dark:text-gray-500 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors disabled:opacity-50 disabled:cursor-not-allowed" title="Trang sau">&gt;</button>
        </div>
      </div>
    </div>

    <!-- MODAL: ADD/EDIT TEACHER -->
    <Transition name="fade">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm" @click="closeModal"></div>
        <div class="relative bg-white dark:bg-[#111C44] rounded-2xl shadow-xl max-w-md w-full max-h-[90vh] overflow-y-auto border border-gray-100 dark:border-white/10">
          <div class="p-6 border-b border-gray-100 dark:border-white/5 flex items-center justify-between">
            <h3 class="text-lg font-bold text-[#2B3674] dark:text-white">{{ isEditMode ? 'Cập nhật Giáo Viên' : 'Thêm Giáo Viên Mới' }}</h3>
            <button @click="closeModal" class="p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors">
              <X :size="20" />
            </button>
          </div>

          <form @submit.prevent="handleSubmit" class="p-6 space-y-4">
            <div>
              <label class="block text-xs font-bold text-gray-500 dark:text-gray-400 mb-1.5 uppercase tracking-wider">Mã Giáo Viên <span class="text-red-500">*</span></label>
              <input v-model="formData.maGV" :disabled="isEditMode" type="text" placeholder="VD: GV001" class="w-full px-4 py-2 border border-gray-200 dark:border-white/10 rounded-lg bg-gray-50 dark:bg-[#0B1437] text-gray-900 dark:text-white focus:outline-none focus:border-blue-500 transition-colors disabled:opacity-50" />
            </div>
            <div>
              <label class="block text-xs font-bold text-gray-500 dark:text-gray-400 mb-1.5 uppercase tracking-wider">Họ và Tên <span class="text-red-500">*</span></label>
              <input v-model="formData.hoTen" type="text" placeholder="VD: Nguyễn Văn A" class="w-full px-4 py-2 border border-gray-200 dark:border-white/10 rounded-lg bg-gray-50 dark:bg-[#0B1437] text-gray-900 dark:text-white focus:outline-none focus:border-blue-500 transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-bold text-gray-500 dark:text-gray-400 mb-1.5 uppercase tracking-wider">Chuyên Môn</label>
              <select v-model="formData.chuyenMon" class="w-full px-4 py-2 border border-gray-200 dark:border-white/10 rounded-lg bg-gray-50 dark:bg-[#0B1437] text-gray-900 dark:text-white focus:outline-none focus:border-blue-500 transition-colors">
                <option value="">-- Chọn chuyên môn --</option>
                <option v-for="cm in chuyenMonOptions" :key="cm" :value="cm">{{ cm }}</option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-bold text-gray-500 dark:text-gray-400 mb-1.5 uppercase tracking-wider">Email</label>
              <input v-model="formData.email" type="email" placeholder="giaovien@mail.com" class="w-full px-4 py-2 border border-gray-200 dark:border-white/10 rounded-lg bg-gray-50 dark:bg-[#0B1437] text-gray-900 dark:text-white focus:outline-none focus:border-blue-500 transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-bold text-gray-500 dark:text-gray-400 mb-1.5 uppercase tracking-wider">Lương Cơ Bản</label>
              <input v-model="formData.luongCoBan" type="number" placeholder="5.000.000" class="w-full px-4 py-2 border border-gray-200 dark:border-white/10 rounded-lg bg-gray-50 dark:bg-[#0B1437] text-gray-900 dark:text-white focus:outline-none focus:border-blue-500 transition-colors" />
            </div>
            <div class="flex gap-3 pt-4">
              <button type="button" @click="closeModal" class="flex-1 px-4 py-2 border border-gray-200 dark:border-white/10 rounded-lg text-gray-700 dark:text-gray-300 font-bold hover:bg-gray-50 dark:hover:bg-white/5 transition-colors">Hủy</button>
              <button type="submit" :disabled="submitting" class="flex-1 px-4 py-2 bg-[#1E88E5] hover:bg-blue-600 text-white rounded-lg font-bold transition-colors shadow-sm shadow-blue-500/30 disabled:opacity-50">{{ submitting ? 'Đang lưu...' : (isEditMode ? 'Cập nhật' : 'Thêm mới') }}</button>
            </div>
          </form>
        </div>
      </div>
    </Transition>
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

