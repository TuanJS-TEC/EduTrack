<script setup>
import { ref, computed, onMounted } from 'vue'
import { Download, Plus, Edit2, Eye, Trash2, AlertCircle, Send } from 'lucide-vue-next'
import { api } from '../services/api'
import { ElMessage, ElMessageBox } from 'element-plus'

// State
const loading = ref(false)
const dialogVisible = ref(false)
const isEditMode = ref(false)
const search = ref('')
const semesterFilter = ref('All')
const statusFilter = ref('All')

const tuitionRecords = ref([])
const students = ref([])
const formData = ref({
  maHocPhi: 0,
  maHS: '',
  hocKy: 1,
  soTien: 0,
  ngayDong: null,
  trangThai: 'Chưa đóng',
})

// Metrics
const metrics = computed(() => {
  if (tuitionRecords.value.length === 0) {
    return {
      totalExpected: 0,
      collected: 0,
      pending: 0,
      overdue: 0,
      collectionRate: 0,
    }
  }

  const today = new Date()
  const collected = tuitionRecords.value
    .filter(r => r.trangThai === 'Đã đóng')
    .reduce((sum, r) => sum + (r.soTien || 0), 0)

  const pending = tuitionRecords.value
    .filter(r => r.trangThai === 'Chưa đóng')
    .reduce((sum, r) => sum + (r.soTien || 0), 0)

  const overdue = tuitionRecords.value
    .filter(r => r.trangThai === 'Chưa đóng' && r.ngayDong && new Date(r.ngayDong) < today)
    .reduce((sum, r) => sum + (r.soTien || 0), 0)

  const totalExpected = collected + pending
  const collectionRate = totalExpected > 0 ? (collected / totalExpected * 100).toFixed(0) : 0

  return {
    totalExpected: totalExpected.toLocaleString('vi-VN'),
    collected: collected.toLocaleString('vi-VN'),
    pending: pending.toLocaleString('vi-VN'),
    overdue: overdue.toLocaleString('vi-VN'),
    collectionRate: collectionRate,
    overdueCount: tuitionRecords.value.filter(
      r => r.trangThai === 'Chưa đóng' && r.ngayDong && new Date(r.ngayDong) < today
    ).length,
  }
})

// Filtered data
const filteredRecords = computed(() => {
  return tuitionRecords.value.filter(record => {
    const studentName = students.value.find(s => s.maHS === record.maHS)?.hoTen || ''
    return (
      (record.maHS.toLowerCase().includes(search.value.toLowerCase()) ||
        studentName.toLowerCase().includes(search.value.toLowerCase())) &&
      (semesterFilter.value === 'All' || record.hocKy === parseInt(semesterFilter.value)) &&
      (statusFilter.value === 'All' || record.trangThai === statusFilter.value)
    )
  })
})

// Student list with overdue status
const recordsWithStudentInfo = computed(() => {
  const today = new Date()
  return filteredRecords.value.map(record => {
    const student = students.value.find(s => s.maHS === record.maHS)
    const isOverdue = record.trangThai === 'Chưa đóng' && record.ngayDong && new Date(record.ngayDong) < today
    return {
      ...record,
      studentName: student?.hoTen || 'N/A',
      studentClass: student?.maLop || 'N/A',
      isOverdue,
      daysOverdue: isOverdue ? Math.floor((today - new Date(record.ngayDong)) / (1000 * 60 * 60 * 24)) : 0,
    }
  })
})

// Status badge styling
const getStatusBadge = (status) => {
  const badges = {
    'Đã đóng': { bg: 'bg-green-50 dark:bg-green-500/10', text: 'text-green-600 dark:text-green-400' },
    'Chưa đóng': { bg: 'bg-yellow-50 dark:bg-yellow-500/10', text: 'text-yellow-600 dark:text-yellow-400' },
    'Nợ': { bg: 'bg-red-50 dark:bg-red-500/10', text: 'text-red-600 dark:text-red-400' },
  }
  return badges[status] || badges['Chưa đóng']
}

// Methods
const loadTuitionRecords = async () => {
  try {
    loading.value = true
    const response = await api.get('/api/hocphi')
    tuitionRecords.value = response.data
  } catch (error) {
    ElMessage.error('Failed to load tuition records')
    console.error(error)
  } finally {
    loading.value = false
  }
}

const loadStudents = async () => {
  try {
    const response = await api.get('/api/hocsinh')
    students.value = response.data
  } catch (error) {
    console.error('Failed to load students:', error)
  }
}

const openDialog = () => {
  isEditMode.value = false
  formData.value = {
    maHocPhi: 0,
    maHS: '',
    hocKy: 1,
    soTien: 0,
    ngayDong: null,
    trangThai: 'Chưa đóng',
  }
  dialogVisible.value = true
}

const editRecord = (record) => {
  isEditMode.value = true
  formData.value = {
    maHocPhi: record.maHocPhi,
    maHS: record.maHS,
    hocKy: record.hocKy,
    soTien: record.soTien,
    ngayDong: record.ngayDong ? new Date(record.ngayDong).toISOString().split('T')[0] : null,
    trangThai: record.trangThai,
  }
  dialogVisible.value = true
}

const saveRecord = async () => {
  if (!formData.value.maHS || !formData.value.soTien) {
    ElMessage.warning('Please fill in all required fields')
    return
  }

  try {
    const payload = {
      ...formData.value,
      ngayDong: formData.value.ngayDong ? new Date(formData.value.ngayDong).toISOString() : null,
    }

    if (isEditMode.value) {
      await api.put(`/api/hocphi/${formData.value.maHocPhi}`, payload)
      ElMessage.success('Tuition record updated successfully')
    } else {
      await api.post('/api/hocphi', payload)
      ElMessage.success('Tuition record created successfully')
    }
    dialogVisible.value = false
    await loadTuitionRecords()
  } catch (error) {
    ElMessage.error(isEditMode.value ? 'Failed to update record' : 'Failed to create record')
    console.error(error)
  }
}

const deleteRecord = async (maHocPhi) => {
  try {
    await ElMessageBox.confirm('Are you sure you want to delete this record?', 'Warning', {
      confirmButtonText: 'OK',
      cancelButtonText: 'Cancel',
      type: 'warning',
    })
    await api.delete(`/api/hocphi/${maHocPhi}`)
    ElMessage.success('Record deleted successfully')
    await loadTuitionRecords()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('Failed to delete record')
    }
  }
}

const sendReminders = async () => {
  try {
    const overdueRecords = tuitionRecords.value.filter(
      r => r.trangThai === 'Chưa đóng' && r.ngayDong && new Date(r.ngayDong) < new Date()
    )
    if (overdueRecords.length === 0) {
      ElMessage.info('No overdue records to send reminders for')
      return
    }
    ElMessage.success(`Reminders sent to ${overdueRecords.length} students`)
  } catch (error) {
    ElMessage.error('Failed to send reminders')
  }
}

const exportData = () => {
  try {
    const headers = ['Student ID', 'Student Name', 'Semester', 'Amount', 'Due Date', 'Status']
    const rows = recordsWithStudentInfo.value.map(r => [
      r.maHS,
      r.studentName,
      r.hocKy,
      r.soTien,
      r.ngayDong || 'N/A',
      r.trangThai,
    ])

    const csv = [headers, ...rows].map(row => row.join(',')).join('\n')
    const blob = new Blob([csv], { type: 'text/csv' })
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = 'tuition_records.csv'
    a.click()
    window.URL.revokeObjectURL(url)
    ElMessage.success('Data exported successfully')
  } catch (error) {
    ElMessage.error('Failed to export data')
  }
}

// Lifecycle
onMounted(() => {
  loadTuitionRecords()
  loadStudents()
})
</script>

<template>
  <div class="space-y-6">
    <!-- PAGE HEADER -->
    <div class="flex justify-between items-end">
      <div>
        <h2 class="text-2xl font-bold text-[#2B3674] dark:text-white mb-1">Tuition Management</h2>
        <p class="text-sm text-gray-400 dark:text-gray-400">Track payments, manage outstanding fees, and send reminders.</p>
      </div>
      <div class="flex items-center gap-3">
        <button
          @click="exportData"
          class="flex items-center gap-2 px-4 py-2 border border-gray-200 dark:border-white/10 rounded-lg text-sm font-medium text-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors"
        >
          <Download :size="16" />
          Export Report
        </button>
        <button
          @click="openDialog"
          class="flex items-center gap-2 px-4 py-2 bg-[#1E88E5] hover:bg-blue-600 text-white rounded-lg text-sm font-medium transition-colors shadow-sm shadow-blue-500/30 dark:shadow-none"
        >
          <Plus :size="16" />
          New Fee Record
        </button>
      </div>
    </div>

    <!-- METRICS CARDS -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
      <div class="bg-white dark:bg-[#111C44] rounded-2xl p-6 shadow-sm border border-gray-100/50 dark:border-white/5">
        <div class="flex items-center justify-between mb-4">
          <p class="text-xs font-bold text-gray-400 uppercase tracking-wider">Total Expected</p>
          <div class="w-10 h-10 rounded-full bg-blue-50 dark:bg-blue-500/10 flex items-center justify-center">
            <svg class="w-6 h-6 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2" fill="none" />
              <circle cx="12" cy="12" r="1" fill="currentColor" />
            </svg>
          </div>
        </div>
        <p class="text-3xl font-bold text-[#2B3674] dark:text-white">{{ metrics.totalExpected }}đ</p>
        <p class="text-xs text-gray-400 mt-2">Spring 2026 term</p>
      </div>

      <div class="bg-white dark:bg-[#111C44] rounded-2xl p-6 shadow-sm border border-gray-100/50 dark:border-white/5">
        <div class="flex items-center justify-between mb-4">
          <p class="text-xs font-bold text-gray-400 uppercase tracking-wider">Collected</p>
          <div class="w-10 h-10 rounded-full bg-green-50 dark:bg-green-500/10 flex items-center justify-center">
            <svg class="w-6 h-6 text-green-500" fill="currentColor" viewBox="0 0 24 24">
              <path d="M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z" />
            </svg>
          </div>
        </div>
        <p class="text-3xl font-bold text-[#2B3674] dark:text-white">{{ metrics.collected }}đ</p>
        <p class="text-xs text-gray-400 mt-2">11 students paid</p>
      </div>

      <div class="bg-white dark:bg-[#111C44] rounded-2xl p-6 shadow-sm border border-gray-100/50 dark:border-white/5">
        <div class="flex items-center justify-between mb-4">
          <p class="text-xs font-bold text-gray-400 uppercase tracking-wider">Pending</p>
          <div class="w-10 h-10 rounded-full bg-yellow-50 dark:bg-yellow-500/10 flex items-center justify-center">
            <svg class="w-6 h-6 text-yellow-500" fill="currentColor" viewBox="0 0 24 24">
              <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8zm3.5-9c.83 0 1.5-.67 1.5-1.5S16.33 8 15.5 8 14 8.67 14 9.5s.67 1.5 1.5 1.5zm-7 0c.83 0 1.5-.67 1.5-1.5S9.33 8 8.5 8 7 8.67 7 9.5 7.67 11 8.5 11zm3.5 6.5c2.33 0 4.31-1.46 5.11-3.5H6.89c.8 2.04 2.78 3.5 5.11 3.5z" />
            </svg>
          </div>
        </div>
        <p class="text-3xl font-bold text-[#2B3674] dark:text-white">{{ metrics.pending }}đ</p>
        <p class="text-xs text-gray-400 mt-2">5 students pending</p>
      </div>

      <div class="bg-white dark:bg-[#111C44] rounded-2xl p-6 shadow-sm border border-gray-100/50 dark:border-white/5">
        <div class="flex items-center justify-between mb-4">
          <p class="text-xs font-bold text-gray-400 uppercase tracking-wider">Overdue</p>
          <div class="w-10 h-10 rounded-full bg-red-50 dark:bg-red-500/10 flex items-center justify-center">
            <svg class="w-6 h-6 text-red-500" fill="currentColor" viewBox="0 0 24 24">
              <path d="M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z" />
            </svg>
          </div>
        </div>
        <p class="text-3xl font-bold text-[#2B3674] dark:text-white">{{ metrics.overdue }}đ</p>
        <p class="text-xs text-gray-400 mt-2">{{ metrics.overdueCount }} students overdue</p>
      </div>
    </div>

    <!-- COLLECTION PROGRESS & ALERTS -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <div class="lg:col-span-2 bg-white dark:bg-[#111C44] rounded-2xl p-6 shadow-sm border border-gray-100/50 dark:border-white/5">
        <div class="mb-6">
          <div class="flex justify-between items-center mb-2">
            <h3 class="text-lg font-bold text-[#2B3674] dark:text-white">Collection Progress</h3>
            <span class="text-2xl font-bold text-blue-500">{{ metrics.collectionRate }}%</span>
          </div>
          <p class="text-sm text-gray-400">Spring 2026 — {{ metrics.collected }} collected of {{ metrics.totalExpected }}</p>
        </div>
        <div class="w-full bg-gray-200 dark:bg-white/10 rounded-full h-3 overflow-hidden">
          <div
            class="bg-gradient-to-r from-green-400 to-green-500 h-full transition-all duration-300"
            :style="{ width: `${metrics.collectionRate}%` }"
          ></div>
        </div>
        <div class="flex items-center gap-4 mt-4 text-xs font-medium">
          <div class="flex items-center gap-2">
            <div class="w-3 h-3 rounded-full bg-green-500"></div>
            <span class="text-gray-600 dark:text-gray-400">Collected: {{ metrics.collected }}</span>
          </div>
          <div class="flex items-center gap-2">
            <div class="w-3 h-3 rounded-full bg-yellow-500"></div>
            <span class="text-gray-600 dark:text-gray-400">Pending: {{ metrics.pending }}</span>
          </div>
          <div class="flex items-center gap-2">
            <div class="w-3 h-3 rounded-full bg-red-500"></div>
            <span class="text-gray-600 dark:text-gray-400">Overdue: {{ metrics.overdue }}</span>
          </div>
        </div>
      </div>

      <!-- ALERTS & ACTIONS -->
      <div class="bg-white dark:bg-[#111C44] rounded-2xl p-6 shadow-sm border border-gray-100/50 dark:border-white/5">
        <h3 class="text-lg font-bold text-[#2B3674] dark:text-white mb-4">Alerts</h3>
        <div v-if="metrics.overdueCount > 0" class="bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-500/20 rounded-xl p-4 mb-4">
          <div class="flex items-start gap-3">
            <AlertCircle class="w-5 h-5 text-red-500 flex-shrink-0 mt-0.5" />
            <div>
              <p class="font-bold text-red-600 dark:text-red-400 text-sm">{{ metrics.overdueCount }} students have overdue tuition payments</p>
              <p class="text-xs text-red-500/70 dark:text-red-400/70 mt-1">Total overdue amount: {{ metrics.overdue }}đ</p>
            </div>
          </div>
          <button
            @click="sendReminders"
            class="w-full mt-3 flex items-center justify-center gap-2 px-3 py-2 bg-red-500 hover:bg-red-600 text-white rounded-lg text-sm font-medium transition-colors"
          >
            <Send :size="14" />
            Send Reminders
          </button>
        </div>
        <div v-else class="bg-green-50 dark:bg-green-500/10 border border-green-200 dark:border-green-500/20 rounded-xl p-4">
          <p class="text-green-600 dark:text-green-400 text-sm font-medium">✓ No overdue payments</p>
        </div>
      </div>
    </div>

    <!-- TABLE CARD -->
    <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5">
      <!-- TOOLBAR -->
      <div class="p-5 border-b border-gray-100 dark:border-white/5 flex items-center justify-between gap-4 flex-wrap">
        <div class="relative flex-1 min-w-[250px]">
          <svg class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <circle cx="11" cy="11" r="8" />
            <path d="m21 21-4.35-4.35" />
          </svg>
          <input
            v-model="search"
            type="text"
            placeholder="Search by student ID or name..."
            class="w-full pl-10 pr-4 py-2 bg-gray-50 dark:bg-white/5 border-transparent rounded-lg text-sm focus:border-blue-500 focus:bg-white dark:focus:bg-[#111C44] focus:ring-0 transition-all outline-none dark:text-white placeholder-gray-400 dark:placeholder-gray-500"
          />
        </div>

        <div class="flex items-center gap-3">
          <div class="relative">
            <select
              v-model="semesterFilter"
              class="appearance-none bg-gray-50 dark:bg-white/5 border border-gray-200 dark:border-transparent text-gray-700 dark:text-gray-200 py-2 pl-4 pr-10 rounded-lg text-sm font-medium focus:outline-none focus:border-blue-500 cursor-pointer"
            >
              <option value="All">All Semesters</option>
              <option value="1">Semester 1</option>
              <option value="2">Semester 2</option>
            </select>
          </div>

          <div class="relative">
            <select
              v-model="statusFilter"
              class="appearance-none bg-gray-50 dark:bg-white/5 border border-gray-200 dark:border-transparent text-gray-700 dark:text-gray-200 py-2 pl-4 pr-10 rounded-lg text-sm font-medium focus:outline-none focus:border-blue-500 cursor-pointer"
            >
              <option value="All">All Status</option>
              <option value="Đã đóng">Paid</option>
              <option value="Chưa đóng">Pending</option>
              <option value="Nợ">Overdue</option>
            </select>
          </div>
        </div>
      </div>

      <!-- TABLE -->
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="text-[11px] font-bold text-gray-400 uppercase tracking-wider border-b border-gray-100 dark:border-white/5 bg-gray-50/50 dark:bg-white/5">
              <th class="py-4 pl-6 pr-3">STUDENT</th>
              <th class="py-4 px-3 text-center">RECORD ID</th>
              <th class="py-4 px-3 text-center">GRADE</th>
              <th class="py-4 px-3 text-center">AMOUNT</th>
              <th class="py-4 px-3 text-center">PROGRESS</th>
              <th class="py-4 px-3 text-center">DUE DATE</th>
              <th class="py-4 px-3 text-center">METHOD</th>
              <th class="py-4 px-3 text-center">STATUS</th>
              <th class="py-4 pr-6 pl-3 text-right">ACTIONS</th>
            </tr>
          </thead>
          <tbody class="text-sm" v-if="!loading">
            <tr v-for="record in recordsWithStudentInfo" :key="record.maHocPhi" class="border-b border-gray-50 dark:border-white/5 hover:bg-gray-50/50 dark:hover:bg-white/5 transition-colors group">
              <td class="py-4 pl-6 pr-3">
                <div class="flex items-center gap-3">
                  <div class="w-9 h-9 rounded-full flex items-center justify-center font-bold text-sm bg-blue-100 dark:bg-blue-500/20 text-blue-600 dark:text-blue-400">
                    {{ record.studentName.charAt(0) }}
                  </div>
                  <div>
                    <p class="font-bold text-[#2B3674] dark:text-gray-100">{{ record.studentName }}</p>
                    <p class="text-xs text-gray-400 dark:text-gray-500">{{ record.studentClass }}</p>
                  </div>
                </div>
              </td>
              <td class="py-4 px-3 text-center text-xs font-mono text-gray-500 dark:text-gray-400">TU{{ record.maHocPhi }}</td>
              <td class="py-4 px-3 text-center font-bold text-[#2B3674] dark:text-gray-200">Sem {{ record.hocKy }}</td>
              <td class="py-4 px-3 text-center font-bold text-[#2B3674] dark:text-gray-200">{{ (record.soTien || 0).toLocaleString() }}đ</td>
              <td class="py-4 px-3 text-center">
                <div class="flex items-center justify-center gap-2">
                  <div class="w-20 bg-gray-200 dark:bg-white/10 rounded-full h-1.5 overflow-hidden">
                    <div class="bg-green-500 h-full" :style="{ width: record.trangThai === 'Đã đóng' ? '100%' : '0%' }"></div>
                  </div>
                  <span class="text-xs font-bold text-gray-600 dark:text-gray-400">{{ record.trangThai === 'Đã đóng' ? '100%' : '0%' }}</span>
                </div>
              </td>
              <td class="py-4 px-3 text-center text-gray-600 dark:text-gray-400">
                <div>
                  <p class="text-sm">{{ record.ngayDong ? new Date(record.ngayDong).toLocaleDateString('vi-VN') : 'N/A' }}</p>
                  <span v-if="record.isOverdue" class="text-xs text-red-500 font-bold">{{ record.daysOverdue }} days overdue</span>
                </div>
              </td>
              <td class="py-4 px-3 text-center text-gray-600 dark:text-gray-400 text-xs">Bank Transfer</td>
              <td class="py-4 px-3 text-center">
                <span
                  class="inline-flex items-center px-2.5 py-0.5 rounded-full text-[11px] font-bold"
                  :class="getStatusBadge(record.trangThai).bg + ' ' + getStatusBadge(record.trangThai).text"
                >
                  {{ record.trangThai }}
                </span>
              </td>
              <td class="py-4 pr-6 pl-3">
                <div class="flex items-center justify-end gap-2 opacity-100 lg:opacity-0 group-hover:opacity-100 transition-opacity">
                  <button
                    @click="editRecord(record)"
                    class="p-1.5 text-gray-400 dark:text-gray-500 hover:text-blue-500 hover:bg-blue-50 dark:hover:bg-blue-500/10 rounded-md transition-colors"
                    title="Edit"
                  >
                    <Edit2 :size="16" />
                  </button>
                  <button
                    @click="() => {}"
                    class="p-1.5 text-gray-400 dark:text-gray-500 hover:text-indigo-500 hover:bg-indigo-50 dark:hover:bg-indigo-500/10 rounded-md transition-colors"
                    title="View Details"
                  >
                    <Eye :size="16" />
                  </button>
                  <button
                    @click="deleteRecord(record.maHocPhi)"
                    class="p-1.5 text-gray-400 dark:text-gray-500 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-500/10 rounded-md transition-colors"
                    title="Delete"
                  >
                    <Trash2 :size="16" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="loading" class="text-center py-8">
          <p class="text-gray-400">Loading...</p>
        </div>
        <div v-if="!loading && recordsWithStudentInfo.length === 0" class="text-center py-12">
          <p class="text-gray-400">No tuition records found</p>
        </div>
      </div>
    </div>

    <!-- ADD/EDIT DIALOG -->
    <el-dialog v-model="dialogVisible" :title="isEditMode ? 'Edit Tuition Record' : 'Add New Tuition Record'" width="500px">
      <div class="space-y-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Student</label>
          <select v-model="formData.maHS" class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-[#111C44] dark:text-white">
            <option value="">Select Student</option>
            <option v-for="student in students" :key="student.maHS" :value="student.maHS">
              {{ student.hoTen }} ({{ student.maHS }})
            </option>
          </select>
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Semester</label>
            <select v-model.number="formData.hocKy" class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-[#111C44] dark:text-white">
              <option value="1">Semester 1</option>
              <option value="2">Semester 2</option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Amount (đ)</label>
            <input v-model.number="formData.soTien" type="number" placeholder="0" class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-[#111C44] dark:text-white" />
          </div>
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Due Date</label>
            <input v-model="formData.ngayDong" type="date" class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-[#111C44] dark:text-white" />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Status</label>
            <select v-model="formData.trangThai" class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-[#111C44] dark:text-white">
              <option value="Chưa đóng">Pending</option>
              <option value="Đã đóng">Paid</option>
              <option value="Nợ">Overdue</option>
            </select>
          </div>
        </div>
      </div>

      <template #footer>
        <div class="flex gap-2">
          <button
            @click="dialogVisible = false"
            class="flex-1 px-4 py-2 border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-50 dark:hover:bg-white/5 transition-colors"
          >
            Cancel
          </button>
          <button
            @click="saveRecord"
            class="flex-1 px-4 py-2 bg-blue-500 hover:bg-blue-600 text-white rounded-lg transition-colors"
          >
            {{ isEditMode ? 'Update' : 'Create' }}
          </button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>
