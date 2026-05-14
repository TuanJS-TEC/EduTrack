<script setup>
import { computed } from 'vue'
import { User, Shield, KeyRound } from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'

const auth = useAuthStore()

const rolesDisplay = computed(() => (auth.roles?.length ? auth.roles.join(', ') : '—'))
const permissionsSorted = computed(() => [...(auth.permissions || [])].sort())
</script>

<template>
  <div class="space-y-6 max-w-3xl">
    <div>
      <h2 class="text-2xl font-bold text-[#2B3674] dark:text-white mb-1">Hồ sơ cá nhân</h2>
      <p class="text-sm text-gray-400 dark:text-gray-400">
        Thông tin tài khoản đang đăng nhập (đồng bộ từ phiên đăng nhập).
      </p>
    </div>

    <div class="bg-white dark:bg-[#111C44] rounded-2xl shadow-sm border border-gray-100/50 dark:border-white/5 overflow-hidden">
      <div class="p-6 border-b border-gray-100 dark:border-white/5 flex items-center gap-4">
        <div class="w-14 h-14 rounded-2xl bg-[#1E88E5]/10 dark:bg-blue-500/20 flex items-center justify-center text-[#1E88E5] dark:text-blue-400">
          <User :size="28" />
        </div>
        <div>
          <p class="text-lg font-extrabold text-[#2B3674] dark:text-white">{{ auth.username || '—' }}</p>
          <p class="text-sm font-bold text-gray-500 dark:text-gray-400">{{ auth.roleLabel }}</p>
        </div>
      </div>

      <dl class="divide-y divide-gray-100 dark:divide-white/5">
        <div class="px-6 py-4 flex flex-col sm:flex-row sm:items-center gap-1 sm:gap-4">
          <dt class="text-xs font-bold text-gray-400 dark:text-gray-500 uppercase tracking-wider sm:w-40 shrink-0 flex items-center gap-2">
            <KeyRound :size="14" /> User ID
          </dt>
          <dd class="text-sm font-mono font-medium text-[#2B3674] dark:text-gray-200 break-all">
            {{ auth.userId || '—' }}
          </dd>
        </div>
        <div class="px-6 py-4 flex flex-col sm:flex-row sm:items-start gap-1 sm:gap-4">
          <dt class="text-xs font-bold text-gray-400 dark:text-gray-500 uppercase tracking-wider sm:w-40 shrink-0 flex items-center gap-2 pt-0.5">
            <Shield :size="14" /> Vai trò
          </dt>
          <dd class="text-sm font-medium text-[#2B3674] dark:text-gray-200">
            {{ rolesDisplay }}
          </dd>
        </div>
        <div class="px-6 py-4">
          <dt class="text-xs font-bold text-gray-400 dark:text-gray-500 uppercase tracking-wider mb-3">
            Quyền (permissions)
          </dt>
          <dd>
            <div v-if="permissionsSorted.length" class="flex flex-wrap gap-2">
              <span
                v-for="p in permissionsSorted"
                :key="p"
                class="inline-flex items-center px-2.5 py-1 rounded-lg text-xs font-bold border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-gray-700 dark:text-gray-300"
              >
                {{ p }}
              </span>
            </div>
            <p v-else class="text-sm text-gray-400 dark:text-gray-500">Không có dữ liệu quyền.</p>
          </dd>
        </div>
      </dl>
    </div>

    <p class="text-xs text-gray-400 dark:text-gray-500">
      Để cập nhật họ tên hoặc mật khẩu, cần chức năng quản trị người dùng trên backend (liên hệ quản trị viên).
    </p>
  </div>
</template>
