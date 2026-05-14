<script setup>
import { computed, ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { 
  LayoutDashboard, Users, UserSquare, BookOpen, 
  Calendar, FileBarChart, DollarSign, Bell,
  Moon, Sun, LogOut, PanelLeftClose, PanelLeftOpen,
  UserPlus, FileText, CreditCard,
  Calculator, Navigation
} from 'lucide-vue-next'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

const isCollapsed = ref(false)
const activeMenu = computed(() => route.path)

const isDark = ref(false)
function toggleTheme() {
  isDark.value = !isDark.value
  if (isDark.value) {
    document.documentElement.classList.add('dark')
    localStorage.setItem('theme', 'dark')
  } else {
    document.documentElement.classList.remove('dark')
    localStorage.setItem('theme', 'light')
  }
}

onMounted(() => {
  if (localStorage.getItem('theme') === 'dark' || (!('theme' in localStorage) && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
    isDark.value = true
    document.documentElement.classList.add('dark')
  }
})

async function handleCommand(command) {
  if (command === 'logout') {
    auth.logout()
    await router.push('/login')
    return
  }
  if (command === 'profile') {
    await router.push('/profile')
    return
  }
  if (command === 'settings') {
    await router.push('/settings')
  }
}
</script>

<template>
  <el-container class="h-screen bg-[#F4F7FE] dark:bg-[#0B1437] transition-colors duration-300 font-sans text-[#2B3674] dark:text-white">
    <!-- SIDEBAR -->
    <el-aside :class="['bg-white dark:bg-[#111C44] hidden md:flex flex-col h-full border-r border-gray-100 dark:border-white/5 overflow-x-hidden overflow-y-auto transition-all duration-300 select-none', isCollapsed ? 'w-[88px]' : 'w-[260px]']" width="auto">
      <!-- BRAND -->
      <div class="flex items-center gap-3 px-6 py-8" :class="{ 'px-5 justify-center': isCollapsed }">
        <div class="w-10 h-10 shrink-0 bg-[#1E88E5] rounded-xl flex items-center justify-center text-white shadow-md shadow-blue-500/20">
          <BookOpen :size="20" stroke-width="2.5" />
        </div>
        <div v-show="!isCollapsed" class="whitespace-nowrap overflow-hidden origin-left transition-all duration-300 delay-100">
          <h1 class="font-bold text-xl tracking-tight leading-tight dark:text-white">EduTrack</h1>
          <p class="text-xs text-gray-400 dark:text-gray-400 font-medium">School Management</p>
        </div>
      </div>

      <!-- MENU -->
      <div class="px-3 pb-8 flex-1" :class="{ 'px-3': isCollapsed }">
        <!-- OVERVIEW -->
        <template v-if="auth.hasPermission('Dashboard.View')">
          <p v-show="!isCollapsed" class="px-3 text-xs font-bold text-gray-400 mb-2 mt-4 transition-all">OVERVIEW</p>
          <div v-show="isCollapsed" class="h-[1px] bg-gray-100 dark:bg-white/5 my-4 mx-4"></div>
          <router-link to="/dashboard" class="menu-item" :class="{ 'active': activeMenu === '/dashboard' || activeMenu === '/', 'justify-center !px-0': isCollapsed }">
            <div class="flex items-center justify-center w-6 h-6 shrink-0"><LayoutDashboard :size="20" /></div>
            <span v-show="!isCollapsed" class="font-medium whitespace-nowrap">Dashboard</span>
          </router-link>
        </template>

        <!-- PEOPLE -->
        <p v-show="!isCollapsed" class="px-3 text-xs font-bold text-gray-400 mb-2 mt-6 transition-all">PEOPLE</p>
        <div v-show="isCollapsed" class="h-[1px] bg-gray-100 dark:bg-white/5 my-4 mx-4"></div>
        <router-link to="/hoc-sinh" class="menu-item" :class="{ 'active': activeMenu.includes('/hoc-sinh'), 'justify-center !px-0': isCollapsed }">
          <div class="flex items-center justify-center w-6 h-6 shrink-0"><Users :size="20" /></div>
          <span v-show="!isCollapsed" class="font-medium whitespace-nowrap">Students</span>
        </router-link>
        <router-link v-if="auth.hasPermission('Teachers.View')" to="/giao-vien" class="menu-item" :class="{ 'active': activeMenu.includes('/giao-vien'), 'justify-center !px-0': isCollapsed }">
          <div class="flex items-center justify-center w-6 h-6 shrink-0"><UserSquare :size="20" /></div>
          <span v-show="!isCollapsed" class="font-medium whitespace-nowrap">Teachers</span>
        </router-link>

        <!-- ACADEMIC -->
        <p v-show="!isCollapsed" class="px-3 text-xs font-bold text-gray-400 mb-2 mt-6 transition-all">ACADEMIC</p>
        <div v-show="isCollapsed" class="h-[1px] bg-gray-100 dark:bg-white/5 my-4 mx-4"></div>
        <router-link v-if="auth.isAdmin || auth.isBGH || auth.isTeacher" to="/lop-hoc" class="menu-item" :class="{ 'active': activeMenu.includes('/lop-hoc'), 'justify-center !px-0': isCollapsed }">
          <div class="flex items-center justify-center w-6 h-6 shrink-0"><BookOpen :size="20" /></div>
          <span v-show="!isCollapsed" class="font-medium whitespace-nowrap">Classes</span>
        </router-link>
        <router-link v-if="auth.isAdmin || auth.isBGH || auth.isTeacher" to="/lich-hoc" class="menu-item" :class="{ 'active': activeMenu.includes('/lich-hoc'), 'justify-center !px-0': isCollapsed }">
          <div class="flex items-center justify-center w-6 h-6 shrink-0"><Calendar :size="20" /></div>
          <span v-show="!isCollapsed" class="font-medium whitespace-nowrap">Schedule</span>
        </router-link>
        <router-link v-if="auth.hasPermission('Scores.View')" to="/diem-so" class="menu-item" :class="{ 'active': activeMenu.includes('/diem-so'), 'justify-center !px-0': isCollapsed }">
          <div class="flex items-center justify-center w-6 h-6 shrink-0"><FileBarChart :size="20" /></div>
          <span v-show="!isCollapsed" class="font-medium whitespace-nowrap">Grades</span>
        </router-link>

        <!-- FINANCE -->
        <template v-if="auth.hasPermission('Finance.View')">
          <p v-show="!isCollapsed" class="px-3 text-xs font-bold text-gray-400 mb-2 mt-6 transition-all">FINANCE</p>
          <div v-show="isCollapsed" class="h-[1px] bg-gray-100 dark:bg-white/5 my-4 mx-4"></div>
          <router-link to="/hoc-phi" class="menu-item" :class="{ 'active': activeMenu.includes('/hoc-phi'), 'justify-center !px-0': isCollapsed }">
            <div class="flex items-center justify-center w-6 h-6 shrink-0"><DollarSign :size="20" /></div>
            <span v-show="!isCollapsed" class="font-medium whitespace-nowrap">Tuition</span>
          </router-link>
        </template>

        <!-- DECISION SUPPORT (DSS) -->
        <template v-if="auth.hasPermission('Scores.View') || auth.hasPermission('Scores.Edit')">
          <p v-show="!isCollapsed" class="px-3 text-xs font-bold text-gray-400 mb-2 mt-6 transition-all">ANALYTICS (DSS)</p>
          <div v-show="isCollapsed" class="h-[1px] bg-gray-100 dark:bg-white/5 my-4 mx-4"></div>
          <router-link v-if="auth.hasPermission('Scores.Edit')" to="/dss/what-if" class="menu-item" :class="{ 'active': activeMenu.includes('/dss/what-if'), 'justify-center !px-0': isCollapsed }">
            <div class="flex items-center justify-center w-6 h-6 shrink-0"><Calculator :size="20" /></div>
            <span v-show="!isCollapsed" class="font-medium whitespace-nowrap">What-If Analysis</span>
          </router-link>
          <router-link v-if="auth.hasPermission('Scores.View')" to="/dss/canh-bao" class="menu-item" :class="{ 'active': activeMenu.includes('/dss/canh-bao'), 'justify-center !px-0': isCollapsed }">
            <div class="flex items-center justify-center w-6 h-6 shrink-0"><Navigation :size="20" /></div>
            <span v-show="!isCollapsed" class="font-medium whitespace-nowrap">Early Warning</span>
          </router-link>
        </template>

        <!-- SYSTEM -->
        <p v-show="!isCollapsed" class="px-3 text-xs font-bold text-gray-400 mb-2 mt-6 transition-all">SYSTEM</p>
        <div v-show="isCollapsed" class="h-[1px] bg-gray-100 my-4 mx-4"></div>
        <router-link to="/thong-bao" class="menu-item" :class="{ 'active': activeMenu.includes('/thong-bao'), 'justify-center !px-0': isCollapsed }">
          <div class="flex items-center justify-center w-6 h-6 shrink-0 relative">
            <Bell :size="20" />
            <span class="absolute -top-1.5 -right-1.5 flex h-2.5 w-2.5">
              <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-red-400 opacity-75"></span>
              <span class="relative inline-flex rounded-full h-2.5 w-2.5 bg-red-500 border border-white"></span>
            </span>
          </div>
          <span v-show="!isCollapsed" class="font-medium whitespace-nowrap">Notifications</span>
        </router-link>
      </div>
      
      <!-- COLLAPSE/FOOTER -->
      <div 
        @click="isCollapsed = !isCollapsed" 
        class="p-5 border-t border-gray-100 dark:border-white/5 flex items-center text-gray-400 dark:text-gray-400 hover:text-blue-500 dark:hover:text-blue-400 cursor-pointer transition-colors"
        :class="{ 'justify-center': isCollapsed }"
      >
        <component :is="isCollapsed ? PanelLeftOpen : PanelLeftClose" :size="20" :class="{ 'mr-3': !isCollapsed }" />
        <span v-show="!isCollapsed" class="text-sm font-medium whitespace-nowrap">Collapse Menu</span>
      </div>
    </el-aside>

    <!-- CONTENT WRAPPER -->
    <el-container class="flex flex-col overflow-hidden">
      <!-- HEADER -->
      <el-header height="80px" class="flex items-center justify-between px-8 bg-transparent">
        <div class="flex items-center text-sm font-medium text-gray-400 dark:text-gray-400">
          <span>Home</span>
          <span class="mx-2">/</span>
          <span class="text-[#2B3674] dark:text-white">{{ route.meta.title || 'Dashboard' }}</span>
        </div>

        <div class="flex items-center gap-6 bg-white dark:bg-[#111C44] rounded-full px-5 py-2.5 shadow-sm border border-transparent dark:border-white/5">
          <button @click="toggleTheme" class="text-gray-400 dark:text-gray-400 hover:text-blue-500 dark:hover:text-yellow-400 transition-colors">
            <component :is="isDark ? Sun : Moon" :size="20" />
          </button>
          
          <el-popover placement="bottom-end" :width="320" trigger="click" popper-style="padding: 0; border-radius: 12px; border: none; overflow: hidden; box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.1), 0 8px 10px -6px rgba(0, 0, 0, 0.1);">
            <template #reference>
              <button class="text-gray-400 dark:text-gray-400 hover:text-blue-500 dark:hover:text-blue-400 transition-colors relative outline-none mt-1">
                <Bell :size="20" />
                <div class="absolute -top-0.5 -right-0.5 w-2 h-2 bg-red-500 rounded-full border border-white dark:border-[#111C44]"></div>
              </button>
            </template>
            <div class="flex flex-col bg-white dark:bg-[#111C44] border border-gray-100 dark:border-white/10 rounded-xl overflow-hidden">
              <div class="px-4 py-3 border-b border-gray-100 dark:border-white/10 flex justify-between items-center bg-gray-50/50 dark:bg-white/5">
                <span class="font-bold text-sm text-[#2B3674] dark:text-white">Notifications</span>
                <span class="text-xs text-[#1E88E5] dark:text-blue-400 cursor-pointer hover:underline font-bold">Mark all read</span>
              </div>
              <div class="max-h-[320px] overflow-y-auto w-full">
                <!-- Notification 1 -->
                <div class="p-4 border-b border-gray-50 dark:border-white/5 hover:bg-gray-50 dark:hover:bg-white/5 cursor-pointer flex gap-3 transition-colors">
                   <div class="w-9 h-9 rounded-full bg-blue-50 dark:bg-blue-500/10 text-blue-500 dark:text-blue-400 flex items-center justify-center shrink-0">
                     <UserPlus :size="16" />
                   </div>
                   <div>
                     <p class="text-[13px] font-bold text-[#2B3674] dark:text-white mb-0.5 leading-tight flex items-center gap-2">New Student Enrollment <span class="w-1.5 h-1.5 rounded-full bg-red-500"></span></p>
                     <p class="text-xs text-gray-500 dark:text-gray-400 mb-1 leading-snug">Emily Watson has submitted an enrollment application.</p>
                     <p class="text-[10px] font-bold text-gray-400 dark:text-gray-500">10 minutes ago</p>
                   </div>
                </div>
                <!-- Notification 2 -->
                <div class="p-4 border-b border-gray-50 dark:border-white/5 hover:bg-gray-50 dark:hover:bg-white/5 cursor-pointer flex gap-3 transition-colors">
                   <div class="w-9 h-9 rounded-full bg-orange-50 dark:bg-orange-500/10 text-orange-500 dark:text-orange-400 flex items-center justify-center shrink-0">
                     <FileText :size="16" />
                   </div>
                   <div>
                     <p class="text-[13px] font-bold text-[#2B3674] dark:text-white mb-0.5 leading-tight">Grades Submitted</p>
                     <p class="text-xs text-gray-500 dark:text-gray-400 mb-1 leading-snug">Dr. Robert Chen submitted final grades for Math 11B.</p>
                     <p class="text-[10px] font-bold text-gray-400 dark:text-gray-500">2 hours ago</p>
                   </div>
                </div>
                <!-- Notification 3 -->
                <div class="p-4 hover:bg-gray-50 dark:hover:bg-white/5 cursor-pointer flex gap-3 transition-colors">
                   <div class="w-9 h-9 rounded-full bg-emerald-50 dark:bg-emerald-500/10 text-emerald-500 dark:text-emerald-400 flex items-center justify-center shrink-0">
                     <CreditCard :size="16" />
                   </div>
                   <div>
                     <p class="text-[13px] font-bold text-[#2B3674] dark:text-white mb-0.5 leading-tight">Tuition Received</p>
                     <p class="text-xs text-gray-500 dark:text-gray-400 mb-1 leading-snug">$4,200 payment received for 4 student accounts.</p>
                     <p class="text-[10px] font-bold text-gray-400 dark:text-gray-500">Yesterday</p>
                   </div>
                </div>
              </div>
              <router-link to="/thong-bao" class="px-4 py-3 border-t border-gray-100 dark:border-white/10 text-center block w-full bg-gray-50/50 dark:bg-white/5 hover:bg-gray-100 dark:hover:bg-white/10 text-xs font-bold text-[#1E88E5] dark:text-blue-400 hover:text-blue-600 dark:hover:text-blue-300 transition-colors cursor-pointer">
                View All Notifications
              </router-link>
            </div>
          </el-popover>
          <el-dropdown @command="handleCommand" trigger="click">
            <div class="flex items-center gap-3 cursor-pointer select-none">
              <div class="text-right hidden sm:block">
                <p class="text-sm font-bold text-[#2B3674] dark:text-white leading-tight">{{ auth.username }}</p>
                <p class="text-xs text-gray-400 dark:text-gray-400">{{ auth.roleLabel }}</p>
              </div>
              <el-avatar :size="38" src="https://i.pravatar.cc/150?img=11" />
            </div>
            <template #dropdown>
              <el-dropdown-menu class="dark:bg-[#111C44] dark:border-white/10">
                <el-dropdown-item command="profile">Hồ sơ cá nhân</el-dropdown-item>
                <el-dropdown-item command="settings">Cài đặt</el-dropdown-item>
                <el-dropdown-item divided command="logout" class="text-red-500">
                  <div class="flex items-center gap-2">
                    <LogOut :size="16" />
                    Đăng xuất
                  </div>
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </el-header>

      <!-- MAIN ROUTER VIEW -->
      <el-main class="px-8 pb-8 pt-2 overflow-auto">
        <router-view v-slot="{ Component }">
          <transition name="fade" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </el-main>
    </el-container>
  </el-container>
</template>

<style scoped>
.menu-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  margin-bottom: 4px;
  border-radius: 12px;
  color: #A3AED0;
  transition: all 0.2s ease;
  text-decoration: none;
}

.menu-item:hover {
  background-color: #F8F9FA;
  color: #1E88E5;
}

html.dark .menu-item:hover {
  background-color: rgba(255, 255, 255, 0.05);
  color: #60A5FA;
}

.menu-item.active {
  background-color: #F4F7FE;
  color: #1E88E5;
  position: relative;
}

html.dark .menu-item.active {
  background-color: rgba(255, 255, 255, 0.05);
  color: #60A5FA;
}

.menu-item.active::before {
  content: '';
  position: absolute;
  left: -12px; /* Pull into container padding */
  top: 50%;
  transform: translateY(-50%);
  height: 24px;
  width: 4px;
  background-color: #1E88E5;
  border-radius: 0 4px 4px 0;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
