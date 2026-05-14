<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { Calculator, Target, ArrowRight, UserSquare, BookOpen, AlertTriangle, CheckCircle, TrendingUp, Hash, Info, RefreshCw } from 'lucide-vue-next'
import { ElMessage } from 'element-plus'
import { apiService } from '../services/api'

// Initial data
const classes = ref([])
const students = ref([])
const subjects = ref([])
const terms = [1, 2]

const form = ref({
  classId: '',
  studentId: '',
  subjectId: '',
  term: 1,
  targetAverage: 8.0,
  hypotheticalFinal: 7.5
})

const loadingOptions = ref(false)
const calculating = ref(false)

// Results state
const currentGrades = ref({
  oral: null,
  quiz15: null,
  midterm: null,
  final: null
})

const result = ref({
  predictedAverage: 0,
  classification: 'Chưa xếp loại',
  requiredFinalScore: 0
})

const debounceTimeout = ref(null)

/** Năm học của lớp đang chọn — khớp DiemSo.NamHoc trên API */
const selectedNamHoc = computed(() => {
  const c = classes.value.find((x) => x.MaLop === form.value.classId)
  return c?.NamHoc || '2025-2026'
})

// Methods to load dropdowns
const initOptions = async () => {
  loadingOptions.value = true
  try {
    const [classRes, subRes] = await Promise.all([
      apiService.getLopHocs(),
      apiService.getMonHocs()
    ])
    classes.value = classRes.data
    subjects.value = subRes.data

    if (classes.value.length > 0) {
      form.value.classId = classes.value[0].MaLop
      await loadStudentsForClass(form.value.classId)
    }
    if (subjects.value.length > 0) form.value.subjectId = subjects.value[0].MaMon

  } catch (error) {
    console.error("Lỗi tải options:", error)
  } finally {
    loadingOptions.value = false
  }
}

const loadStudentsForClass = async (maLop) => {
  if (!maLop) {
    students.value = []
    form.value.studentId = ''
    return
  }
  try {
    const res = await apiService.getHocSinhs(maLop)
    students.value = res.data
    if (students.value.length > 0) {
      form.value.studentId = students.value[0].MaHS
    } else {
      form.value.studentId = ''
    }
  } catch (err) {
    console.error("Lỗi lấy danh sách học sinh:", err)
  }
}

// Watch class to change students
watch(() => form.value.classId, (newId) => {
  loadStudentsForClass(newId)
})

// Trigger calculation
const calculateWhatIf = async () => {
  if (!form.value.studentId || !form.value.subjectId) return
  const hyp = Number(form.value.hypotheticalFinal)
  const tgt = Number(form.value.targetAverage)
  if (Number.isNaN(hyp) || Number.isNaN(tgt)) {
    ElMessage.warning('Tham số điểm không hợp lệ.')
    return
  }
  calculating.value = true
  try {
    const payload = {
      maHS: form.value.studentId,
      maMon: form.value.subjectId,
      namHoc: selectedNamHoc.value,
      hocKy: form.value.term,
      diemCuoiKyGiaDinh: hyp,
      targetTb: tgt,
    }
    const res = await apiService.postDssWhatIf(payload)
    // API dùng PascalCase (PropertyNamingPolicy = null)
    const d = res.data

    currentGrades.value = {
      oral: d.DiemMieng ?? null,
      quiz15: d.Diem15p ?? null,
      midterm: d.DiemGiuaKy ?? null,
      final: d.DiemCuoiKyHienTai ?? null,
    }

    const ck = d.CkCanThietDeDatTarget
    result.value = {
      predictedAverage: d.TbGiaDinh != null ? Number(d.TbGiaDinh).toFixed(1) : '-',
      classification: d.XepLoaiGiaDinh || 'Chưa XL',
      requiredFinalScore: ck != null && !Number.isNaN(Number(ck)) ? Number(ck) : 0,
    }
  } catch (error) {
    console.error('Lỗi tính toán WhatIf:', error)
    const msg = error?.response?.data?.message || error?.response?.data?.title || 'Không gọi được API What-If (kiểm tra quyền Scores.Edit và dữ liệu điểm).'
    ElMessage.error(typeof msg === 'string' ? msg : 'Lỗi tính toán What-If.')
  } finally {
    calculating.value = false
  }
}

// Watch input changes (debounce for ranges)
watch([() => form.value.targetAverage, () => form.value.hypotheticalFinal], () => {
  if (!form.value.studentId || !form.value.subjectId) return
  if (debounceTimeout.value) clearTimeout(debounceTimeout.value)
  debounceTimeout.value = setTimeout(() => {
    calculateWhatIf()
  }, 300)
})

// Khi đổi HS/môn/kỳ/năm học (theo lớp) — tính lại ngay
watch([() => form.value.studentId, () => form.value.subjectId, () => form.value.term, selectedNamHoc], () => {
  if (form.value.studentId && form.value.subjectId) calculateWhatIf()
})

onMounted(() => {
  initOptions().then(() => {
    if (form.value.studentId && form.value.subjectId) calculateWhatIf()
  })
})

// UI Helpers
const getGradeColor = (score) => {
  if (score === '-') return 'text-gray-400'
  const s = parseFloat(score)
  if (s >= 8.0) return 'text-green-500'
  if (s >= 6.5) return 'text-blue-500'
  if (s >= 5.0) return 'text-orange-500'
  return 'text-red-500'
}

const getRequiredScoreColor = (score) => {
  if (score > 10) return 'text-red-500' // Impossible
  if (score >= 8) return 'text-orange-500' // Hard
  if (score >= 5) return 'text-blue-500' // Normal
  return 'text-green-500' // Easy
}

const classificationColor = computed(() => {
  const label = result.value.classification
  if (label === 'Giỏi') return 'bg-green-100 dark:bg-green-500/20 text-green-700 dark:text-green-400'
  if (label === 'Khá') return 'bg-blue-100 dark:bg-blue-500/20 text-blue-700 dark:text-blue-400'
  if (label === 'TrungBinh' || label === 'Trung Bình') return 'bg-orange-100 dark:bg-orange-500/20 text-orange-700 dark:text-orange-400'
  if (label === 'Yếu' || label === 'Kém') return 'bg-red-100 dark:bg-red-500/20 text-red-700 dark:text-red-400'
  return 'bg-gray-100 dark:bg-gray-700 dark:text-gray-300'
})
</script>

<template>
  <div class="space-y-6">
    <!-- PAGE HEADER -->
    <div class="flex justify-between items-end">
      <div>
        <h2 class="text-2xl font-bold text-[#2B3674] dark:text-white mb-1 flex items-center gap-2">
          <Calculator class="text-[#1E88E5] dark:text-blue-400" :size="24" />
          DSS: Phân tích What-If
        </h2>
        <p class="text-sm text-gray-400 dark:text-gray-400">Máy tính giả lập điểm số và dự báo kết quả thi cuối kỳ trực tiếp dựa trên DB.</p>
      </div>
    </div>

    <!-- LOADING OVERLAY -->
    <div v-if="loadingOptions" class="flex justify-center py-8">
      <RefreshCw class="animate-spin text-blue-500" :size="32" />
    </div>

    <div v-else class="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start relative">
      <div v-if="calculating" class="absolute inset-0 bg-white/20 dark:bg-[#111C44]/20 backdrop-blur-[1px] z-10 rounded-2xl"></div>

      <!-- LEFTPANEL: INPUTS & PARAMETERS -->
      <div class="lg:col-span-4 space-y-6">
        
        <!-- Selection Card -->
        <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 p-6">
          <h3 class="text-xs font-bold text-gray-400 dark:text-gray-500 uppercase tracking-wider mb-4 border-b border-gray-100 dark:border-white/5 pb-2">Đối tượng phân tích</h3>
          
          <div class="space-y-4">
            <div>
              <label class="block text-sm font-bold text-[#2B3674] dark:text-white mb-2 flex items-center gap-2">
                <Hash :size="16" class="text-orange-500" /> Lớp Học
              </label>
              <select v-model="form.classId" class="w-full bg-gray-50 dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 text-gray-700 dark:text-gray-200 py-2.5 px-4 rounded-xl text-sm font-bold focus:outline-none focus:border-blue-500 outline-none transition-colors cursor-pointer">
                <option v-for="c in classes" :key="c.MaLop" :value="c.MaLop">Lớp {{ c.TenLop }}</option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-bold text-[#2B3674] dark:text-white mb-2 flex items-center gap-2">
                <UserSquare :size="16" class="text-blue-500" /> Học Sinh
              </label>
              <select v-model="form.studentId" :disabled="students.length === 0" class="w-full bg-gray-50 dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 text-gray-700 dark:text-gray-200 py-2.5 px-4 rounded-xl text-sm font-bold focus:outline-none focus:border-blue-500 outline-none transition-colors cursor-pointer disabled:opacity-50">
                <option v-for="s in students" :key="s.MaHS" :value="s.MaHS">{{ s.HoTen }} ({{ s.MaHS }})</option>
                <option v-if="students.length === 0" value="">Không có HS</option>
              </select>
            </div>
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-bold text-[#2B3674] dark:text-white mb-2 flex items-center gap-2">
                  <BookOpen :size="16" class="text-purple-500" /> Môn Học
                </label>
                <select v-model="form.subjectId" class="w-full bg-gray-50 dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 text-gray-700 dark:text-gray-200 py-2.5 px-4 rounded-xl text-sm font-bold focus:outline-none focus:border-blue-500 outline-none transition-colors cursor-pointer">
                  <option v-for="s in subjects" :key="s.MaMon" :value="s.MaMon">{{ s.TenMon }}</option>
                </select>
              </div>
              <div>
                <label class="block text-sm font-bold text-[#2B3674] dark:text-white mb-2 ml-1">Kỳ Học</label>
                <select v-model="form.term" class="w-full bg-gray-50 dark:bg-[#0B1437] border border-gray-200 dark:border-white/10 text-gray-700 dark:text-gray-200 py-2.5 px-4 rounded-xl text-sm font-bold focus:outline-none focus:border-blue-500 outline-none transition-colors cursor-pointer">
                  <option :value="1">Học Kỳ 1</option>
                  <option :value="2">Học Kỳ 2</option>
                </select>
              </div>
            </div>
          </div>
        </div>

        <!-- Simulation Params Card -->
        <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 p-6 border-b-4 border-b-blue-500">
          <div class="flex items-center justify-between mb-4 border-b border-gray-100 dark:border-white/5 pb-2">
            <h3 class="text-xs font-bold text-[#2B3674] dark:text-white uppercase tracking-wider">Tham số trượt (Slider)</h3>
            <span class="text-blue-500"><Target :size="14" /></span>
          </div>

          <div class="space-y-6">
            <!-- Target Average -->
            <div class="p-4 bg-gray-50/50 dark:bg-white/5 rounded-xl border border-gray-100 dark:border-white/10">
              <div class="flex justify-between items-end mb-2">
                <label class="block text-[13px] font-bold text-[#2B3674] dark:text-white">Mục tiêu Phẩy mong muốn</label>
                <div class="text-[28px] leading-tight font-extrabold text-[#1E88E5] dark:text-blue-400 w-16 text-right">{{ parseFloat(form.targetAverage).toFixed(1) }}</div>
              </div>
              <input 
                type="range" v-model="form.targetAverage" min="0" max="10" step="0.1" 
                class="w-full h-2 bg-gray-200 dark:bg-gray-700 rounded-lg appearance-none cursor-pointer accent-blue-500"
              />
              <div class="flex justify-between text-[10px] font-bold text-gray-400 dark:text-gray-500 mt-2">
                <span>0.0</span><span>5.0</span><span>10.0</span>
              </div>
            </div>

            <!-- Hypothetical Final -->
            <div class="p-4 bg-gray-50/50 dark:bg-white/5 rounded-xl border border-gray-100 dark:border-white/10">
              <div class="flex justify-between items-end mb-2">
                <label class="block text-[13px] font-bold text-[#2B3674] dark:text-white">Nếu thi cuối kỳ được</label>
                <div class="text-[28px] leading-tight font-extrabold text-purple-500 dark:text-purple-400 w-16 text-right">{{ parseFloat(form.hypotheticalFinal).toFixed(1) }}</div>
              </div>
              <input 
                type="range" v-model="form.hypotheticalFinal" min="0" max="10" step="0.1" 
                class="w-full h-2 bg-gray-200 dark:bg-gray-700 rounded-lg appearance-none cursor-pointer accent-purple-500"
              />
              <div class="flex justify-between text-[10px] font-bold text-gray-400 dark:text-gray-500 mt-2">
                <span>0.0</span><span>5.0</span><span>10.0</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- RIGHT PANEL: ANALYSIS HUD -->
      <div class="lg:col-span-8 space-y-6">
        
        <!-- Process Current State -->
        <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 p-6">
          <h3 class="text-[13px] font-bold text-[#2B3674] dark:text-white uppercase tracking-wider mb-4 border-b border-gray-100 dark:border-white/5 pb-2 flex items-center gap-2">
            <Info :size="16" class="text-blue-500" /> Tình trạng điểm quá trình trên hệ thống
          </h3>
          <div class="grid grid-cols-4 gap-4">
            <div class="p-3 bg-gray-50 dark:bg-[#0B1437] rounded-xl text-center border border-gray-100 dark:border-white/5 shadow-sm">
              <span class="block text-[10px] font-bold text-gray-400 dark:text-gray-500 uppercase">Miệng (10%)</span>
              <strong class="text-xl font-extrabold text-[#2B3674] dark:text-white">{{ currentGrades.oral ?? '-' }}</strong>
            </div>
            <div class="p-3 bg-gray-50 dark:bg-[#0B1437] rounded-xl text-center border border-gray-100 dark:border-white/5 shadow-sm">
              <span class="block text-[10px] font-bold text-gray-400 dark:text-gray-500 uppercase">15 Phút (10%)</span>
              <strong class="text-xl font-extrabold text-[#2B3674] dark:text-white">{{ currentGrades.quiz15 ?? '-' }}</strong>
            </div>
            <div class="p-3 bg-gray-50 dark:bg-[#0B1437] rounded-xl text-center border border-gray-100 dark:border-white/5 shadow-sm bg-blue-50/10 dark:bg-blue-500/[0.02]">
              <span class="block text-[10px] font-bold text-blue-500 dark:text-blue-400 uppercase">Giữa Kỳ (30%)</span>
              <strong class="text-xl font-extrabold text-[#1E88E5] dark:text-blue-400">{{ currentGrades.midterm ?? '-' }}</strong>
            </div>
            <div class="p-3 bg-gray-50 dark:bg-[#0B1437] rounded-xl text-center border-2 border-dashed border-red-200 dark:border-red-500/20 shadow-sm bg-red-50/10 dark:bg-red-500/[0.02]">
              <span class="block text-[10px] font-bold text-red-400 dark:text-red-500 uppercase">Cuối kỳ cũ (50%)</span>
              <strong class="text-xl font-extrabold text-red-500 dark:text-red-400">{{ currentGrades.final ?? 'Chưa thi' }}</strong>
            </div>
          </div>
        </div>

        <!-- The Double HUD -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-6">
          
          <!-- HUD 1: Target Required Analysis -->
          <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 p-6 relative overflow-hidden group border-b-4 border-b-[#1E88E5]">
            <div class="absolute -right-4 -top-4 opacity-[0.03] group-hover:opacity-10 transition-opacity">
              <Target :size="120" />
            </div>
            <h3 class="text-[11px] font-bold text-gray-400 dark:text-gray-500 uppercase tracking-widest mb-1">Mục tiêu Phẩy: <span class="text-[#2B3674] dark:text-white border-b border-dashed">{{ parseFloat(form.targetAverage).toFixed(1) }}</span></h3>
            
            <p class="text-[13px] font-bold text-gray-500 dark:text-gray-400 mb-2 mt-4">Điểm thi Cuối kỳ tối thiểu cần đạt:</p>
            <div class="flex items-end gap-3 min-h-[72px]">
              <span class="text-[72px] font-black leading-none tracking-tighter drop-shadow-sm" :class="getRequiredScoreColor(result.requiredFinalScore)">
                {{ result.requiredFinalScore > 10 ? 'N/A' : result.requiredFinalScore.toFixed(1) }}
              </span>
              <span class="text-lg font-bold text-gray-400 dark:text-gray-500 pb-2">/ 10</span>
            </div>
            
            <div class="mt-6 pt-4 border-t border-gray-100 dark:border-white/5 flex items-start gap-2">
              <AlertTriangle v-if="result.requiredFinalScore > 10" :size="16" class="text-red-500 mt-0.5 shrink-0" />
              <CheckCircle v-else-if="result.requiredFinalScore <= parseFloat(form.hypotheticalFinal)" :size="16" class="text-green-500 mt-0.5 shrink-0" />
              <TrendingUp v-else :size="16" class="text-orange-500 mt-0.5 shrink-0" />
              
              <p class="text-xs font-medium text-gray-500 dark:text-gray-400 leading-relaxed">
                <span v-if="result.requiredFinalScore > 10">Mục tiêu này <b>BẤT KHẢ THI</b> do điểm quá trình trung bình quá thấp.</span>
                <span v-else-if="result.requiredFinalScore <= parseFloat(form.hypotheticalFinal)">Tuyệt vời! Điểm thi giả định {{ parseFloat(form.hypotheticalFinal).toFixed(1) }} vượt mức mong đợi, mục tiêu <b>KHẢ THI</b>!</span>
                <span v-else>Cần cố gắng ôn tập để có thể làm được đề đạt tối thiểu <b>{{ result.requiredFinalScore.toFixed(1) }}đ</b>.</span>
              </p>
            </div>
          </div>

          <!-- HUD 2: What If Analysis -->
          <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 p-6 relative overflow-hidden group border-b-4 border-b-purple-500">
            <div class="absolute -right-4 -top-4 opacity-[0.03] group-hover:opacity-10 transition-opacity">
              <Calculator :size="120" />
            </div>
            <h3 class="text-[11px] font-bold text-gray-400 dark:text-gray-500 uppercase tracking-widest mb-1">Giả định Cuối kỳ: <span class="text-purple-600 dark:text-purple-400 border-b border-dashed">{{ parseFloat(form.hypotheticalFinal).toFixed(1) }}</span></h3>
            
            <p class="text-[13px] font-bold text-gray-500 dark:text-gray-400 mb-2 mt-4">Tổng kết Phẩy thực tế sẽ là:</p>
            <div class="flex items-end gap-3 min-h-[72px]">
              <span class="text-[72px] font-black leading-none tracking-tighter drop-shadow-sm" :class="getGradeColor(result.predictedAverage)">
                {{ result.predictedAverage }}
              </span>
              <span class="px-2.5 py-1 rounded-md text-[11px] font-extrabold uppercase tracking-widest mb-3 border shadow-sm" :class="classificationColor">
                {{ result.classification === 'Gioi' ? 'Giỏi' : result.classification === 'Kha' ? 'Khá' : result.classification === 'TrungBinh' ? 'Trung Bình' : result.classification === 'Yeu' ? 'Yếu' : result.classification === 'Kem' ? 'Kém' : result.classification }}
              </span>
            </div>

            <div class="mt-6 pt-4 border-t border-gray-100 dark:border-white/5 flex items-start gap-2">
              <p class="text-xs font-medium text-gray-500 dark:text-gray-400 leading-relaxed">
                Đã tính hợp hệ số theo công thức chuẩn hoá phân cực bằng thuật toán Backend EduTrack.
              </p>
            </div>
          </div>

        </div>
      </div>
    </div>
  </div>
</template>
