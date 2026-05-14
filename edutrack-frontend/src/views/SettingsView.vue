<script setup>
import { ref, onMounted } from 'vue'
import { Moon, Sun, Bell } from 'lucide-vue-next'

const isDark = ref(false)

function applyTheme(dark) {
  isDark.value = dark
  if (dark) {
    document.documentElement.classList.add('dark')
    localStorage.setItem('theme', 'dark')
  } else {
    document.documentElement.classList.remove('dark')
    localStorage.setItem('theme', 'light')
  }
}

function toggleTheme() {
  applyTheme(!isDark.value)
}

onMounted(() => {
  isDark.value =
    localStorage.getItem('theme') === 'dark' ||
    (!('theme' in localStorage) && window.matchMedia('(prefers-color-scheme: dark)').matches)
  if (isDark.value) document.documentElement.classList.add('dark')
})
</script>

<template>
  <div class="space-y-6 max-w-3xl">
    <div>
      <h2 class="text-2xl font-bold text-[#2B3674] dark:text-white mb-1">Cài đặt</h2>
      <p class="text-sm text-gray-400 dark:text-gray-400">Giao diện và thông báo cơ bản.</p>
    </div>

    <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 p-6 space-y-6">
      <div class="flex items-center justify-between gap-4">
        <div class="flex items-center gap-3">
          <component :is="isDark ? Sun : Moon" class="text-[#1E88E5] dark:text-blue-400" :size="22" />
          <div>
            <p class="font-bold text-[#2B3674] dark:text-white">Giao diện sáng / tối</p>
            <p class="text-xs text-gray-500 dark:text-gray-400">Đồng bộ với nút chuyển theme trên thanh header.</p>
          </div>
        </div>
        <button
          type="button"
          @click="toggleTheme"
          class="px-4 py-2 rounded-xl text-sm font-bold border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-[#2B3674] dark:text-white hover:bg-gray-100 dark:hover:bg-white/10 transition-colors"
        >
          {{ isDark ? 'Chuyển sáng' : 'Chuyển tối' }}
        </button>
      </div>

      <div class="border-t border-gray-100 dark:border-white/5 pt-6 flex items-start gap-3">
        <Bell class="text-gray-400 shrink-0 mt-0.5" :size="22" />
        <div>
          <p class="font-bold text-[#2B3674] dark:text-white">Thông báo</p>
          <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
            Xem toàn bộ thông báo trong mục <router-link to="/thong-bao" class="text-[#1E88E5] dark:text-blue-400 font-bold hover:underline">Hệ thống Thông báo</router-link>.
          </p>
        </div>
      </div>
    </div>
  </div>
</template>
