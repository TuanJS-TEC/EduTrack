<script setup>
import { onMounted, ref } from 'vue'
import { api } from '../services/api'

const loading = ref(false)
const rows = ref([])
const error = ref('')

async function load() {
  loading.value = true
  error.value = ''
  try {
    const { data } = await api.get('/api/hocsinh')
    rows.value = data
  } catch (e) {
    error.value = e?.response?.data?.message || 'Không tải được dữ liệu'
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<template>
  <div class="wrap">
    <el-page-header content="Quản lý học sinh" />

    <el-alert v-if="error" :title="error" type="error" show-icon class="mt" />

    <el-card class="mt">
      <div style="display: flex; justify-content: space-between; align-items: center; gap: 12px">
        <div style="font-weight: 600">Danh sách học sinh</div>
        <el-button :loading="loading" @click="load">Tải lại</el-button>
      </div>
      <el-table :data="rows" style="width: 100%" class="mt" v-loading="loading">
        <el-table-column prop="maHS" label="Mã HS" width="140" />
        <el-table-column prop="hoTen" label="Họ tên" />
        <el-table-column prop="maLop" label="Lớp" width="120" />
        <el-table-column prop="email_PhuHuynh" label="Email PH" />
        <el-table-column prop="sDT_PhuHuynh" label="SĐT PH" width="140" />
      </el-table>
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

