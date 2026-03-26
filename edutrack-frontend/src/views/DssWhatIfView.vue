<script setup>
import { onMounted, reactive, ref } from 'vue'
import { api } from '../services/api'

const loading = ref(false)
const error = ref('')
const result = ref(null)

const form = reactive({
  maHS: 'HS014',
  maMon: 'TOAN',
  hocKy: 1,
  diemCuoiKyGiaDinh: 7.0,
  targetTb: 5.0,
})

async function run() {
  loading.value = true
  error.value = ''
  result.value = null
  try {
    const { data } = await api.post('/api/dss/what-if', form)
    result.value = data
  } catch (e) {
    error.value = e?.response?.data?.message || 'Không chạy được What‑If'
  } finally {
    loading.value = false
  }
}

onMounted(run)
</script>

<template>
  <div class="wrap">
    <div class="top">
      <div>
        <div class="h1">DSS • What‑If</div>
        <div class="sub">Giả định điểm CK và tính tổng kết</div>
      </div>
      <el-button :loading="loading" type="primary" @click="run">Chạy</el-button>
    </div>

    <el-alert v-if="error" :title="error" type="error" show-icon class="mt" />

    <el-card class="mt">
      <el-form label-position="top" class="grid">
        <el-form-item label="Mã HS">
          <el-input v-model="form.maHS" />
        </el-form-item>
        <el-form-item label="Mã môn">
          <el-input v-model="form.maMon" />
        </el-form-item>
        <el-form-item label="Học kỳ">
          <el-input-number v-model="form.hocKy" :min="1" :max="2" style="width: 100%" />
        </el-form-item>
        <el-form-item label="CK giả định">
          <el-input-number v-model="form.diemCuoiKyGiaDinh" :min="0" :max="10" :step="0.25" style="width: 100%" />
        </el-form-item>
        <el-form-item label="Mục tiêu TB">
          <el-input-number v-model="form.targetTb" :min="0" :max="10" :step="0.25" style="width: 100%" />
        </el-form-item>
      </el-form>
    </el-card>

    <el-card class="mt" v-if="result">
      <div style="font-weight: 800; margin-bottom: 10px">Kết quả</div>
      <div class="cards">
        <el-card shadow="never" class="mini">
          <div class="k">TB (giả định)</div>
          <div class="v">{{ result.tbGiaDinh ?? '—' }}</div>
          <div class="s">Xếp loại: {{ result.xepLoaiGiaDinh ?? '—' }}</div>
        </el-card>
        <el-card shadow="never" class="mini">
          <div class="k">CK cần thiết</div>
          <div class="v">{{ result.ckCanThietDeDatTarget }}</div>
          <div class="s">Để đạt TB ≥ {{ result.targetTb }}</div>
        </el-card>
      </div>

      <el-descriptions :column="4" border class="mt">
        <el-descriptions-item label="Miệng">{{ result.diemMieng ?? '—' }}</el-descriptions-item>
        <el-descriptions-item label="15p">{{ result.diem15p ?? '—' }}</el-descriptions-item>
        <el-descriptions-item label="Giữa kỳ">{{ result.diemGiuaKy ?? '—' }}</el-descriptions-item>
        <el-descriptions-item label="CK hiện tại">{{ result.diemCuoiKyHienTai ?? '—' }}</el-descriptions-item>
      </el-descriptions>
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
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: 12px;
}
.cards {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 12px;
}
.mini .k {
  color: rgba(15, 23, 42, 0.65);
  font-weight: 700;
  font-size: 12px;
}
.mini .v {
  margin-top: 6px;
  font-weight: 900;
  font-size: 24px;
}
.mini .s {
  margin-top: 6px;
  color: rgba(15, 23, 42, 0.65);
}
@media (max-width: 1100px) {
  .grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
@media (max-width: 900px) {
  .cards {
    grid-template-columns: 1fr;
  }
}
</style>

