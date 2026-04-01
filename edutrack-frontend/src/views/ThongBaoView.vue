<script setup>
import { ref, onMounted, computed } from 'vue'
import { CheckCheck, Trash2, Bell, CreditCard, UserPlus, FileText, Calendar, Settings, AlertCircle, Search, RefreshCcw } from 'lucide-vue-next'
import { apiService } from '../services/api'

const search = ref('')
const loading = ref(false)
const notifications = ref([])
const activeFilter = ref('Tất cả')

const fetchThongBaos = async () => {
  loading.value = true
  try {
    const res = await apiService.getThongBaos()
    notifications.value = res.data
  } catch (error) {
    console.error("Lỗi khi tải dữ liệu Thông Báo:", error)
  } finally {
    loading.value = false
  }
}

const markAsRead = async (id) => {
  try {
    await apiService.markAsRead(id)
    const notif = notifications.value.find(n => n.MaTB === id)
    if (notif) notif.DaDoc = true
  } catch (error) {
    console.error("Lỗi khi đánh dấu đã đọc:", error)
  }
}

const deleteNotification = async (id) => {
  try {
    await apiService.deleteThongBao(id)
    notifications.value = notifications.value.filter(n => n.MaTB !== id)
  } catch (error) {
    console.error("Lỗi khi xóa thông báo:", error)
  }
}

const markAllAsRead = async () => {
  try {
    for (const notif of filteredNotifications.value) {
      if (!notif.DaDoc) {
        await markAsRead(notif.MaTB)
      }
    }
  } catch (error) {
    console.error("Lỗi khi đánh dấu tất cả đã đọc:", error)
  }
}

onMounted(() => {
  fetchThongBaos()
})

const statCards = [
  { id: 1, title: 'Cảnh báo', count: 0, newCount: 0, icon: AlertCircle, color: 'text-red-500 bg-red-50 dark:text-red-400 dark:bg-red-500/10', typeMatcher: 'alert' },
  { id: 2, title: 'Học phí', count: 0, newCount: 0, icon: CreditCard, color: 'text-emerald-500 bg-emerald-50 dark:text-emerald-400 dark:bg-emerald-500/10', typeMatcher: 'payment' },
  { id: 3, title: 'Tuyển sinh', count: 0, newCount: 0, icon: UserPlus, color: 'text-blue-500 bg-blue-50 dark:text-blue-400 dark:bg-blue-500/10', typeMatcher: 'enrollment' },
  { id: 4, title: 'Điểm số', count: 0, newCount: 0, icon: FileText, color: 'text-orange-500 bg-orange-50 dark:text-orange-400 dark:bg-orange-500/10', typeMatcher: 'grade' },
  { id: 5, title: 'Lịch trình', count: 0, newCount: 0, icon: Calendar, color: 'text-purple-500 bg-purple-50 dark:text-purple-400 dark:bg-purple-500/10', typeMatcher: 'schedule' },
  { id: 6, title: 'Hệ thống', count: 0, newCount: 0, icon: Settings, color: 'text-gray-500 bg-gray-50 dark:text-gray-400 dark:bg-gray-500/10', typeMatcher: 'system' },
]

const sidebarLinks = [
  { name: 'Tất cả', icon: Bell, filter: 'all' },
  { name: 'Chưa đọc', icon: CheckCheck, filter: 'unread' },
  { name: 'Cảnh báo', icon: AlertCircle, filter: 'alert' },
  { name: 'Thanh toán', icon: CreditCard, filter: 'payment' },
  { name: 'Tuyển sinh', icon: UserPlus, filter: 'enrollment' },
  { name: 'Học tập', icon: FileText, filter: 'grade' },
  { name: 'Hệ thống', icon: Settings, filter: 'system' },
]

// UI mapping for notifications API Types
const getTypeConfig = (type) => {
  switch (type?.toLowerCase()) {
    case 'alert':
    case 'cảnh báo':
      return { 
        icon: AlertCircle, 
        iconColor: 'text-red-500 bg-red-50 dark:text-red-400 dark:bg-red-500/10', 
        borderColor: 'border-l-red-500', 
        tagColor: 'text-red-500 bg-red-50 border-red-100 dark:text-red-400 dark:bg-red-500/10 dark:border-red-500/20' 
      }
    case 'payment':
    case 'học phí':
      return { 
        icon: CreditCard, 
        iconColor: 'text-emerald-500 bg-emerald-50 dark:text-emerald-400 dark:bg-emerald-500/10', 
        borderColor: 'border-l-emerald-500',
        tagColor: 'text-emerald-500 bg-emerald-50 border-emerald-100 dark:text-emerald-400 dark:bg-emerald-500/10 dark:border-emerald-500/20'
      }
    case 'enrollment':
    case 'tuyển sinh':
      return { 
        icon: UserPlus, 
        iconColor: 'text-blue-500 bg-blue-50 dark:text-blue-400 dark:bg-blue-500/10', 
        borderColor: 'border-l-blue-500',
        tagColor: 'text-blue-500 bg-blue-50 border-blue-100 dark:text-blue-400 dark:bg-blue-500/10 dark:border-blue-500/20'
      }
    case 'grade':
    case 'điểm số':
      return { 
        icon: FileText, 
        iconColor: 'text-orange-500 bg-orange-50 dark:text-orange-400 dark:bg-orange-500/10', 
        borderColor: 'border-l-orange-500',
        tagColor: 'text-orange-500 bg-orange-50 border-orange-100 dark:text-orange-400 dark:bg-orange-500/10 dark:border-orange-500/20'
      }
    case 'schedule':
    case 'lịch học':
      return { 
        icon: Calendar, 
        iconColor: 'text-purple-500 bg-purple-50 dark:text-purple-400 dark:bg-purple-500/10', 
        borderColor: 'border-l-purple-500',
        tagColor: 'text-purple-500 bg-purple-50 border-purple-100 dark:text-purple-400 dark:bg-purple-500/10 dark:border-purple-500/20'
      }
    default:
      return { 
        icon: Settings, 
        iconColor: 'text-gray-500 bg-gray-50 dark:text-gray-400 dark:bg-gray-500/10', 
        borderColor: 'border-l-gray-400 dark:border-l-gray-600',
        tagColor: 'text-gray-500 bg-gray-50 border-gray-100 dark:text-gray-400 dark:bg-gray-500/10 dark:border-gray-500/20'
      }
  }
}

const formatDate = (dateString) => {
  if (!dateString) return ''
  const date = new Date(dateString)
  return new Intl.DateTimeFormat('vi-VN', {
    dateStyle: 'short',
    timeStyle: 'short'
  }).format(date)
}

const getTypeFilter = (type) => {
  if (!type) return 'system'
  const lower = type.toLowerCase()
  
  // Map tiếng Anh
  if (lower.includes('alert')) return 'alert'
  if (lower.includes('payment')) return 'payment'
  if (lower.includes('enrollment')) return 'enrollment'
  if (lower.includes('grade')) return 'grade'
  if (lower.includes('schedule')) return 'schedule'
  if (lower.includes('system')) return 'system'
  
  // Map tiếng Việt
  if (lower.includes('cảnh báo') || lower.includes('cảnh')) return 'alert'
  if (lower.includes('học phí') || lower.includes('thanh toán')) return 'payment'
  if (lower.includes('tuyển sinh')) return 'enrollment'
  if (lower.includes('điểm') || lower.includes('học tập')) return 'grade'
  if (lower.includes('lịch')) return 'schedule'
  
  return 'system'
}

const filteredNotifications = computed(() => {
  let filtered = notifications.value
  
  // Filter by search
  filtered = filtered.filter(n => {
    return (n.TieuDe || '').toLowerCase().includes(search.value.toLowerCase()) || 
           (n.NoiDung || '').toLowerCase().includes(search.value.toLowerCase())
  })
  
  // Filter by active filter
  if (activeFilter.value === 'Chưa đọc') {
    filtered = filtered.filter(n => !n.DaDoc)
  } else if (activeFilter.value !== 'Tất cả') {
    const filterLink = sidebarLinks.find(link => link.name === activeFilter.value)
    if (filterLink && filterLink.filter !== 'all') {
      filtered = filtered.filter(n => {
        const nType = getTypeFilter(n.LoaiTB)
        return nType === filterLink.filter
      })
    }
  }
  
  return filtered.sort((a, b) => new Date(b.NgayGui) - new Date(a.NgayGui))
})

const computedStatCards = computed(() => {
  return statCards.map(card => {
    const matching = notifications.value.filter(n => {
      const nType = getTypeFilter(n.LoaiTB)
      return nType === card.typeMatcher
    })
    return {
      ...card,
      count: matching.length,
      newCount: matching.filter(m => !m.DaDoc).length
    }
  })
})

const unreadCount = computed(() => {
  return notifications.value.filter(n => !n.DaDoc).length
})
</script>

<template>
  <div class="space-y-6">
    <!-- PAGE HEADER -->
    <div class="flex justify-between items-end">
      <div>
        <h2 class="text-2xl font-bold text-[#2B3674] dark:text-white mb-1">Thông Báo</h2>
        <p class="text-sm text-gray-400 dark:text-gray-400">Xem và quản lý các thông báo từ hệ thống EduTrack.</p>
      </div>
      <div class="flex items-center gap-3">
        <button @click="fetchThongBaos" class="flex items-center gap-2 px-4 py-2 bg-blue-50 text-blue-600 dark:bg-blue-500/10 dark:text-blue-400 border border-blue-200 dark:border-blue-500/30 rounded-lg text-sm font-bold shadow-sm transition-colors">
          <RefreshCcw :size="16" :class="{ 'animate-spin': loading }" />
          Làm mới
        </button>
        <button @click="markAllAsRead" class="flex items-center gap-2 px-4 py-2 bg-white dark:bg-[#111C44] border border-gray-200 dark:border-white/10 rounded-lg text-sm font-bold text-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-white/5 shadow-sm transition-colors hidden sm:flex">
          <CheckCheck :size="16" />
          Đánh dấu tất cả
        </button>
      </div>
    </div>

    <!-- 6 STAT CARDS ROW -->
    <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
      <div v-for="stat in computedStatCards" :key="stat.id" @click="activeFilter = stat.title" class="bg-white dark:bg-[#111C44] rounded-xl p-4 shadow-sm border border-gray-100 dark:border-white/5 flex flex-col items-center justify-center gap-2 cursor-pointer transition-all" :class="activeFilter === stat.title ? 'border-blue-500 dark:border-blue-400 shadow-md' : 'hover:border-blue-200 dark:hover:border-blue-500/50 hover:shadow-md'">
        <div :class="['w-10 h-10 rounded-full flex items-center justify-center font-bold relative', stat.color]">
          <component :is="stat.icon" :size="20" />
          <span v-if="stat.newCount > 0" class="absolute -top-1 -right-1 w-5 h-5 bg-red-500 text-white text-[10px] font-bold rounded-full flex items-center justify-center">{{ stat.newCount }}</span>
        </div>
        <div class="text-center mt-1">
          <h4 class="text-xl font-extrabold text-[#2B3674] dark:text-white">{{ stat.count }}</h4>
          <p class="text-xs font-bold text-gray-400 dark:text-gray-400 mt-0.5">{{ stat.title }}</p>
        </div>
      </div>
    </div>

    <!-- MAIN NOTIFICATION LAYOUT (2 Columns) -->
    <div class="grid grid-cols-1 lg:grid-cols-4 gap-6 items-start">
      
      <!-- LEFT SIDEBAR: FILTERS AND SETTINGS -->
      <div class="lg:col-span-1 space-y-4">
        
        <!-- Filter Menu -->
        <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 p-2 text-sm font-bold text-[#2B3674] dark:text-white">
          <button 
            v-for="link in sidebarLinks" :key="link.name"
            @click="activeFilter = link.name"
            :class="[
              'w-full flex items-center justify-between px-4 py-3 rounded-xl transition-all',
              activeFilter === link.name ? 'bg-blue-50 dark:bg-blue-500/10 text-blue-600 dark:text-blue-400' : 'text-gray-500 dark:text-gray-400 hover:bg-gray-50 dark:hover:bg-white/5'
            ]"
          >
            <div class="flex items-center gap-3">
              <component :is="link.icon" :size="18" :class="activeFilter === link.name ? 'text-blue-500 dark:text-blue-400' : 'text-gray-400 dark:text-gray-500'" />
              <span>{{ link.name }}</span>
              <span v-if="link.filter === 'unread'" class="ml-auto text-xs bg-red-500 text-white px-2 py-0.5 rounded-full">{{ unreadCount }}</span>
            </div>
          </button>
        </div>

        <!-- Notification Preferences -->
        <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 p-5 hidden sm:block">
          <h4 class="text-[11px] font-bold text-[#2B3674] dark:text-white uppercase tracking-wider mb-4 border-b border-gray-100 dark:border-white/5 pb-2">Tuỳ chọn hệ thống</h4>
          
          <div class="space-y-4 text-sm font-bold text-gray-500 dark:text-gray-400">
            <div class="flex items-center justify-between">
              <span>Nhận email</span>
              <div class="w-8 h-4 bg-blue-500 rounded-full relative cursor-pointer"><div class="absolute right-0.5 top-0.5 w-3 h-3 bg-white rounded-full"></div></div>
            </div>
            <div class="flex items-center justify-between">
              <span>Âm báo nổi</span>
              <div class="w-8 h-4 bg-blue-500 rounded-full relative cursor-pointer"><div class="absolute right-0.5 top-0.5 w-3 h-3 bg-white rounded-full"></div></div>
            </div>
            <div class="flex items-center justify-between">
              <span>Rung trên Mobile</span>
              <div class="w-8 h-4 bg-gray-200 dark:bg-gray-600 rounded-full relative cursor-pointer"><div class="absolute left-0.5 top-0.5 w-3 h-3 bg-white shadow-sm rounded-full"></div></div>
            </div>
          </div>
        </div>
      </div>

      <!-- RIGHT FEED: NOTIFICATIONS LIST -->
      <div class="lg:col-span-3 space-y-4">
        
        <!-- Search bar inline -->
        <div class="relative w-full">
          <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 dark:text-gray-500" />
          <input 
            v-model="search"
            type="text" 
            placeholder="Tìm kiếm nội dung thông báo..." 
            class="w-full pl-12 pr-4 py-3.5 bg-white dark:bg-[#111C44] border border-gray-100 dark:border-white/5 shadow-sm rounded-xl text-sm focus:border-blue-500 focus:ring-0 transition-all outline-none dark:text-white placeholder-gray-400 dark:placeholder-gray-500"
          />
        </div>

        <div v-if="loading" class="text-center py-8 text-blue-500 flex justify-center items-center gap-3">
          <RefreshCcw :size="24" class="animate-spin" />
          <span class="font-bold">Đang tải thông báo...</span>
        </div>

        <!-- Notifications feed -->
        <div v-else class="space-y-3">
          <!-- Section header -->
          <div class="flex items-center justify-between mt-6 mb-2">
            <h3 class="text-xs font-bold text-gray-400 dark:text-gray-500 uppercase tracking-wider">HỘP THƯ ĐẾN ({{ filteredNotifications.length }})</h3>
          </div>

          <div 
            v-for="notif in filteredNotifications" 
            :key="notif.MaTB" 
            :class="['bg-white dark:bg-[#111C44] rounded-xl shadow-sm border border-gray-100 dark:border-white/5 border-l-[4px] p-5 flex gap-4 transition-all hover:shadow-md cursor-default relative overflow-hidden', getTypeConfig(notif.LoaiTB).borderColor, notif.DaDoc ? 'opacity-70' : '']"
          >
            <!-- Badge Icon -->
            <div :class="['w-10 h-10 rounded-full flex items-center justify-center shrink-0 mt-1', getTypeConfig(notif.LoaiTB).iconColor]">
              <component :is="getTypeConfig(notif.LoaiTB).icon" :size="20" />
            </div>

            <!-- Content -->
            <div class="flex-1 min-w-0">
              <div class="flex flex-col sm:flex-row sm:justify-between sm:items-start mb-1 gap-2">
                <h4 class="text-[15px] font-bold text-[#2B3674] dark:text-white flex items-center gap-2 truncate">
                  {{ notif.TieuDe }}
                  <span v-if="!notif.DaDoc" class="w-2 h-2 rounded-full bg-blue-500 dark:bg-blue-400 flex-shrink-0"></span>
                </h4>
                <span class="text-[11px] font-bold text-gray-400 dark:text-gray-500 whitespace-nowrap">{{ formatDate(notif.NgayGui) }}</span>
              </div>
              
              <p class="text-sm text-gray-500 dark:text-gray-400 leading-relaxed max-w-4xl break-words mb-4">{{ notif.NoiDung }}</p>

              <!-- Tags and Actions -->
              <div class="flex flex-col sm:flex-row justify-between items-start gap-4 border-t border-gray-50 dark:border-white/5 pt-4">
                <div class="flex flex-wrap items-center gap-2">
                  <span 
                    :class="['flex items-center gap-1.5 px-2.5 py-1 rounded text-[10px] font-bold uppercase tracking-wide border', getTypeConfig(notif.LoaiTB).tagColor]"
                  >
                    <component :is="getTypeConfig(notif.LoaiTB).icon" :size="12" />
                    {{ notif.LoaiTB || 'Hệ thống' }}
                  </span>
                  <span v-if="notif.MaHS" class="flex items-center gap-1.5 px-2.5 py-1 rounded text-[10px] font-bold uppercase tracking-wide border text-gray-500 bg-gray-50 border-gray-100 dark:text-gray-400 dark:bg-gray-500/10 dark:border-gray-500/20">
                     HS: {{ notif.MaHS }}
                  </span>
                </div>
                
                <div class="flex items-center gap-4 text-xs font-bold shrink-0 mt-2 sm:mt-0">
                  <button v-if="!notif.DaDoc" @click="markAsRead(notif.MaTB)" class="flex items-center gap-1 text-[#1E88E5] dark:text-blue-400 hover:text-blue-600 dark:hover:text-blue-300 transition-colors">
                    <CheckCheck :size="14" />
                    Đã đọc
                  </button>
                  <button v-else class="flex items-center gap-1 text-green-600 dark:text-green-400">
                    <CheckCheck :size="14" />
                    Đã đọc
                  </button>
                  <button @click="deleteNotification(notif.MaTB)" class="text-red-500 dark:text-red-400 hover:text-red-600 dark:hover:text-red-300 transition-colors flex items-center gap-1">
                    <Trash2 :size="14" />
                    Xóa
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- Empty state -->
          <div v-if="filteredNotifications.length === 0 && !loading" class="bg-gray-50/50 dark:bg-white/5 rounded-xl border border-dashed border-gray-200 dark:border-white/10 p-12 text-center text-gray-400 dark:text-gray-500 font-bold">
            <Bell :size="48" class="mx-auto mb-4 opacity-50 text-gray-300 dark:text-gray-600" />
            <p class="text-lg text-[#2B3674] dark:text-gray-300">Không có thông báo mới!</p>
            <p class="text-xs font-medium max-w-md mx-auto mt-2">EduTrack hiện chưa có thông báo gửi cho bạn hoặc bộ lọc đang được áp dụng đã loại bỏ tất cả kết quả.</p>
          </div>
        </div>

      </div>

    </div>
  </div>
</template>
