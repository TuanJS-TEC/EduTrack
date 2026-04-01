import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../views/LoginView.vue'
import AppLayout from '../layouts/AppLayout.vue'
import DashboardView from '../views/DashboardView.vue'

// Temporary generic views or redirect for unimplemented ones
const GenericView = { template: '<div class="p-8"><el-empty description="Tính năng đang được phát triển" /></div>' }

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', component: LoginView },
    {
      path: '/',
      component: AppLayout,
      children: [
        { path: '', redirect: '/dashboard' },
        { path: 'dashboard', component: DashboardView, meta: { title: 'Dashboard' } },
        { path: 'hoc-sinh', component: () => import('../views/HocSinhView.vue').catch(() => GenericView), meta: { title: 'Quản lý Học sinh' } },
        { path: 'giao-vien', component: () => import('../views/GiaoVienView.vue').catch(() => GenericView), meta: { title: 'Quản lý Giáo viên' } },
        { path: 'lop-hoc', component: () => import('../views/LopHocView.vue').catch(() => GenericView), meta: { title: 'Quản lý Lớp học' } },
        { path: 'mon-hoc', component: () => import('../views/MonHocView.vue').catch(() => GenericView), meta: { title: 'Quản lý Môn học' } },
        { path: 'diem-so', component: () => import('../views/DiemSoView.vue').catch(() => GenericView), meta: { title: 'Sổ điểm & Học bạ' } },
        { path: 'lich-hoc', component: () => import('../views/LichHocView.vue').catch(() => GenericView), meta: { title: 'Thời khóa biểu' } },
        { path: 'hoc-phi', component: () => import('../views/HocPhiView.vue').catch(() => GenericView), meta: { title: 'Quản lý Học phí' } },
        { path: 'thong-bao', component: () => import('../views/ThongBaoView.vue').catch(() => GenericView), meta: { title: 'Hệ thống Thông báo' } },
        { path: 'dss/what-if', component: () => import('../views/DssWhatIfView.vue').catch(() => GenericView), meta: { title: 'DSS What-If' } },
        { path: 'dss/canh-bao', component: () => import('../views/DssCanhBaoView.vue').catch(() => GenericView), meta: { title: 'DSS Cảnh báo' } },
        // Catch-all route to redirect any old path to dashboard
        { path: ':pathMatch(.*)*', redirect: '/dashboard' }
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
