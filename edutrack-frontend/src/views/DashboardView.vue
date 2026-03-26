<script setup>
import { useAuthStore } from '../stores/auth'

const auth = useAuthStore()
</script>

<template>
  <div class="wrap">
    <el-page-header content="Dashboard" />
    <el-alert
      :title="`Xin chào ${auth.username || 'bạn'} (role: ${auth.role || 'N/A'})`"
      type="success"
      show-icon
      class="mt"
    />
    <el-card class="mt">
      <div>Gợi ý: vào menu “Học sinh” để xem danh sách.</div>
    </el-card>
  </div>
</template>

<style scoped>
.wrap {
  padding: 16px;
}
.mt {
  margin-top: 12px;
}
</style>

