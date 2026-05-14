<script setup>
import { computed } from 'vue'
import { Download, Search, Edit2, Trash2, RefreshCcw, Plus, UserMinus } from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import { useHocSinh } from '../composables/useHocSinh'

const auth = useAuthStore()
/** Chỉ Admin (và các vai được gán Students.Edit) mới được thêm/sửa/xóa hồ sơ học sinh. */
const canEditStudents = computed(() => auth.hasPermission('Students.Edit'))

const {
  search, gradeFilter, statusFilter,
  loading, filteredStudents, pagedStudents,
  currentPage, totalPages, pageNumbers,
  goToPage, prevPage, nextPage,
  fetchStudents, getGpaColor, getHanhKiemClass, getAvatarColor, formatDate,
  showEditModal, editForm, saving,
  openEdit, saveEdit, confirmDelete,
  showAddModal, addForm, adding,
  openAdd, saveAdd,
  selectedIds, isAllSelected, toggleAll, toggleOne, bulkDelete,
  exportExcel,
} = useHocSinh()
</script>

<template>
  <div class="space-y-6 p-8">

    <!-- PAGE HEADER -->
    <div class="flex justify-between items-end">
      <div>
        <h2 class="text-2xl font-bold text-[#2B3674] dark:text-white mb-1">Học Sinh</h2>
        <p class="text-sm text-gray-400">
          {{ canEditStudents ? 'Quản lý hồ sơ và thông tin toàn bộ học sinh.' : 'Xem thông tin học sinh được liên kết với tài khoản của bạn.' }}
        </p>
      </div>
      <div class="flex items-center gap-3">
        <button @click="fetchStudents"
          class="flex items-center gap-2 px-4 py-2 border border-blue-200 dark:border-blue-500/30 rounded-lg text-sm font-medium text-blue-600 dark:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-500/10 transition-colors">
          <RefreshCcw :size="16" :class="{ 'animate-spin': loading }" />
          Tải lại
        </button>
        <button @click="exportExcel"
          class="hidden sm:flex items-center gap-2 px-4 py-2 border border-gray-200 dark:border-white/10 rounded-lg text-sm font-medium text-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors">
          <Download :size="16" /> Xuất Excel
        </button>
        <button v-if="canEditStudents" @click="openAdd"
          class="flex items-center gap-2 px-4 py-2 bg-[#1E88E5] hover:bg-blue-600 rounded-lg text-sm font-bold text-white transition-colors shadow-sm shadow-blue-500/30">
          <Plus :size="16" /> Thêm mới
        </button>
      </div>
    </div>

    <!-- BULK ACTION BAR -->
    <Transition name="slide-down">
      <div v-if="canEditStudents && selectedIds.size > 0"
        class="flex items-center justify-between px-5 py-3 bg-blue-50 dark:bg-blue-500/10 border border-blue-200 dark:border-blue-500/20 rounded-xl">
        <span class="text-sm font-bold text-blue-700 dark:text-blue-300">
          Đã chọn <span class="bg-blue-600 text-white rounded px-1.5">{{ selectedIds.size }}</span> học sinh
        </span>
        <button @click="bulkDelete"
          class="flex items-center gap-2 px-4 py-1.5 bg-red-500 hover:bg-red-600 text-white text-sm font-bold rounded-lg transition-colors">
          <UserMinus :size="14" /> Xóa tất cả đã chọn
        </button>
      </div>
    </Transition>

    <!-- MAIN TABLE CARD -->
    <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 relative min-h-[400px]">

      <!-- TOOLBAR -->
      <div class="p-5 border-b border-gray-100 dark:border-white/5 flex flex-wrap items-center justify-between gap-4 bg-gray-50/50 dark:bg-white/5">
        <div class="relative flex-1 min-w-[200px] max-w-lg">
          <Search :size="18" class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 dark:text-gray-500" />
          <input
            v-model="search"
            type="text"
            placeholder="Tìm theo tên hoặc mã học sinh..."
            class="w-full pl-10 pr-4 py-2 bg-white dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 rounded-lg text-sm focus:border-blue-500 shadow-sm transition-all outline-none dark:text-white placeholder-gray-400 dark:placeholder-gray-500"
          />
        </div>

        <div class="flex items-center gap-3">
          <select v-model="gradeFilter"
            class="appearance-none bg-white dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 text-gray-700 dark:text-gray-200 py-2 pl-4 pr-10 rounded-lg text-sm font-medium focus:outline-none focus:border-blue-500 shadow-sm cursor-pointer">
            <option value="All">Tất cả khối</option>
            <option value="10">Khối 10</option>
            <option value="11">Khối 11</option>
            <option value="12">Khối 12</option>
          </select>

          <select v-model="statusFilter"
            class="appearance-none bg-white dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 text-gray-700 dark:text-gray-200 py-2 pl-4 pr-10 rounded-lg text-sm font-medium focus:outline-none focus:border-blue-500 shadow-sm cursor-pointer">
            <option value="All">Tất cả trạng thái</option>
            <option value="Đang học">Đang học</option>
            <option value="Đã nghỉ">Đã nghỉ</option>
          </select>
        </div>
      </div>

      <!-- LOADING OVERLAY -->
      <div v-if="loading"
        class="absolute inset-0 bg-white/50 dark:bg-[#111C44]/50 z-10 flex items-center justify-center backdrop-blur-sm rounded-2xl">
        <RefreshCcw :size="32" class="animate-spin text-blue-500" />
      </div>

      <!-- TABLE -->
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="text-[11px] font-bold text-gray-400 uppercase tracking-wider border-b border-gray-100 dark:border-white/5">
              <th v-if="canEditStudents" class="py-4 pl-6 pr-3 w-12">
                <input type="checkbox" :checked="isAllSelected" @change="toggleAll"
                  class="rounded border-gray-300 dark:border-gray-600 dark:bg-transparent text-blue-500 focus:ring-blue-500 w-4 h-4 cursor-pointer" />
              </th>
              <th class="py-4 px-3">Học Sinh</th>
              <th class="py-4 px-3 text-center">Mã HS</th>
              <th class="py-4 px-3 text-center">Lớp</th>
              <th class="py-4 px-3 text-center">SĐT Phụ Huynh</th>
              <th class="py-4 px-3 text-center">Năm Sinh</th>
              <th class="py-4 px-3 text-center">Trạng Thái</th>
              <th class="py-4 px-3 text-center">Điểm TB</th>
              <th class="py-4 px-3 text-center">Hạnh Kiểm</th>
              <th v-if="canEditStudents" class="py-4 pr-6 pl-3 text-right">Hành Động</th>
            </tr>
          </thead>
          <tbody class="text-sm font-medium">
            <tr v-for="student in pagedStudents" :key="student.MaHS"
              class="border-b border-gray-50 dark:border-white/5 hover:bg-gray-50/50 dark:hover:bg-white/5 transition-colors group">

              <td v-if="canEditStudents" class="py-4 pl-6 pr-3">
                <input type="checkbox" :checked="selectedIds.has(student.MaHS)" @change="toggleOne(student.MaHS)"
                  class="rounded border-gray-300 dark:border-gray-600 dark:bg-transparent text-blue-500 focus:ring-blue-500 w-4 h-4 cursor-pointer" />
              </td>

              <!-- Tên + Email -->
              <td class="py-4 px-3">
                <div class="flex items-center gap-3">
                  <div :class="['w-9 h-9 rounded-full flex items-center justify-center font-bold text-sm shrink-0', getAvatarColor(student.MaHS)]">
                    {{ (student.HoTen || 'A').charAt(0) }}
                  </div>
                  <div>
                    <p class="font-bold text-[#2B3674] dark:text-gray-100">{{ student.HoTen }}</p>
                    <p class="text-[11px] text-gray-400 dark:text-gray-500 truncate max-w-[150px]">
                      {{ student.Email_PhuHuynh || 'Chưa cập nhật Email' }}
                    </p>
                  </div>
                </div>
              </td>

              <td class="py-4 px-3 text-center text-xs font-mono font-bold text-gray-500 dark:text-gray-400">
                {{ student.MaHS }}
              </td>
              <td class="py-4 px-3 text-center font-bold text-[#1E88E5] dark:text-blue-400">
                {{ student.MaLop || '-' }}
              </td>
              <td class="py-4 px-3 text-center text-gray-600 dark:text-gray-300">
                {{ student.SDT_PhuHuynh || '-' }}
              </td>
              <td class="py-4 px-3 text-center text-gray-600 dark:text-gray-400">
                {{ formatDate(student.NgaySinh) }}
              </td>

              <!-- Trạng thái -->
              <td class="py-4 px-3 text-center">
                <span
                  class="inline-flex items-center px-2.5 py-1 rounded text-[11px] font-bold uppercase tracking-wider"
                  :class="(student.TrangThai || 'Đang học') === 'Đang học'
                    ? 'bg-green-50 dark:bg-green-500/10 text-green-600 dark:text-green-400 border border-green-200 dark:border-transparent'
                    : 'bg-gray-100 dark:bg-white/10 text-gray-500 dark:text-gray-400 border border-gray-200 dark:border-transparent'"
                >
                  {{ student.TrangThai || 'Đang học' }}
                </span>
              </td>

              <!-- Điểm TB -->
              <td class="py-4 px-3 text-center">
                <span v-if="student.DiemTB != null" :class="['font-extrabold text-base', getGpaColor(student.DiemTB)]">
                  {{ Number(student.DiemTB).toFixed(1) }}
                </span>
                <span v-else class="text-gray-300 dark:text-gray-600 font-bold">—</span>
              </td>

              <!-- Hạnh Kiểm -->
              <td class="py-4 px-3 text-center">
                <span
                  class="inline-flex items-center px-2.5 py-1 rounded text-[11px] font-bold uppercase tracking-wider"
                  :class="getHanhKiemClass(student.HanhKiem)"
                >
                  {{ student.HanhKiem || '—' }}
                </span>
              </td>

              <!-- Hành động -->
              <td v-if="canEditStudents" class="py-4 pr-6 pl-3">
                <div class="flex items-center justify-end gap-2">
                  <button @click="openEdit(student)"
                    class="p-1.5 text-gray-400 dark:text-gray-500 hover:text-blue-500 hover:bg-blue-50 dark:hover:bg-blue-500/10 rounded-md transition-colors" title="Chỉnh sửa">
                    <Edit2 :size="16" />
                  </button>
                  <button @click="confirmDelete(student)"
                    class="p-1.5 text-gray-400 dark:text-gray-500 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-500/10 rounded-md transition-colors" title="Xóa">
                    <Trash2 :size="16" />
                  </button>
                </div>
              </td>
            </tr>

            <!-- Empty state -->
            <tr v-if="filteredStudents.length === 0 && !loading">
              <td :colspan="canEditStudents ? 10 : 8" class="py-12 text-center text-gray-500 dark:text-gray-400 font-medium">
                Không tìm thấy hồ sơ học sinh nào.
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- FOOTER / PAGINATION -->
      <div class="p-5 border-t border-gray-100 dark:border-white/5 flex items-center justify-between text-sm text-gray-500 dark:text-gray-400 bg-gray-50/30 dark:bg-transparent">
        <div>
          Trang <span class="font-bold text-[#2B3674] dark:text-white">{{ currentPage }}</span> /
          <span class="font-bold text-[#2B3674] dark:text-white">{{ totalPages }}</span>
          &nbsp;&mdash;&nbsp;
          <span class="font-bold text-[#2B3674] dark:text-white">{{ filteredStudents.length }}</span> bản ghi
        </div>
        <div class="flex items-center gap-1">
          <!-- Prev -->
          <button
            @click="prevPage"
            :disabled="currentPage === 1"
            class="w-8 h-8 flex items-center justify-center rounded text-gray-400 dark:text-gray-500 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors disabled:opacity-30 disabled:cursor-not-allowed"
          >&lt;</button>

          <!-- Page numbers -->
          <template v-for="(p, idx) in pageNumbers" :key="p">
            <!-- Ellipsis -->
            <span
              v-if="idx > 0 && pageNumbers[idx - 1] !== p - 1"
              class="w-8 h-8 flex items-center justify-center text-gray-400 text-xs"
            >...</span>
            <button
              @click="goToPage(p)"
              :class="[
                'w-8 h-8 flex items-center justify-center rounded font-medium transition-colors',
                p === currentPage
                  ? 'bg-[#1E88E5] text-white shadow-sm shadow-blue-500/30 dark:shadow-none'
                  : 'text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-white/5'
              ]"
            >{{ p }}</button>
          </template>

          <!-- Next -->
          <button
            @click="nextPage"
            :disabled="currentPage === totalPages"
            class="w-8 h-8 flex items-center justify-center rounded text-gray-400 dark:text-gray-500 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors disabled:opacity-30 disabled:cursor-not-allowed"
          >&gt;</button>
        </div>
      </div>
    </div>


  <!-- ── EDIT MODAL ─────────────────────────────────── -->
  <Transition name="fade">
    <div v-if="canEditStudents && showEditModal"
      class="fixed inset-0 z-50 flex items-center justify-center p-4"
      @click.self="showEditModal = false"
    >
      <!-- Backdrop -->
      <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>

      <!-- Dialog -->
      <div class="relative bg-white dark:bg-[#111C44] rounded-2xl shadow-2xl w-full max-w-lg border border-gray-100 dark:border-white/10">
        <!-- Header -->
        <div class="flex items-center justify-between p-6 border-b border-gray-100 dark:border-white/5">
          <div>
            <h3 class="text-lg font-extrabold text-[#2B3674] dark:text-white">Chỉnh sửa Học Sinh</h3>
            <p class="text-xs text-gray-400 mt-0.5 font-mono">{{ editForm.MaHS }}</p>
          </div>
          <button @click="showEditModal = false"
            class="w-8 h-8 flex items-center justify-center rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 dark:hover:bg-white/10 transition-colors text-xl font-bold">
            &times;
          </button>
        </div>

        <!-- Form -->
        <div class="p-6 space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <!-- Họ tên -->
            <div class="col-span-2">
              <label class="block text-xs font-bold text-gray-500 dark:text-gray-400 mb-1.5 uppercase tracking-wider">Họ và tên</label>
              <input v-model="editForm.HoTen" type="text"
                class="w-full px-3 py-2 bg-gray-50 dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 rounded-lg text-sm font-medium text-[#2B3674] dark:text-white focus:border-blue-500 outline-none transition-colors" />
            </div>

            <!-- Ngày sinh -->
            <div>
              <label class="block text-xs font-bold text-gray-500 dark:text-gray-400 mb-1.5 uppercase tracking-wider">Ngày sinh</label>
              <input v-model="editForm.NgaySinh" type="date"
                class="w-full px-3 py-2 bg-gray-50 dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 rounded-lg text-sm font-medium text-[#2B3674] dark:text-white focus:border-blue-500 outline-none transition-colors" />
            </div>

            <!-- Mã lớp -->
            <div>
              <label class="block text-xs font-bold text-gray-500 dark:text-gray-400 mb-1.5 uppercase tracking-wider">Lớp</label>
              <input v-model="editForm.MaLop" type="text"
                class="w-full px-3 py-2 bg-gray-50 dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 rounded-lg text-sm font-medium text-[#2B3674] dark:text-white focus:border-blue-500 outline-none transition-colors" />
            </div>

            <!-- Trạng thái -->
            <div>
              <label class="block text-xs font-bold text-gray-500 dark:text-gray-400 mb-1.5 uppercase tracking-wider">Trạng thái</label>
              <select v-model="editForm.TrangThai"
                class="w-full px-3 py-2 bg-gray-50 dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 rounded-lg text-sm font-medium text-[#2B3674] dark:text-white focus:border-blue-500 outline-none transition-colors">
                <option>Đang học</option>
                <option>Đã nghỉ</option>
              </select>
            </div>

            <!-- SĐT Phụ Huynh -->
            <div>
              <label class="block text-xs font-bold text-gray-500 dark:text-gray-400 mb-1.5 uppercase tracking-wider">SĐT Phụ Huynh</label>
              <input v-model="editForm.SDT_PhuHuynh" type="text"
                class="w-full px-3 py-2 bg-gray-50 dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 rounded-lg text-sm font-medium text-[#2B3674] dark:text-white focus:border-blue-500 outline-none transition-colors" />
            </div>

            <!-- Email Phụ Huynh -->
            <div class="col-span-2">
              <label class="block text-xs font-bold text-gray-500 dark:text-gray-400 mb-1.5 uppercase tracking-wider">Email Phụ Huynh</label>
              <input v-model="editForm.Email_PhuHuynh" type="email"
                class="w-full px-3 py-2 bg-gray-50 dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 rounded-lg text-sm font-medium text-[#2B3674] dark:text-white focus:border-blue-500 outline-none transition-colors" />
            </div>

            <!-- Địa chỉ -->
            <div class="col-span-2">
              <label class="block text-xs font-bold text-gray-500 dark:text-gray-400 mb-1.5 uppercase tracking-wider">Địa chỉ</label>
              <input v-model="editForm.DiaChi" type="text"
                class="w-full px-3 py-2 bg-gray-50 dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 rounded-lg text-sm font-medium text-[#2B3674] dark:text-white focus:border-blue-500 outline-none transition-colors" />
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="flex items-center justify-end gap-3 p-6 pt-0">
          <button @click="showEditModal = false"
            class="px-5 py-2 rounded-lg border border-gray-200 dark:border-white/10 text-sm font-bold text-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors">
            Hủy
          </button>
          <button @click="saveEdit" :disabled="saving"
            class="px-5 py-2 rounded-lg bg-[#1E88E5] hover:bg-blue-600 text-white text-sm font-bold transition-colors shadow-sm shadow-blue-500/30 disabled:opacity-60 disabled:cursor-not-allowed flex items-center gap-2">
            <RefreshCcw v-if="saving" :size="14" class="animate-spin" />
            {{ saving ? 'Đang lưu...' : 'Lưu thay đổi' }}
          </button>
        </div>
      </div>
    </div>
  </Transition>

  <!-- ── ADD STUDENT MODAL ─────────────────────────── -->
  <Transition name="fade">
    <div v-if="canEditStudents && showAddModal"
      class="fixed inset-0 z-50 flex items-center justify-center p-4"
      @click.self="showAddModal = false">
      <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>

      <div class="relative bg-white dark:bg-[#111C44] rounded-2xl shadow-2xl w-full max-w-lg border border-gray-100 dark:border-white/10">
        <!-- Header -->
        <div class="flex items-center justify-between p-6 border-b border-gray-100 dark:border-white/5">
          <div>
            <h3 class="text-lg font-extrabold text-[#2B3674] dark:text-white">Thêm Học Sinh Mới</h3>
            <p class="text-xs text-gray-400 mt-0.5">Điền đầy đủ thông tin bên dưới</p>
          </div>
          <button @click="showAddModal = false"
            class="w-8 h-8 flex items-center justify-center rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 dark:hover:bg-white/10 transition-colors text-xl font-bold">
            &times;
          </button>
        </div>

        <!-- Form -->
        <div class="p-6 space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-xs font-bold text-gray-500 dark:text-gray-400 mb-1.5 uppercase tracking-wider">Mã Học Sinh <span class="text-red-500">*</span></label>
              <input v-model="addForm.MaHS" type="text" placeholder="VD: HS031"
                class="w-full px-3 py-2 bg-gray-50 dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 rounded-lg text-sm font-medium text-[#2B3674] dark:text-white focus:border-blue-500 outline-none transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-bold text-gray-500 dark:text-gray-400 mb-1.5 uppercase tracking-wider">Lớp <span class="text-red-500">*</span></label>
              <input v-model="addForm.MaLop" type="text" placeholder="VD: 10A1"
                class="w-full px-3 py-2 bg-gray-50 dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 rounded-lg text-sm font-medium text-[#2B3674] dark:text-white focus:border-blue-500 outline-none transition-colors" />
            </div>
            <div class="col-span-2">
              <label class="block text-xs font-bold text-gray-500 dark:text-gray-400 mb-1.5 uppercase tracking-wider">Họ và Tên <span class="text-red-500">*</span></label>
              <input v-model="addForm.HoTen" type="text" placeholder="Nguyễn Văn A"
                class="w-full px-3 py-2 bg-gray-50 dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 rounded-lg text-sm font-medium text-[#2B3674] dark:text-white focus:border-blue-500 outline-none transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-bold text-gray-500 dark:text-gray-400 mb-1.5 uppercase tracking-wider">Ngày Sinh</label>
              <input v-model="addForm.NgaySinh" type="date"
                class="w-full px-3 py-2 bg-gray-50 dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 rounded-lg text-sm font-medium text-[#2B3674] dark:text-white focus:border-blue-500 outline-none transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-bold text-gray-500 dark:text-gray-400 mb-1.5 uppercase tracking-wider">Trạng Thái</label>
              <select v-model="addForm.TrangThai"
                class="w-full px-3 py-2 bg-gray-50 dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 rounded-lg text-sm font-medium text-[#2B3674] dark:text-white focus:border-blue-500 outline-none transition-colors">
                <option>Đang học</option>
                <option>Đã nghỉ</option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-bold text-gray-500 dark:text-gray-400 mb-1.5 uppercase tracking-wider">SĐT Phụ Huynh</label>
              <input v-model="addForm.SDT_PhuHuynh" type="text" placeholder="0912345678"
                class="w-full px-3 py-2 bg-gray-50 dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 rounded-lg text-sm font-medium text-[#2B3674] dark:text-white focus:border-blue-500 outline-none transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-bold text-gray-500 dark:text-gray-400 mb-1.5 uppercase tracking-wider">Email Phụ Huynh</label>
              <input v-model="addForm.Email_PhuHuynh" type="email" placeholder="parent@mail.com"
                class="w-full px-3 py-2 bg-gray-50 dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 rounded-lg text-sm font-medium text-[#2B3674] dark:text-white focus:border-blue-500 outline-none transition-colors" />
            </div>
            <div class="col-span-2">
              <label class="block text-xs font-bold text-gray-500 dark:text-gray-400 mb-1.5 uppercase tracking-wider">Địa Chỉ</label>
              <input v-model="addForm.DiaChi" type="text" placeholder="Số nhà, đường, quận, tỉnh/thành"
                class="w-full px-3 py-2 bg-gray-50 dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 rounded-lg text-sm font-medium text-[#2B3674] dark:text-white focus:border-blue-500 outline-none transition-colors" />
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="flex items-center justify-end gap-3 p-6 pt-0">
          <button @click="showAddModal = false"
            class="px-5 py-2 rounded-lg border border-gray-200 dark:border-white/10 text-sm font-bold text-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors">
            Hủy
          </button>
          <button @click="saveAdd" :disabled="adding"
            class="px-5 py-2 rounded-lg bg-[#1E88E5] hover:bg-blue-600 text-white text-sm font-bold transition-colors shadow-sm shadow-blue-500/30 disabled:opacity-60 disabled:cursor-not-allowed flex items-center gap-2">
            <RefreshCcw v-if="adding" :size="14" class="animate-spin" />
            {{ adding ? 'Đang lưu...' : 'Thêm học sinh' }}
          </button>
        </div>
      </div>
    </div>
    </Transition>
  </div>
</template>
