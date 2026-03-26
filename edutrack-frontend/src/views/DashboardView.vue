<script setup>
import { onMounted, ref } from 'vue'
import { useAuthStore } from '../stores/auth'
import { api } from '../services/api'

const auth = useAuthStore()

const loading = ref(false)
const stats = ref(null)
const error = ref('')

async function load() {
  loading.value = true
  error.value = ''
  try {
    const { data } = await api.get('/api/dss/dashboard-hoc-luc', { params: { hocKy: 1, namHoc: '2025-2026' } })
    stats.value = data
  } catch (e) {
    error.value = e?.response?.data?.message || 'Không tải được dashboard'
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
        <div class="h1">Dashboard</div>
        <div class="sub">Tổng quan học lực (HK1 • 2025-2026)</div>
      </div>
      <el-button :loading="loading" @click="load">Tải lại</el-button>
    </div>

    <el-alert
      :title="`Xin chào ${auth.username || 'bạn'} (role: ${auth.role || 'N/A'})`"
      type="success"
      show-icon
      class="mt"
    />

    <el-alert v-if="error" :title="error" type="error" show-icon class="mt" />

    <el-skeleton v-if="loading && !stats" :rows="6" animated class="mt" />

    <template v-else>
      <div class="grid mt" v-if="stats">
        <el-card class="kpi">
          <div class="kpiTitle">Tổng học sinh</div>
          <div class="kpiValue">{{ stats.tongHocSinh }}</div>
        </el-card>
        <el-card class="kpi">
          <div class="kpiTitle">Giỏi</div>
          <div class="kpiValue">{{ stats.gioi }}</div>
        </el-card>
        <el-card class="kpi">
          <div class="kpiTitle">Khá</div>
          <div class="kpiValue">{{ stats.kha }}</div>
        </el-card>
        <el-card class="kpi">
          <div class="kpiTitle">Trung bình</div>
          <div class="kpiValue">{{ stats.trungBinh }}</div>
        </el-card>
        <el-card class="kpi">
          <div class="kpiTitle">Yếu</div>
          <div class="kpiValue">{{ stats.yeu }}</div>
        </el-card>
        <el-card class="kpi">
          <div class="kpiTitle">Kém</div>
          <div class="kpiValue">{{ stats.kem }}</div>
        </el-card>
      </div>

      <el-card class="mt" v-if="stats">
        <div style="font-weight: 700; margin-bottom: 8px">TB chung theo lớp</div>
        <el-table :data="stats.theoLop" style="width: 100%">
          <el-table-column prop="maLop" label="Mã lớp" width="120" />
          <el-table-column prop="tenLop" label="Tên lớp" />
          <el-table-column prop="siSo" label="Sĩ số" width="100" />
          <el-table-column prop="tbChung" label="TB chung" width="120" />
        </el-table>
      </el-card>
    </template>
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
.grid {
  display: grid;
  grid-template-columns: repeat(6, minmax(0, 1fr));
  gap: 12px;
}
.kpiTitle {
  color: rgba(15, 23, 42, 0.65);
  font-weight: 600;
  font-size: 12px;
}
.kpiValue {
  margin-top: 6px;
  font-weight: 900;
  font-size: 22px;
}
@media (max-width: 1200px) {
  .grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}
@media (max-width: 680px) {
  .grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>

