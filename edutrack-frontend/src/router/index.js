import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../views/LoginView.vue'
import AppLayout from '../layouts/AppLayout.vue'
import DashboardView from '../views/DashboardView.vue'
import HocSinhView from '../views/HocSinhView.vue'
import LopHocView from '../views/LopHocView.vue'
import MonHocView from '../views/MonHocView.vue'
import DiemSoView from '../views/DiemSoView.vue'
import DssWhatIfView from '../views/DssWhatIfView.vue'
import DssCanhBaoView from '../views/DssCanhBaoView.vue'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', component: LoginView },
    {
      path: '/',
      component: AppLayout,
      children: [
        { path: '', redirect: '/dashboard' },
        { path: 'dashboard', component: DashboardView },
        { path: 'hocsinh', component: HocSinhView },
        { path: 'lophoc', component: LopHocView },
        { path: 'monhoc', component: MonHocView },
        { path: 'diemso', component: DiemSoView },
        { path: 'dss/what-if', component: DssWhatIfView },
        { path: 'dss/canh-bao', component: DssCanhBaoView },
      ],
    },
  ],
})

router.beforeEach((to) => {
  if (to.path === '/login') return true
  const token = localStorage.getItem('accessToken')
  if (!token) return '/login'
  return true
})

