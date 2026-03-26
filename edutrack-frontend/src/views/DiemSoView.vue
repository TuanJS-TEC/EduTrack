<script setup>
import { onMounted, reactive, ref } from 'vue'
import { api } from '../services/api'

const loading = ref(false)
const error = ref('')
const rows = ref([])

const filters = reactive({
  maLop: '10A1',
  maMon: 'TOAN',
  hocKy: 1,
})

async function loadBangDiem() {
  loading.value = true
  error.value = ''
  try {
    const { data } = await api.get('/api/diemso/bangdiem', { params: { ...filters } })
    rows.value = data
  } catch (e) {
    error.value = e?.response?.data?.message || 'Không tải được bảng điểm'
  } finally {
    loading.value = false
  }
}

onMounted(loadBangDiem)
</script>

<template>
  <div class="wrap">
    <div class="top">
      <div>
        <div class="h1">Điểm số</div>
        <div class="sub">Bảng điểm theo lớp • môn • học kỳ</div>
      </div>
      <div class="actions">
        <el-input v-model="filters.maLop" placeholder="Mã lớp" style="width: 110px" />
        <el-input v-model="filters.maMon" placeholder="Mã môn" style="width: 110px" />
        <el-input-number v-model="filters.hocKy" :min="1" :max="2" style="width: 110px" />
        <el-button :loading="loading" type="primary" @click="loadBangDiem">Tải</el-button>
      </div>
    </div>

    <el-alert v-if="error" :title="error" type="error" show-icon class="mt" />

    <el-card class="mt">
      <el-table :data="rows" v-loading="loading" style="width: 100%" height="560">
        <el-table-column prop="maHS" label="Mã HS" width="110" />
        <el-table-column prop="hoTen" label="Họ tên" min-width="220" />
        <el-table-column prop="diemMieng" label="Miệng" width="90" />
        <el-table-column prop="diem15p" label="15p" width="90" />
        <el-table-column prop="diemGiuaKy" label="GK" width="90" />
        <el-table-column prop="diemCuoiKy" label="CK" width="90" />
        <el-table-column prop="diemTBMon" label="TB" width="90" />
        <el-table-column prop="xepLoai" label="Xếp loại" width="120" />
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

