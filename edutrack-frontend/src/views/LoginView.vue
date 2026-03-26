<script setup>
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const auth = useAuthStore()

const loading = ref(false)
const form = reactive({ username: 'admin', password: 'admin' })
const error = ref('')

async function onSubmit() {
  error.value = ''
  loading.value = true
  try {
    await auth.login(form.username, form.password)
    await router.push('/dashboard')
  } catch (e) {
    error.value = e?.response?.data?.message || 'Đăng nhập thất bại'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="page">
    <el-card class="card">
      <div class="title">EduTrack</div>
      <el-form label-position="top" @submit.prevent="onSubmit">
        <el-form-item label="Username">
          <el-input v-model="form.username" autocomplete="username" />
        </el-form-item>
        <el-form-item label="Password">
          <el-input v-model="form.password" type="password" autocomplete="current-password" show-password />
        </el-form-item>
        <el-alert v-if="error" :title="error" type="error" show-icon class="mb" />
        <el-button type="primary" :loading="loading" style="width: 100%" @click="onSubmit">
          Đăng nhập
        </el-button>
      </el-form>
    </el-card>
  </div>
</template>

<style scoped>
.page {
  min-height: 100vh;
  display: grid;
  place-items: center;
  padding: 24px;
  background: radial-gradient(1200px 600px at 20% 10%, #e8f3ff, transparent),
    radial-gradient(900px 500px at 90% 30%, #f0eaff, transparent),
    linear-gradient(180deg, #f8fafc, #ffffff);
}
.card {
  width: min(420px, 100%);
}
.title {
  font-size: 22px;
  font-weight: 700;
  margin-bottom: 16px;
}
.mb {
  margin-bottom: 12px;
}
</style>

