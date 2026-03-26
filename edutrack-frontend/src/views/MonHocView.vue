<script setup>
import { onMounted, reactive, ref } from 'vue'
import { api } from '../services/api'

const loading = ref(false)
const rows = ref([])
const error = ref('')

const filters = reactive({ maGV: '' })

async function load() {
  loading.value = true
  error.value = ''
  try {
    const { data } = await api.get('/api/monhoc', { params: { maGV: filters.maGV || undefined } })
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
        <div class="h1">Môn học</div>
        <div class="sub">Danh mục môn học</div>
      </div>
      <div class="actions">
        <el-input v-model="filters.maGV" placeholder="Lọc theo mã GV" style="width: 160px" clearable />
        <el-button :loading="loading" type="primary" @click="load">Tải</el-button>
      </div>
    </div>

    <el-alert v-if="error" :title="error" type="error" show-icon class="mt" />

    <el-card class="mt">
      <el-table :data="rows" v-loading="loading" style="width: 100%">
        <el-table-column prop="maMon" label="Mã môn" width="120" />
        <el-table-column prop="tenMon" label="Tên môn" />
        <el-table-column prop="soTiet" label="Số tiết" width="100" />
        <el-table-column prop="heSoThi" label="Hệ số" width="100" />
        <el-table-column prop="maGV" label="Mã GV" width="120" />
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

