import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../views/LoginView.vue'
import AppLayout from '../layouts/AppLayout.vue'
import DashboardView from '../views/DashboardView.vue'

const GenericView = { template: '<div class="p-8"><el-empty description="Tính năng đang được phát triển" /></div>' }

/** Trả về route đầu tiên mà user có quyền truy cập */
export function getFirstAccessibleRoute(permissions = [], roles = []) {
  if (permissions.includes('Dashboard.View')) return '/dashboard'
  if (permissions.includes('Scores.View')) return '/diem-so'
  if (permissions.includes('Students.View')) return '/hoc-sinh'
  if (permissions.includes('Finance.View') || permissions.includes('Finance.Manage')) return '/hoc-phi'
  return '/thong-bao'
}

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', component: LoginView },
    {
      path: '/',
      component: AppLayout,
      children: [
        {
          path: '',
          redirect: () => {
            const perms = JSON.parse(localStorage.getItem('permissions') || '[]')
            const roles = JSON.parse(localStorage.getItem('roles') || '[]')
            return getFirstAccessibleRoute(perms, roles)
          },
        },
        {
          path: 'dashboard',
          component: DashboardView,
          meta: { title: 'Dashboard', permission: 'Dashboard.View' },
        },
        {
          path: 'hoc-sinh',
          component: () => import('../views/HocSinhView.vue').catch(() => GenericView),
          meta: { title: 'Quản lý Học sinh', permission: 'Students.View' },
        },
        {
          path: 'giao-vien',
          component: () => import('../views/GiaoVienView.vue').catch(() => GenericView),
          meta: { title: 'Quản lý Giáo viên', permission: 'Teachers.View' },
        },
        {
          path: 'lop-hoc',
          component: () => import('../views/LopHocView.vue').catch(() => GenericView),
          meta: { title: 'Quản lý Lớp học', roles: ['Admin', 'BGH', 'Teacher'] },
        },
        {
          path: 'mon-hoc',
          component: () => import('../views/MonHocView.vue').catch(() => GenericView),
          meta: { title: 'Quản lý Môn học', roles: ['Admin', 'BGH', 'Teacher'] },
        },
        {
          path: 'diem-so',
          component: () => import('../views/DiemSoView.vue').catch(() => GenericView),
          meta: { title: 'Sổ điểm & Học bạ', permission: 'Scores.View' },
        },
        {
          path: 'lich-hoc',
          component: () => import('../views/LichHocView.vue').catch(() => GenericView),
          meta: { title: 'Thời khóa biểu', roles: ['Admin', 'BGH', 'Teacher'] },
        },
        {
          path: 'hoc-phi',
          component: () => import('../views/HocPhiView.vue').catch(() => GenericView),
          meta: { title: 'Quản lý Học phí', permission: 'Finance.View' },
        },
        {
          path: 'thong-bao',
          component: () => import('../views/ThongBaoView.vue').catch(() => GenericView),
          meta: { title: 'Hệ thống Thông báo' },
        },
        {
          path: 'profile',
          component: () => import('../views/ProfileView.vue').catch(() => GenericView),
          meta: { title: 'Hồ sơ cá nhân' },
        },
        {
          path: 'settings',
          component: () => import('../views/SettingsView.vue').catch(() => GenericView),
          meta: { title: 'Cài đặt' },
        },
        {
          path: 'dss/what-if',
          component: () => import('../views/DssWhatIfView.vue').catch(() => GenericView),
          meta: { title: 'DSS What-If', permission: 'Scores.Edit' },
        },
        {
          path: 'dss/canh-bao',
          component: () => import('../views/DssCanhBaoView.vue').catch(() => GenericView),
          meta: { title: 'DSS Cảnh báo', permission: 'Scores.View' },
        },
        {
          path: ':pathMatch(.*)*',
          redirect: () => {
            const perms = JSON.parse(localStorage.getItem('permissions') || '[]')
            const roles = JSON.parse(localStorage.getItem('roles') || '[]')
            return getFirstAccessibleRoute(perms, roles)
          },
        },
      ],
    },
  ],
})

router.beforeEach((to) => {
  if (to.path === '/login') return true

  const token = localStorage.getItem('accessToken')
  if (!token) return '/login'

  const permissions = JSON.parse(localStorage.getItem('permissions') || '[]')
  const roles = JSON.parse(localStorage.getItem('roles') || '[]')
  const { permission, roles: requiredRoles } = to.meta || {}

  if (permission && !permissions.includes(permission)) {
    return getFirstAccessibleRoute(permissions, roles)
  }

  if (requiredRoles && Array.isArray(requiredRoles) && !requiredRoles.some((r) => roles.includes(r))) {
    return getFirstAccessibleRoute(permissions, roles)
  }

  return true
})
