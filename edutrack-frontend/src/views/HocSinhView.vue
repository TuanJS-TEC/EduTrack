<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { api } from '../services/api'

const loading = ref(false)
const rows = ref([])
const error = ref('')

const filters = reactive({ maLop: '' })

const filtered = computed(() => rows.value)

async function load() {
  loading.value = true
  error.value = ''
  try {
    const { data } = await api.get('/api/hocsinh', { params: { maLop: filters.maLop || undefined } })
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
    <div class="top">
      <div>
        <div class="h1">Học sinh</div>
        <div class="sub">Tìm kiếm & lọc theo lớp</div>
      </div>
      <div class="actions">
        <el-input v-model="filters.maLop" placeholder="Lọc theo mã lớp (vd: 10A1)" style="width: 240px" clearable />
        <el-button :loading="loading" type="primary" @click="load">Áp dụng</el-button>
      </div>
    </div>

    <el-alert v-if="error" :title="error" type="error" show-icon class="mt" />

    <el-card class="mt">
      <el-table :data="filtered" style="width: 100%" v-loading="loading" height="560">
        <el-table-column prop="maHS" label="Mã HS" width="120" />
        <el-table-column prop="hoTen" label="Họ tên" min-width="220" />
        <el-table-column prop="maLop" label="Lớp" width="90" />
        <el-table-column prop="ngaySinh" label="Ngày sinh" width="130" />
        <el-table-column prop="email_PhuHuynh" label="Email PH" min-width="220" />
        <el-table-column prop="sDT_PhuHuynh" label="SĐT PH" width="140" />
      </el-table>
    </el-card>
  </div>
</template>

<style scoped>
.wrap {
  padding: 4px;
}
.top {
  display: flex;
  justify-content: space-between;
  align-items: end;
  gap: 12px;
}
.actions {
  display: flex;
  gap: 8px;
  align-items: center;
}
.h1 {
  font-size: 20px;
  font-weight: 800;
}
.sub {
  margin-top: 4px;
  color: rgba(15, 23, 42, 0.7);
}
.mt {
  margin-top: 12px;
}
</style>

