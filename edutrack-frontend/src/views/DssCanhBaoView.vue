<script setup>
import { onMounted, reactive, ref } from 'vue'
import { api } from '../services/api'

const loading = ref(false)
const error = ref('')
const rows = ref([])

const filters = reactive({
  hocKy: 1,
  maLop: '',
  targetTb: 5.0,
})

function tagType(mucDo) {
  if (mucDo === 'Do') return 'danger'
  if (mucDo === 'Vang') return 'warning'
  return 'success'
}

async function load() {
  loading.value = true
  error.value = ''
  try {
    const { data } = await api.get('/api/dss/canh-bao-roi-mon', { params: { ...filters, maLop: filters.maLop || undefined } })
    rows.value = data
  } catch (e) {
    error.value = e?.response?.data?.message || 'Không tải được cảnh báo'
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
        <div class="h1">DSS • Cảnh báo rớt môn</div>
        <div class="sub">Danh sách HS có nguy cơ (lọc theo lớp)</div>
      </div>
      <div class="actions">
        <el-input-number v-model="filters.hocKy" :min="1" :max="2" style="width: 110px" />
        <el-input v-model="filters.maLop" placeholder="Mã lớp (vd: 10A1)" style="width: 160px" clearable />
        <el-input-number v-model="filters.targetTb" :min="0" :max="10" :step="0.25" style="width: 120px" />
        <el-button :loading="loading" type="primary" @click="load">Tải</el-button>
      </div>
    </div>

    <el-alert v-if="error" :title="error" type="error" show-icon class="mt" />

    <el-card class="mt">
      <el-table :data="rows" v-loading="loading" style="width: 100%" height="560">
        <el-table-column prop="maLop" label="Lớp" width="90" />
        <el-table-column prop="maHS" label="Mã HS" width="110" />
        <el-table-column prop="hoTen" label="Họ tên" min-width="220" />
        <el-table-column prop="tenMon" label="Môn" width="140" />
        <el-table-column prop="diemTBMon" label="TB" width="90" />
        <el-table-column prop="ckCanThiet" label="CK cần" width="100" />
        <el-table-column label="Mức">
          <template #default="{ row }">
            <el-tag :type="tagType(row.mucDo)">{{ row.mucDo }}</el-tag>
          </template>
        </el-table-column>
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
  flex-wrap: wrap;
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

