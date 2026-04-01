<script setup>
import { ref, computed } from 'vue'
import { ChevronLeft, ChevronRight, Calendar as CalendarIcon, Filter, MapPin, User, Clock, AlertCircle } from 'lucide-vue-next'

const timeSlots = [
  '08:00 AM', '09:00 AM', '10:00 AM', '11:00 AM', 
  '12:00 PM', '01:00 PM', '02:00 PM', '03:00 PM'
]

const days = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday']

// View mode: 'week' or 'month'
const viewMode = ref('week')

// Semester data - 3 years
const semesters = ref([
  {
    id: '1-2024-2025',
    name: 'Học Kỳ 1 (2024-2025)',
    year: '2024-2025',
    term: 1,
    startDate: new Date(2024, 8, 1),
    endDate: new Date(2024, 11, 20),
    holidays: [
      { date: new Date(2024, 8, 2), name: 'Khai Giảng' },
      { date: new Date(2024, 9, 13), name: 'Ngày Thành Phố' },
    ]
  },
  {
    id: '2-2024-2025',
    name: 'Học Kỳ 2 (2024-2025)',
    year: '2024-2025',
    term: 2,
    startDate: new Date(2025, 0, 6),
    endDate: new Date(2025, 4, 16),
    holidays: [
      { date: new Date(2025, 1, 10), name: 'Tết Nguyên Đán' },
      { date: new Date(2025, 3, 18), name: 'Giỗ Tổ Hùng Vương' },
    ]
  },
  {
    id: '1-2025-2026',
    name: 'Học Kỳ 1 (2025-2026)',
    year: '2025-2026',
    term: 1,
    startDate: new Date(2025, 8, 1),
    endDate: new Date(2025, 11, 20),
    holidays: [
      { date: new Date(2025, 8, 2), name: 'Khai Giảng' },
      { date: new Date(2025, 9, 13), name: 'Ngày Thành Phố' },
      { date: new Date(2025, 10, 20), name: 'Lễ Pháp' },
      { date: new Date(2025, 10, 31), name: 'Ngày Giáo Viên' },
      { date: new Date(2025, 11, 25), name: 'Lễ Sinh Nhật Bác Hồ' },
    ]
  },
  {
    id: '2-2025-2026',
    name: 'Học Kỳ 2 (2025-2026)',
    year: '2025-2026',
    term: 2,
    startDate: new Date(2026, 0, 5),
    endDate: new Date(2026, 4, 15),
    holidays: [
      { date: new Date(2026, 1, 9), name: 'Tết Nguyên Đán' },
      { date: new Date(2026, 3, 18), name: 'Giỗ Tổ Hùng Vương' },
      { date: new Date(2026, 4, 1), name: 'Ngày Quốc Tế Lao Động' },
    ]
  },
  {
    id: '1-2026-2027',
    name: 'Học Kỳ 1 (2026-2027)',
    year: '2026-2027',
    term: 1,
    startDate: new Date(2026, 8, 1),
    endDate: new Date(2026, 11, 20),
    holidays: [
      { date: new Date(2026, 8, 2), name: 'Khai Giảng' },
      { date: new Date(2026, 9, 13), name: 'Ngày Thành Phố' },
    ]
  }
])

// Current selected semester (default is HK2 2025-2026)
const selectedSemesterId = ref('2-2025-2026')

const currentSemester = computed(() => {
  return semesters.value.find(s => s.id === selectedSemesterId.value)
})

// Get holidays for current semester
const holidays = computed(() => {
  return currentSemester.value?.holidays || []
})

// Get current week display text
const currentWeek = computed(() => {
  return currentWeekInfo.value.dateRange
})

// Current date for determining current week/month
const currentDate = ref(new Date(2026, 3, 1)) // April 1, 2026

// Calculate week info
const getWeekInfo = (date) => {
  const d = new Date(date)
  const day = d.getDay()
  const diff = d.getDate() - day + (day === 0 ? -6 : 1) // Adjust when day is Sunday
  const monday = new Date(d.setDate(diff))
  const sunday = new Date(monday)
  sunday.setDate(sunday.getDate() + 4) // Friday
  
  const weekNum = Math.ceil((d.getDate() - d.getDay() + 1) / 7)
  
  return {
    monday,
    sunday,
    weekNum,
    dateRange: `${monday.toLocaleDateString('vi-VN')} - ${sunday.toLocaleDateString('vi-VN')}`
  }
}

const currentWeekInfo = computed(() => {
  return getWeekInfo(currentDate.value)
})

// Navigate weeks
const previousWeek = () => {
  const d = new Date(currentDate.value)
  d.setDate(d.getDate() - 7)
  currentDate.value = d
}

const nextWeek = () => {
  const d = new Date(currentDate.value)
  d.setDate(d.getDate() + 7)
  currentDate.value = d
}

const goToToday = () => {
  currentDate.value = new Date()
}

// Get dates for week view
const weekDates = computed(() => {
  const dates = []
  const { monday } = currentWeekInfo.value
  for (let i = 0; i < 5; i++) {
    const d = new Date(monday)
    d.setDate(d.getDate() + i)
    dates.push(d)
  }
  return dates
})

// Format day header with date
const formatDayHeader = (index) => {
  const date = weekDates.value[index]
  return {
    day: days[index],
    fullDate: date.toLocaleDateString('vi-VN', { weekday: 'short', year: 'numeric', month: '2-digit', day: '2-digit' })
  }
}

// Calendar data for month view
const currentMonth = computed(() => currentDate.value.getMonth())
const currentYear = computed(() => currentDate.value.getFullYear())

const monthDays = computed(() => {
  const firstDay = new Date(currentYear.value, currentMonth.value, 1)
  const lastDay = new Date(currentYear.value, currentMonth.value + 1, 0)
  const daysInMonth = lastDay.getDate()
  let startingDayOfWeek = firstDay.getDay() // 0 = Sunday, 1 = Monday, ..., 6 = Saturday
  
  // Adjust so Monday = 0, Tuesday = 1, ..., Sunday = 6
  startingDayOfWeek = (startingDayOfWeek === 0) ? 6 : startingDayOfWeek - 1
  
  const days = []
  // Add empty cells for days before the month starts
  for (let i = 0; i < startingDayOfWeek; i++) {
    days.push(null)
  }
  // Add all days of the month
  for (let i = 1; i <= daysInMonth; i++) {
    days.push(new Date(currentYear.value, currentMonth.value, i))
  }
  return days
})

const previousMonth = () => {
  const d = new Date(currentDate.value)
  d.setMonth(d.getMonth() - 1)
  currentDate.value = d
}

const nextMonth = () => {
  const d = new Date(currentDate.value)
  d.setMonth(d.getMonth() + 1)
  currentDate.value = d
}

// Check if there's a class on a specific day
const getDayName = (date) => {
  const dayOfWeek = date.getDay() // 0 = Sunday, 1 = Monday, ..., 6 = Saturday
  const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']
  return dayNames[dayOfWeek]
}

const hasClassOnDay = (date) => {
  if (!selectedClass.value) return false
  const dayName = getDayName(date)
  return schedules.value.some(s => s.day === dayName)
}

// Get classes for a specific day
const getClassesForDay = (date) => {
  if (!selectedClass.value) return []
  const dayName = getDayName(date)
  return schedules.value.filter(s => s.day === dayName)
}

// Mock teachers info
const mockTeachers = {
  GV001: { name: 'Nguyễn Văn A', speciality: 'Toán học' },
  GV002: { name: 'Trần Thị B', speciality: 'Vật Lý' },
  GV003: { name: 'Lê Văn C', speciality: 'Hóa học' },
  GV004: { name: 'Phạm Thị D', speciality: 'Tiếng Anh' },
  GV005: { name: 'Đỗ Văn E', speciality: 'Ngữ Văn' },
}

// Mock schedule data
const mockSchedules = {
  '10A': [
    { MonHoc: 'Toán', day: 'Monday', start: '08:00 AM', duration: 2, teacherId: 'GV001', room: 'A201', color: 'bg-blue-50 dark:bg-blue-500/10 border-blue-200 dark:border-blue-500/30 text-blue-700 dark:text-blue-400' },
    { MonHoc: 'Vật Lý', day: 'Monday', start: '10:00 AM', duration: 2, teacherId: 'GV002', room: 'B102', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Hóa Học', day: 'Tuesday', start: '08:00 AM', duration: 2, teacherId: 'GV003', room: 'C301', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Tiếng Anh', day: 'Tuesday', start: '10:00 AM', duration: 2, teacherId: 'GV004', room: 'A202', color: 'bg-purple-50 dark:bg-purple-500/10 border-purple-200 dark:border-purple-500/30 text-purple-700 dark:text-purple-400' },
    { MonHoc: 'Ngữ Văn', day: 'Wednesday', start: '08:00 AM', duration: 2, teacherId: 'GV005', room: 'B101', color: 'bg-orange-50 dark:bg-orange-500/10 border-orange-200 dark:border-orange-500/30 text-orange-700 dark:text-orange-400' },
    { MonHoc: 'Toán', day: 'Thursday', start: '09:00 AM', duration: 2, teacherId: 'GV001', room: 'A201', color: 'bg-blue-50 dark:bg-blue-500/10 border-blue-200 dark:border-blue-500/30 text-blue-700 dark:text-blue-400' },
    { MonHoc: 'Vật Lý', day: 'Thursday', start: '02:00 PM', duration: 1, teacherId: 'GV002', room: 'B102', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Hóa Học', day: 'Friday', start: '09:00 AM', duration: 2, teacherId: 'GV003', room: 'C301', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Tiếng Anh', day: 'Friday', start: '02:00 PM', duration: 1, teacherId: 'GV004', room: 'A202', color: 'bg-purple-50 dark:bg-purple-500/10 border-purple-200 dark:border-purple-500/30 text-purple-700 dark:text-purple-400' },
  ],
  '10B': [
    { MonHoc: 'Vật Lý', day: 'Monday', start: '08:00 AM', duration: 2, teacherId: 'GV002', room: 'B103', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Toán', day: 'Monday', start: '10:00 AM', duration: 2, teacherId: 'GV001', room: 'A201', color: 'bg-blue-50 dark:bg-blue-500/10 border-blue-200 dark:border-blue-500/30 text-blue-700 dark:text-blue-400' },
    { MonHoc: 'Hóa Học', day: 'Tuesday', start: '08:00 AM', duration: 2, teacherId: 'GV003', room: 'C302', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Tiếng Anh', day: 'Tuesday', start: '10:00 AM', duration: 2, teacherId: 'GV004', room: 'B101', color: 'bg-purple-50 dark:bg-purple-500/10 border-purple-200 dark:border-purple-500/30 text-purple-700 dark:text-purple-400' },
    { MonHoc: 'Ngữ Văn', day: 'Wednesday', start: '09:00 AM', duration: 2, teacherId: 'GV005', room: 'B104', color: 'bg-orange-50 dark:bg-orange-500/10 border-orange-200 dark:border-orange-500/30 text-orange-700 dark:text-orange-400' },
    { MonHoc: 'Vật Lý', day: 'Thursday', start: '08:00 AM', duration: 2, teacherId: 'GV002', room: 'B103', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Toán', day: 'Thursday', start: '02:00 PM', duration: 1, teacherId: 'GV001', room: 'A201', color: 'bg-blue-50 dark:bg-blue-500/10 border-blue-200 dark:border-blue-500/30 text-blue-700 dark:text-blue-400' },
    { MonHoc: 'Hóa Học', day: 'Friday', start: '10:00 AM', duration: 2, teacherId: 'GV003', room: 'C302', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Tiếng Anh', day: 'Friday', start: '02:00 PM', duration: 1, teacherId: 'GV004', room: 'B101', color: 'bg-purple-50 dark:bg-purple-500/10 border-purple-200 dark:border-purple-500/30 text-purple-700 dark:text-purple-400' },
  ],
  '10C': [
    { MonHoc: 'Hóa Học', day: 'Monday', start: '08:00 AM', duration: 2, teacherId: 'GV003', room: 'Lab 1', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Toán', day: 'Monday', start: '10:00 AM', duration: 2, teacherId: 'GV001', room: 'A202', color: 'bg-blue-50 dark:bg-blue-500/10 border-blue-200 dark:border-blue-500/30 text-blue-700 dark:text-blue-400' },
    { MonHoc: 'Vật Lý', day: 'Tuesday', start: '09:00 AM', duration: 2, teacherId: 'GV002', room: 'B102', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Tiếng Anh', day: 'Wednesday', start: '08:00 AM', duration: 2, teacherId: 'GV004', room: 'A201', color: 'bg-purple-50 dark:bg-purple-500/10 border-purple-200 dark:border-purple-500/30 text-purple-700 dark:text-purple-400' },
    { MonHoc: 'Ngữ Văn', day: 'Wednesday', start: '10:00 AM', duration: 2, teacherId: 'GV005', room: 'B103', color: 'bg-orange-50 dark:bg-orange-500/10 border-orange-200 dark:border-orange-500/30 text-orange-700 dark:text-orange-400' },
    { MonHoc: 'Hóa Học', day: 'Thursday', start: '09:00 AM', duration: 2, teacherId: 'GV003', room: 'Lab 1', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Toán', day: 'Friday', start: '08:00 AM', duration: 2, teacherId: 'GV001', room: 'A202', color: 'bg-blue-50 dark:bg-blue-500/10 border-blue-200 dark:border-blue-500/30 text-blue-700 dark:text-blue-400' },
    { MonHoc: 'Vật Lý', day: 'Friday', start: '02:00 PM', duration: 1, teacherId: 'GV002', room: 'B102', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Tiếng Anh', day: 'Friday', start: '03:00 PM', duration: 1, teacherId: 'GV004', room: 'A201', color: 'bg-purple-50 dark:bg-purple-500/10 border-purple-200 dark:border-purple-500/30 text-purple-700 dark:text-purple-400' },
  ],
  '11A': [
    { MonHoc: 'Tiếng Anh', day: 'Monday', start: '08:00 AM', duration: 2, teacherId: 'GV004', room: 'A201', color: 'bg-purple-50 dark:bg-purple-500/10 border-purple-200 dark:border-purple-500/30 text-purple-700 dark:text-purple-400' },
    { MonHoc: 'Toán', day: 'Monday', start: '10:00 AM', duration: 2, teacherId: 'GV001', room: 'B101', color: 'bg-blue-50 dark:bg-blue-500/10 border-blue-200 dark:border-blue-500/30 text-blue-700 dark:text-blue-400' },
    { MonHoc: 'Vật Lý', day: 'Tuesday', start: '08:00 AM', duration: 2, teacherId: 'GV002', room: 'C301', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Hóa Học', day: 'Tuesday', start: '10:00 AM', duration: 2, teacherId: 'GV003', room: 'C302', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Ngữ Văn', day: 'Wednesday', start: '09:00 AM', duration: 2, teacherId: 'GV005', room: 'B104', color: 'bg-orange-50 dark:bg-orange-500/10 border-orange-200 dark:border-orange-500/30 text-orange-700 dark:text-orange-400' },
    { MonHoc: 'Tiếng Anh', day: 'Thursday', start: '08:00 AM', duration: 2, teacherId: 'GV004', room: 'A201', color: 'bg-purple-50 dark:bg-purple-500/10 border-purple-200 dark:border-purple-500/30 text-purple-700 dark:text-purple-400' },
    { MonHoc: 'Toán', day: 'Thursday', start: '02:00 PM', duration: 1, teacherId: 'GV001', room: 'B101', color: 'bg-blue-50 dark:bg-blue-500/10 border-blue-200 dark:border-blue-500/30 text-blue-700 dark:text-blue-400' },
    { MonHoc: 'Vật Lý', day: 'Friday', start: '10:00 AM', duration: 2, teacherId: 'GV002', room: 'C301', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Hóa Học', day: 'Friday', start: '02:00 PM', duration: 1, teacherId: 'GV003', room: 'C302', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
  ],
  '11B': [
    { MonHoc: 'Ngữ Văn', day: 'Monday', start: '08:00 AM', duration: 2, teacherId: 'GV005', room: 'B104', color: 'bg-orange-50 dark:bg-orange-500/10 border-orange-200 dark:border-orange-500/30 text-orange-700 dark:text-orange-400' },
    { MonHoc: 'Toán', day: 'Monday', start: '10:00 AM', duration: 2, teacherId: 'GV001', room: 'A202', color: 'bg-blue-50 dark:bg-blue-500/10 border-blue-200 dark:border-blue-500/30 text-blue-700 dark:text-blue-400' },
    { MonHoc: 'Vật Lý', day: 'Tuesday', start: '09:00 AM', duration: 2, teacherId: 'GV002', room: 'B102', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Hóa Học', day: 'Wednesday', start: '08:00 AM', duration: 2, teacherId: 'GV003', room: 'Lab 2', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Tiếng Anh', day: 'Wednesday', start: '10:00 AM', duration: 2, teacherId: 'GV004', room: 'A201', color: 'bg-purple-50 dark:bg-purple-500/10 border-purple-200 dark:border-purple-500/30 text-purple-700 dark:text-purple-400' },
    { MonHoc: 'Ngữ Văn', day: 'Thursday', start: '09:00 AM', duration: 2, teacherId: 'GV005', room: 'B104', color: 'bg-orange-50 dark:bg-orange-500/10 border-orange-200 dark:border-orange-500/30 text-orange-700 dark:text-orange-400' },
    { MonHoc: 'Toán', day: 'Friday', start: '08:00 AM', duration: 2, teacherId: 'GV001', room: 'A202', color: 'bg-blue-50 dark:bg-blue-500/10 border-blue-200 dark:border-blue-500/30 text-blue-700 dark:text-blue-400' },
    { MonHoc: 'Vật Lý', day: 'Friday', start: '02:00 PM', duration: 1, teacherId: 'GV002', room: 'B102', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Hóa Học', day: 'Friday', start: '03:00 PM', duration: 1, teacherId: 'GV003', room: 'Lab 2', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
  ],
  '12A': [
    { MonHoc: 'Toán', day: 'Monday', start: '08:00 AM', duration: 2, teacherId: 'GV001', room: 'A201', color: 'bg-blue-50 dark:bg-blue-500/10 border-blue-200 dark:border-blue-500/30 text-blue-700 dark:text-blue-400' },
    { MonHoc: 'Vật Lý', day: 'Monday', start: '10:00 AM', duration: 2, teacherId: 'GV002', room: 'B103', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Hóa Học', day: 'Tuesday', start: '08:00 AM', duration: 2, teacherId: 'GV003', room: 'C301', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Tiếng Anh', day: 'Tuesday', start: '10:00 AM', duration: 2, teacherId: 'GV004', room: 'B101', color: 'bg-purple-50 dark:bg-purple-500/10 border-purple-200 dark:border-purple-500/30 text-purple-700 dark:text-purple-400' },
    { MonHoc: 'Ngữ Văn', day: 'Wednesday', start: '09:00 AM', duration: 2, teacherId: 'GV005', room: 'B104', color: 'bg-orange-50 dark:bg-orange-500/10 border-orange-200 dark:border-orange-500/30 text-orange-700 dark:text-orange-400' },
    { MonHoc: 'Toán', day: 'Thursday', start: '10:00 AM', duration: 2, teacherId: 'GV001', room: 'A201', color: 'bg-blue-50 dark:bg-blue-500/10 border-blue-200 dark:border-blue-500/30 text-blue-700 dark:text-blue-400' },
    { MonHoc: 'Vật Lý', day: 'Thursday', start: '02:00 PM', duration: 1, teacherId: 'GV002', room: 'B103', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Hóa Học', day: 'Friday', start: '10:00 AM', duration: 2, teacherId: 'GV003', room: 'C301', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Tiếng Anh', day: 'Friday', start: '02:00 PM', duration: 1, teacherId: 'GV004', room: 'B101', color: 'bg-purple-50 dark:bg-purple-500/10 border-purple-200 dark:border-purple-500/30 text-purple-700 dark:text-purple-400' },
  ],
  '12B': [
    { MonHoc: 'Toán', day: 'Monday', start: '08:00 AM', duration: 2, teacherId: 'GV001', room: 'A202', color: 'bg-blue-50 dark:bg-blue-500/10 border-blue-200 dark:border-blue-500/30 text-blue-700 dark:text-blue-400' },
    { MonHoc: 'Vật Lý', day: 'Monday', start: '10:00 AM', duration: 2, teacherId: 'GV002', room: 'C301', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Hóa Học', day: 'Tuesday', start: '09:00 AM', duration: 2, teacherId: 'GV003', room: 'C302', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Tiếng Anh', day: 'Wednesday', start: '08:00 AM', duration: 2, teacherId: 'GV004', room: 'A201', color: 'bg-purple-50 dark:bg-purple-500/10 border-purple-200 dark:border-purple-500/30 text-purple-700 dark:text-purple-400' },
    { MonHoc: 'Ngữ Văn', day: 'Wednesday', start: '10:00 AM', duration: 2, teacherId: 'GV005', room: 'B103', color: 'bg-orange-50 dark:bg-orange-500/10 border-orange-200 dark:border-orange-500/30 text-orange-700 dark:text-orange-400' },
    { MonHoc: 'Toán', day: 'Thursday', start: '09:00 AM', duration: 2, teacherId: 'GV001', room: 'A202', color: 'bg-blue-50 dark:bg-blue-500/10 border-blue-200 dark:border-blue-500/30 text-blue-700 dark:text-blue-400' },
    { MonHoc: 'Vật Lý', day: 'Friday', start: '08:00 AM', duration: 2, teacherId: 'GV002', room: 'C301', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Hóa Học', day: 'Friday', start: '02:00 PM', duration: 1, teacherId: 'GV003', room: 'C302', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Tiếng Anh', day: 'Friday', start: '03:00 PM', duration: 1, teacherId: 'GV004', room: 'A201', color: 'bg-purple-50 dark:bg-purple-500/10 border-purple-200 dark:border-purple-500/30 text-purple-700 dark:text-purple-400' },
  ],
  '9A': [
    { MonHoc: 'Toán', day: 'Monday', start: '08:00 AM', duration: 2, teacherId: 'GV001', room: 'B101', color: 'bg-blue-50 dark:bg-blue-500/10 border-blue-200 dark:border-blue-500/30 text-blue-700 dark:text-blue-400' },
    { MonHoc: 'Vật Lý', day: 'Monday', start: '10:00 AM', duration: 2, teacherId: 'GV002', room: 'B102', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Hóa Học', day: 'Tuesday', start: '08:00 AM', duration: 2, teacherId: 'GV003', room: 'Lab 1', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Tiếng Anh', day: 'Tuesday', start: '10:00 AM', duration: 2, teacherId: 'GV004', room: 'A202', color: 'bg-purple-50 dark:bg-purple-500/10 border-purple-200 dark:border-purple-500/30 text-purple-700 dark:text-purple-400' },
    { MonHoc: 'Ngữ Văn', day: 'Wednesday', start: '08:00 AM', duration: 2, teacherId: 'GV005', room: 'B104', color: 'bg-orange-50 dark:bg-orange-500/10 border-orange-200 dark:border-orange-500/30 text-orange-700 dark:text-orange-400' },
    { MonHoc: 'Toán', day: 'Thursday', start: '09:00 AM', duration: 2, teacherId: 'GV001', room: 'B101', color: 'bg-blue-50 dark:bg-blue-500/10 border-blue-200 dark:border-blue-500/30 text-blue-700 dark:text-blue-400' },
    { MonHoc: 'Vật Lý', day: 'Thursday', start: '02:00 PM', duration: 1, teacherId: 'GV002', room: 'B102', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Hóa Học', day: 'Friday', start: '10:00 AM', duration: 2, teacherId: 'GV003', room: 'Lab 1', color: 'bg-teal-50 dark:bg-teal-500/10 border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-400' },
    { MonHoc: 'Tiếng Anh', day: 'Friday', start: '02:00 PM', duration: 1, teacherId: 'GV004', room: 'A202', color: 'bg-purple-50 dark:bg-purple-500/10 border-purple-200 dark:border-purple-500/30 text-purple-700 dark:text-purple-400' },
  ]
}


const classes = ['10A', '10B', '10C', '11A', '11B', '12A', '12B', '9A']
const selectedClass = ref('10A')

// Get current schedules based on selected class
const schedules = ref([])

const updateSchedules = () => {
  schedules.value = mockSchedules[selectedClass.value] || []
}

// Initialize schedules
updateSchedules()

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
        <!-- Class Selection Dropdown -->
        <select 
          v-model="selectedClass" 
          @change="updateSchedules"
          class="px-4 py-2 bg-white dark:bg-[#111C44] border border-gray-200 dark:border-white/10 rounded-lg text-sm font-bold text-gray-700 dark:text-gray-300 hover:border-blue-500 dark:hover:border-blue-500 transition-colors shadow-sm"
        >
          <option v-for="cls in classes" :key="cls" :value="cls">
            {{ cls }}
          </option>
        </select>

        <div class="flex bg-white dark:bg-[#111C44] rounded-lg shadow-sm border border-gray-200 dark:border-white/10 p-1">
          <button 
            @click="viewMode = 'week'"
            :class="viewMode === 'week' ? 'bg-[#F4F7FE] dark:bg-white/10 text-[#1E88E5] dark:text-blue-400' : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200'"
            class="px-3 py-1 text-sm font-bold rounded-md transition-colors"
          >Week</button>
          <button 
            @click="viewMode = 'month'"
            :class="viewMode === 'month' ? 'bg-[#F4F7FE] dark:bg-white/10 text-[#1E88E5] dark:text-blue-400' : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200'"
            class="px-3 py-1 text-sm font-bold rounded-md transition-colors"
          >Month</button>
        </div>

        <button 
          @click="goToToday"
          class="flex items-center gap-2 px-4 py-2 bg-blue-50 dark:bg-blue-500/20 border border-blue-200 dark:border-blue-500/50 rounded-lg text-sm font-bold text-blue-600 dark:text-blue-400 hover:bg-blue-100 dark:hover:bg-blue-500/30 shadow-sm transition-colors"
        >
          Hôm Nay
        </button>

        <button class="flex items-center gap-2 px-4 py-2 bg-white dark:bg-[#111C44] border border-gray-200 dark:border-white/10 rounded-lg text-sm font-bold text-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-white/5 shadow-sm transition-colors">
          <Filter :size="16" />
          Filter
        </button>
      </div>
    </div>

    <!-- SEMESTER INFO CARD -->
    <div class="bg-gradient-to-r from-blue-50 to-indigo-50 dark:from-blue-500/10 dark:to-indigo-500/10 rounded-xl p-4 border border-blue-200 dark:border-blue-500/20 shadow-sm">
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <!-- Semester Name -->
        <div class="flex items-start gap-3">
          <div class="p-2.5 bg-blue-100 dark:bg-blue-500/20 rounded-lg">
            <CalendarIcon :size="20" class="text-blue-600 dark:text-blue-400" />
          </div>
          <div>
            <p class="text-xs font-bold text-blue-600 dark:text-blue-400 uppercase tracking-wide">Kỳ Học</p>
            <p class="text-sm font-bold text-[#2B3674] dark:text-white">{{ currentSemester.name }}</p>
          </div>
        </div>

        <!-- Start Date -->
        <div class="flex items-start gap-3">
          <div class="p-2.5 bg-green-100 dark:bg-green-500/20 rounded-lg">
            <Clock :size="20" class="text-green-600 dark:text-green-400" />
          </div>
          <div>
            <p class="text-xs font-bold text-green-600 dark:text-green-400 uppercase tracking-wide">Bắt Đầu</p>
            <p class="text-sm font-bold text-[#2B3674] dark:text-white">{{ currentSemester.startDate.toLocaleDateString('vi-VN') }}</p>
          </div>
        </div>

        <!-- End Date -->
        <div class="flex items-start gap-3">
          <div class="p-2.5 bg-red-100 dark:bg-red-500/20 rounded-lg">
            <Clock :size="20" class="text-red-600 dark:text-red-400" />
          </div>
          <div>
            <p class="text-xs font-bold text-red-600 dark:text-red-400 uppercase tracking-wide">Kết Thúc</p>
            <p class="text-sm font-bold text-[#2B3674] dark:text-white">{{ currentSemester.endDate.toLocaleDateString('vi-VN') }}</p>
          </div>
        </div>

        <!-- Holidays Count -->
        <div class="flex items-start gap-3">
          <div class="p-2.5 bg-orange-100 dark:bg-orange-500/20 rounded-lg">
            <AlertCircle :size="20" class="text-orange-600 dark:text-orange-400" />
          </div>
          <div>
            <p class="text-xs font-bold text-orange-600 dark:text-orange-400 uppercase tracking-wide">Ngày Nghỉ</p>
            <p class="text-sm font-bold text-[#2B3674] dark:text-white">{{ holidays.length }} ngày</p>
          </div>
        </div>
      </div>

      <!-- Holidays List -->
      <div class="mt-4 pt-4 border-t border-blue-200 dark:border-blue-500/20">
        <p class="text-xs font-bold text-blue-600 dark:text-blue-400 uppercase tracking-wide mb-2">Các Ngày Nghỉ Lễ</p>
        <div class="flex flex-wrap gap-2">
          <div 
            v-for="holiday in holidays" 
            :key="holiday.name"
            class="px-3 py-1.5 bg-white dark:bg-white/5 border border-blue-200 dark:border-blue-500/30 rounded-full text-xs font-semibold text-blue-700 dark:text-blue-300 flex items-center gap-1.5"
          >
            <span class="inline-block w-1.5 h-1.5 bg-red-500 rounded-full"></span>
            {{ holiday.date.toLocaleDateString('vi-VN') }} - {{ holiday.name }}
          </div>
        </div>
      </div>
    </div>

    <!-- CALENDAR CARD -->
    <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 flex flex-col flex-1 overflow-hidden min-h-0">
      
      <!-- Calendar Header -->
      <div class="p-4 border-b border-gray-100 dark:border-white/5 flex items-center justify-between bg-gray-50/50 dark:bg-white/5 shrink-0">
        <div class="flex items-center gap-4">
          <button 
            @click="viewMode === 'week' ? previousWeek() : previousMonth()"
            class="w-8 h-8 flex items-center justify-center rounded-lg border border-gray-200 dark:border-white/10 text-gray-500 dark:text-gray-400 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors bg-white dark:bg-[#111C44]"
          >
            <ChevronLeft :size="18" />
          </button>
          <h3 class="font-extrabold text-[#2B3674] dark:text-white text-lg flex items-center gap-2 min-w-[300px]">
            <CalendarIcon :size="20" class="text-blue-500 dark:text-blue-400" />
            <span v-if="viewMode === 'week'">{{ currentWeek }}</span>
            <span v-else>{{ currentDate.toLocaleDateString('vi-VN', { month: 'long', year: 'numeric' }) }}</span>
          </h3>
          <button 
            @click="viewMode === 'week' ? nextWeek() : nextMonth()"
            class="w-8 h-8 flex items-center justify-center rounded-lg border border-gray-200 dark:border-white/10 text-gray-500 dark:text-gray-400 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors bg-white dark:bg-[#111C44]"
          >
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

      <!-- CSS GRID CALENDAR - WEEK VIEW -->
      <div v-if="viewMode === 'week'" class="flex-1 overflow-auto bg-gray-50/20 dark:bg-transparent relative p-4">
        <div class="min-w-[800px] h-full grid grid-cols-[80px_repeat(5,1fr)] grid-rows-[40px_repeat(8,120px)] gap-2">
          
          <!-- Top Left Empty -->
          <div class="col-start-1 row-start-1"></div>

          <!-- Day Headers -->
          <div v-for="(day, index) in days" :key="day" :style="{ gridColumnStart: index + 2, gridRowStart: 1 }" class="text-center flex flex-col justify-center">
            <span class="text-xs font-bold text-gray-400 dark:text-gray-400 uppercase tracking-wider">{{ day }}</span>
            <span class="text-[10px] text-gray-500 dark:text-gray-500">{{ weekDates[index]?.toLocaleDateString('vi-VN', {day: '2-digit'}) }}</span>
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
            v-for="(item, idx) in schedules" 
            :key="idx"
            :style="getGridPosition(item.day, item.start, item.duration)"
            :class="['m-1 rounded-xl border p-3 flex flex-col shadow-sm hover:shadow-md transition-shadow cursor-pointer z-10 overflow-hidden group', item.color]"
          >
            <div class="flex justify-between items-start mb-1">
              <h4 class="font-bold text-sm leading-tight">{{ item.MonHoc }}</h4>
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
                {{ mockTeachers[item.teacherId]?.name || 'N/A' }}
              </p>
              <p class="text-[11px] font-bold flex items-center gap-1.5 opacity-90">
                <MapPin :size="12" />
                {{ item.room }}
              </p>
            </div>
          </div>

        </div>
      </div>

      <!-- CALENDAR MONTH VIEW -->
      <div v-else class="flex-1 overflow-auto bg-gray-50/20 dark:bg-transparent relative p-4">
        <div class="grid grid-cols-7 gap-2 h-full">
          <!-- Day headers (Mon-Sun) -->
          <div v-for="dayHeader in ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'CN']" :key="dayHeader" class="text-center py-2 font-bold text-sm text-gray-600 dark:text-gray-400 bg-gray-50 dark:bg-white/5 rounded-lg">
            {{ dayHeader }}
          </div>

          <!-- Calendar days -->
          <div 
            v-for="(day, index) in monthDays" 
            :key="index"
            class="min-h-[120px] p-2 rounded-lg border-2 transition-all cursor-pointer"
            :class="[
              day ? 'bg-white dark:bg-[#111C44] border-gray-100 dark:border-white/5 hover:border-blue-300 dark:hover:border-blue-500/50 hover:shadow-md' : 'bg-gray-50 dark:bg-gray-900/50 border-transparent',
              day && hasClassOnDay(day) ? 'ring-2 ring-blue-400 dark:ring-blue-500' : ''
            ]"
          >
            <div v-if="day" class="h-full flex flex-col">
              <div class="flex justify-between items-start mb-1">
                <span class="font-bold text-sm" :class="hasClassOnDay(day) ? 'text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-500/20 px-2 py-0.5 rounded' : 'text-gray-700 dark:text-gray-300'">
                  {{ day.getDate() }}
                </span>
                <span v-if="hasClassOnDay(day)" class="inline-block w-2 h-2 bg-blue-500 rounded-full"></span>
              </div>
              
              <!-- Show first 2 classes on this day -->
              <div class="flex-1 space-y-1 overflow-y-auto text-[10px]">
                <div 
                  v-for="(cls, cidx) in getClassesForDay(day).slice(0, 2)" 
                  :key="cidx"
                  :class="[
                    'px-2 py-1 rounded-md truncate font-semibold text-white',
                    cls.color.includes('blue') ? 'bg-blue-500' : 
                    cls.color.includes('teal') ? 'bg-teal-500' :
                    cls.color.includes('purple') ? 'bg-purple-500' :
                    cls.color.includes('orange') ? 'bg-orange-500' : 'bg-indigo-500'
                  ]"
                  :title="cls.MonHoc"
                >
                  {{ cls.MonHoc }}
                </div>
                <div v-if="getClassesForDay(day).length > 2" class="px-2 py-1 text-gray-500 dark:text-gray-400 font-semibold">
                  +{{ getClassesForDay(day).length - 2 }} more
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
