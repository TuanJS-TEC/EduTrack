<script setup>
import { ref } from 'vue'
import { ChevronLeft, ChevronRight, Calendar as CalendarIcon, Filter, MapPin, User, Clock } from 'lucide-vue-next'

const currentWeek = ref('Week 12: Oct 16 - Oct 22')

const timeSlots = [
  '08:00 AM', '09:00 AM', '10:00 AM', '11:00 AM', 
  '12:00 PM', '01:00 PM', '02:00 PM', '03:00 PM'
]

const days = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday']

// Schedule Data representing a complex CSS Grid
const schedules = ref([
  // Monday
  { id: 1, class: 'Mathematics 10A', teacher: 'Dr. Robert Chen', room: 'A201', type: 'Math', day: 'Monday', start: '08:00 AM', duration: 2, color: 'bg-blue-50 dark:bg-blue-500/10 border-blue-200 dark:border-blue-500/30 text-blue-700 dark:text-blue-400' },
  { id: 2, class: 'Physics 11B', teacher: 'Prof. S. Mitchell', room: 'C301', type: 'Science', day: 'Monday', start: '10:00 AM', duration: 1, color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
  // Tuesday
  { id: 3, class: 'English Lit 12A', teacher: 'Ms. A. Foster', room: 'B105', type: 'Humanities', day: 'Tuesday', start: '09:00 AM', duration: 2, color: 'bg-purple-50 dark:bg-purple-500/10 border-purple-200 dark:border-purple-500/30 text-purple-700 dark:text-purple-400' },
  { id: 4, class: 'Biology 10B', teacher: 'Dr. M. Santos', room: 'C302', type: 'Science', day: 'Tuesday', start: '01:00 PM', duration: 2, color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
  // Wednesday
  { id: 5, class: 'Chemistry 11A', teacher: 'Mr. David Park', room: 'Lab 1', type: 'Science', day: 'Wednesday', start: '08:00 AM', duration: 3, color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
  { id: 6, class: 'History 9A', teacher: 'Mr. J. O\'Brien', room: 'B201', type: 'Humanities', day: 'Wednesday', start: '02:00 PM', duration: 1, color: 'bg-orange-50 dark:bg-orange-500/10 border-orange-200 dark:border-orange-500/30 text-orange-700 dark:text-orange-400' },
  // Thursday
  { id: 7, class: 'Mathematics 12B', teacher: 'Dr. Robert Chen', room: 'A201', type: 'Math', day: 'Thursday', start: '10:00 AM', duration: 2, color: 'bg-blue-50 dark:bg-blue-500/10 border-blue-200 dark:border-blue-500/30 text-blue-700 dark:text-blue-400' },
  // Friday
  { id: 8, class: 'Computer Sci 10A', teacher: 'Ms. Linda Kim', room: 'Lab 3', type: 'Tech', day: 'Friday', start: '09:00 AM', duration: 2, color: 'bg-indigo-50 dark:bg-indigo-500/10 border-indigo-200 dark:border-indigo-500/30 text-indigo-700 dark:text-indigo-400' },
  { id: 9, class: 'Economics 11B', teacher: 'Prof. W. Hayes', room: 'B108', type: 'Social', day: 'Friday', start: '01:00 PM', duration: 2, color: 'bg-gray-100 dark:bg-gray-500/20 border-gray-300 dark:border-gray-500/30 text-gray-700 dark:text-gray-300' }
])

const getGridPosition = (day, start, duration) => {
  const dayIndex = days.indexOf(day) + 2 // col 1 is time labels
  const startIndex = timeSlots.indexOf(start) + 2 // row 1 is day labels
  return {
    gridColumn: dayIndex,
    gridRow: `${startIndex} / span ${duration}`
  }
}
</script>

<template>
  <div class="space-y-6 flex flex-col h-[calc(100vh-120px)]">
    <!-- PAGE HEADER -->
    <div class="flex justify-between items-end shrink-0">
      <div>
        <h2 class="text-2xl font-bold text-[#2B3674] dark:text-white mb-1">Schedule Master</h2>
        <p class="text-sm text-gray-400 dark:text-gray-400">Manage time slots, room allocations, and class timings.</p>
      </div>
      
      <!-- Toolbar -->
      <div class="flex items-center gap-3">
        <div class="flex bg-white dark:bg-[#111C44] rounded-lg shadow-sm border border-gray-200 dark:border-white/10 p-1">
          <button class="px-3 py-1 text-sm font-bold rounded-md bg-[#F4F7FE] dark:bg-white/10 text-[#1E88E5] dark:text-blue-400">Week</button>
          <button class="px-3 py-1 text-sm font-bold rounded-md text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200">Month</button>
        </div>

        <button class="flex items-center gap-2 px-4 py-2 bg-white dark:bg-[#111C44] border border-gray-200 dark:border-white/10 rounded-lg text-sm font-bold text-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-white/5 shadow-sm transition-colors">
          <Filter :size="16" />
          Filter
        </button>
      </div>
    </div>

    <!-- CALENDAR CARD -->
    <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 flex flex-col flex-1 overflow-hidden min-h-0">
      
      <!-- Calendar Header -->
      <div class="p-4 border-b border-gray-100 dark:border-white/5 flex items-center justify-between bg-gray-50/50 dark:bg-white/5 shrink-0">
        <div class="flex items-center gap-4">
          <button class="w-8 h-8 flex items-center justify-center rounded-lg border border-gray-200 dark:border-white/10 text-gray-500 dark:text-gray-400 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors bg-white dark:bg-[#111C44]">
            <ChevronLeft :size="18" />
          </button>
          <h3 class="font-extrabold text-[#2B3674] dark:text-white text-lg flex items-center gap-2">
            <CalendarIcon :size="20" class="text-blue-500 dark:text-blue-400" />
            {{ currentWeek }}
          </h3>
          <button class="w-8 h-8 flex items-center justify-center rounded-lg border border-gray-200 dark:border-white/10 text-gray-500 dark:text-gray-400 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors bg-white dark:bg-[#111C44]">
            <ChevronRight :size="18" />
          </button>
        </div>
        
        <!-- Legend Indicator -->
        <div class="hidden lg:flex items-center gap-4 text-xs font-bold text-gray-500 dark:text-gray-400">
          <div class="flex items-center gap-1.5"><div class="w-3 h-3 rounded-full bg-blue-400"></div> Math</div>
          <div class="flex items-center gap-1.5"><div class="w-3 h-3 rounded-full bg-teal-400"></div> Science</div>
          <div class="flex items-center gap-1.5"><div class="w-3 h-3 rounded-full bg-purple-400"></div> Humanities</div>
          <div class="flex items-center gap-1.5"><div class="w-3 h-3 rounded-full bg-indigo-400"></div> Tech</div>
        </div>
      </div>

      <!-- CSS GRID CALENDAR -->
      <div class="flex-1 overflow-auto bg-gray-50/20 dark:bg-transparent relative p-4">
        <div class="min-w-[800px] h-full grid grid-cols-[80px_repeat(5,1fr)] grid-rows-[40px_repeat(8,120px)] gap-2">
          
          <!-- Top Left Empty -->
          <div class="col-start-1 row-start-1"></div>

          <!-- Day Headers -->
          <div v-for="(day, index) in days" :key="day" :style="{ gridColumnStart: index + 2, gridRowStart: 1 }" class="text-center flex flex-col justify-center">
            <span class="text-xs font-bold text-gray-400 dark:text-gray-400 uppercase tracking-wider">{{ day }}</span>
          </div>

          <!-- Time Labels (Y-Axis) -->
          <div v-for="(time, index) in timeSlots" :key="time" :style="{ gridColumnStart: 1, gridRowStart: index + 2 }" class="text-right pr-4 border-r border-gray-200 dark:border-white/10 pt-2">
            <span class="text-[11px] font-bold text-gray-400 dark:text-gray-500 block -mt-3 bg-white dark:bg-[#111C44]">{{ time }}</span>
          </div>

          <!-- Grid Lines -->
          <template v-for="r in 8" :key="'row'+r">
            <template v-for="c in 5" :key="'col'+c">
              <div 
                :style="{ gridColumnStart: c + 1, gridRowStart: r + 1 }"
                class="border-b border-r border-gray-100/80 dark:border-white/5 border-dashed"
                :class="{ 'border-t': r === 1, 'border-l': c === 1 }"
              ></div>
            </template>
          </template>

          <!-- Standard Lunch Break Overlay (Fixed at 12:00 PM) -->
          <div class="col-start-2 col-span-5 row-start-6 z-0 flex items-center justify-center pointer-events-none opacity-50 dark:opacity-30">
            <div class="w-full border-t border-gray-300 dark:border-gray-500 border-dashed relative">
              <span class="absolute top-1/2 left-1/2 -translate-y-1/2 -translate-x-1/2 bg-white dark:bg-[#111C44] px-4 text-xs font-bold text-gray-400 dark:text-gray-500 uppercase tracking-widest rounded-full border border-gray-200 dark:border-white/10 shadow-sm">LUNCH BREAK</span>
            </div>
          </div>

          <!-- Schedule Blocks -->
          <div 
            v-for="item in schedules" 
            :key="item.id"
            :style="getGridPosition(item.day, item.start, item.duration)"
            :class="['m-1 rounded-xl border p-3 flex flex-col shadow-sm hover:shadow-md transition-shadow cursor-pointer z-10 overflow-hidden group', item.color]"
          >
            <div class="flex justify-between items-start mb-1">
              <h4 class="font-bold text-sm leading-tight">{{ item.class }}</h4>
              <button class="opacity-0 group-hover:opacity-100 transition-opacity text-current hover:bg-black/5 dark:hover:bg-white/10 rounded p-0.5">
                <Filter :size="12" />
              </button>
            </div>
            
            <p class="text-[11px] font-bold opacity-80 flex items-center gap-1 mb-2 mt-0.5">
              <Clock :size="10" />
              {{ item.start }} - {{ parseInt(item.start.split(':')[0]) + item.duration }}:00 {{ item.start.includes('AM') && (parseInt(item.start.split(':')[0]) + item.duration) < 12 ? 'AM' : 'PM' }}
            </p>
            
            <div class="mt-auto space-y-1">
              <p class="text-[11px] font-bold flex items-center gap-1.5 opacity-90 truncate">
                <User :size="12" />
                {{ item.teacher }}
              </p>
              <p class="text-[11px] font-bold flex items-center gap-1.5 opacity-90">
                <MapPin :size="12" />
                Room {{ item.room }}
              </p>
            </div>
          </div>

        </div>
      </div>
    </div>
  </div>
</template>
