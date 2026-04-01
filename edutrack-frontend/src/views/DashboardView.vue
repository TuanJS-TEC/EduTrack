<script setup>
import { Users, BookOpen, UserSquare, DollarSign, ArrowUpRight, Calendar } from 'lucide-vue-next'

const stats = [
  {
    id: 1,
    title: 'TOTAL STUDENTS',
    value: '1,248',
    trend: '+12%',
    trendText: 'vs last month',
    trendUp: true,
    icon: Users,
    iconBg: 'bg-blue-50 text-blue-500',
  },
  {
    id: 2,
    title: 'ACTIVE CLASSES',
    value: '42',
    trend: '+3',
    trendText: 'vs last month',
    trendUp: true,
    icon: BookOpen,
    iconBg: 'bg-teal-50 text-teal-500',
  },
  {
    id: 3,
    title: 'TEACHERS',
    value: '86',
    trend: '+2',
    trendText: 'vs last month',
    trendUp: true,
    icon: UserSquare,
    iconBg: 'bg-green-50 text-green-500',
  },
  {
    id: 4,
    title: 'MONTHLY REVENUE',
    value: '$48,250',
    trend: '+8.4%',
    trendText: 'vs last month',
    trendUp: true,
    icon: DollarSign,
    iconBg: 'bg-orange-50 text-orange-500',
  }
]
</script>

<template>
  <div class="space-y-6">
    <!-- TOP STAT STATS (4 Cards) -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
      <div 
        v-for="stat in stats" 
        :key="stat.id" 
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
            <span class="text-gray-400 ml-1.5">{{ stat.trendText }}</span>
          </div>
        </div>
        <div :class="['w-12 h-12 rounded-full flex items-center justify-center', stat.iconBg]">
          <component :is="stat.icon" :size="24" stroke-width="2" />
        </div>
      </div>
    </div>

    <!-- MAIN DASHBOARD CONTENT (Charts & Quick Actions) -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <!-- Enrollment Growth ChartPlaceholder -->
      <div class="bg-white dark:bg-[#111C44] rounded-2xl p-6 shadow-sm border border-gray-100/50 dark:border-white/5 lg:col-span-2">
        <div class="flex justify-between items-start mb-6">
          <div>
            <h3 class="text-lg font-bold text-[#2B3674] dark:text-white">Enrollment Growth</h3>
            <p class="text-sm text-gray-400 dark:text-gray-400">Students & teachers over the year</p>
          </div>
        </div>
        <div class="h-[280px] w-full flex items-center justify-center border-2 border-dashed border-gray-100 dark:border-white/10 rounded-xl">
           <p class="text-gray-400 flex items-center gap-2"><BookOpen :size="18"/> Khung Đợi Tích hợp Biểu đồ ECharts</p>
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="bg-white dark:bg-[#111C44] rounded-2xl p-6 shadow-sm border border-gray-100/50 dark:border-white/5">
        <div class="flex justify-between items-center mb-6">
          <h3 class="text-lg font-bold text-[#2B3674] dark:text-white">Quick Actions</h3>
          <span class="text-xs text-gray-400 dark:text-gray-400">8 shortcuts</span>
        </div>
        
        <div class="grid grid-cols-2 gap-3">
          <button class="flex flex-col items-center justify-center p-4 border border-blue-100 dark:border-blue-500/10 bg-blue-50/50 dark:bg-blue-500/5 rounded-xl hover:bg-blue-50 dark:hover:bg-blue-500/10 transition-colors text-center group">
            <Users :size="20" class="text-blue-500 dark:text-blue-400 mb-2 group-hover:scale-110 transition-transform" />
            <span class="text-xs font-bold text-[#2B3674] dark:text-gray-200">Add Student</span>
          </button>
          <button class="flex flex-col items-center justify-center p-4 border border-teal-100 dark:border-teal-500/10 bg-teal-50/50 dark:bg-teal-500/5 rounded-xl hover:bg-teal-50 dark:hover:bg-teal-500/10 transition-colors text-center group">
            <UserSquare :size="20" class="text-teal-500 dark:text-teal-400 mb-2 group-hover:scale-110 transition-transform" />
            <span class="text-xs font-bold text-[#2B3674] dark:text-gray-200">Add Teacher</span>
          </button>
          <button class="flex flex-col items-center justify-center p-4 border border-indigo-100 dark:border-indigo-500/10 bg-indigo-50/50 dark:bg-indigo-500/5 rounded-xl hover:bg-indigo-50 dark:hover:bg-indigo-500/10 transition-colors text-center group">
            <BookOpen :size="20" class="text-indigo-500 dark:text-indigo-400 mb-2 group-hover:scale-110 transition-transform" />
            <span class="text-xs font-bold text-[#2B3674] dark:text-gray-200">Manage Classes</span>
          </button>
          <button class="flex flex-col items-center justify-center p-4 border border-purple-100 dark:border-purple-500/10 bg-purple-50/50 dark:bg-purple-500/5 rounded-xl hover:bg-purple-50 dark:hover:bg-purple-500/10 transition-colors text-center group">
            <Calendar :size="20" class="text-purple-500 dark:text-purple-400 mb-2 group-hover:scale-110 transition-transform" />
            <span class="text-xs font-bold text-[#2B3674] dark:text-gray-200">View Schedule</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
