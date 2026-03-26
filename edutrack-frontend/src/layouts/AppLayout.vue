<script setup>
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

const collapsed = ref(false)

const active = computed(() => route.path)

async function logout() {
  auth.logout()
  await router.push('/login')
}
</script>

<template>
  <el-container class="app">
    <el-aside :width="collapsed ? '64px' : '240px'" class="aside">
      <div class="brand" :class="{ collapsed }">
        <div class="logo">ET</div>
        <div v-if="!collapsed" class="name">EduTrack</div>
      </div>

      <el-menu :default-active="active" router class="menu" :collapse="collapsed" :collapse-transition="false">
        <el-menu-item index="/dashboard">
          <span>Dashboard</span>
        </el-menu-item>
        <el-sub-menu index="/ql">
          <template #title>
            <span>Quản lý</span>
          </template>
          <el-menu-item index="/hocsinh">Học sinh</el-menu-item>
          <el-menu-item index="/lophoc">Lớp học</el-menu-item>
          <el-menu-item index="/monhoc">Môn học</el-menu-item>
          <el-menu-item index="/diemso">Điểm số</el-menu-item>
        </el-sub-menu>
        <el-sub-menu index="/dss">
          <template #title>
            <span>DSS</span>
          </template>
          <el-menu-item index="/dss/what-if">What‑If</el-menu-item>
          <el-menu-item index="/dss/canh-bao">Cảnh báo</el-menu-item>
        </el-sub-menu>
      </el-menu>
    </el-aside>

    <el-container>
      <el-header class="header">
        <div class="left">
          <el-button text @click="collapsed = !collapsed">
            {{ collapsed ? 'Mở menu' : 'Thu gọn' }}
          </el-button>
        </div>
        <div class="right">
          <el-tag type="info" effect="plain">{{ auth.username || 'guest' }}</el-tag>
          <el-tag type="success" effect="plain">{{ auth.role || 'N/A' }}</el-tag>
          <el-button @click="logout">Đăng xuất</el-button>
        </div>
      </el-header>

      <el-main class="main">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<style scoped>
.app {
  min-height: 100vh;
  background: #f6f7fb;
}
.aside {
  background: #0b1220;
  color: #fff;
  border-right: 1px solid rgba(255, 255, 255, 0.08);
}
.brand {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px;
}
.brand.collapsed {
  justify-content: center;
}
.logo {
  width: 32px;
  height: 32px;
  border-radius: 10px;
  display: grid;
  place-items: center;
  background: linear-gradient(135deg, #3b82f6, #a855f7);
  font-weight: 800;
}
.name {
  font-weight: 800;
  letter-spacing: 0.2px;
}
.menu {
  border-right: none;
  background: transparent;
}
.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  background: rgba(255, 255, 255, 0.75);
  backdrop-filter: blur(8px);
  border-bottom: 1px solid rgba(15, 23, 42, 0.08);
}
.right {
  display: flex;
  align-items: center;
  gap: 8px;
}
.main {
  padding: 16px;
}
</style>

