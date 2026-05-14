import { defineStore } from 'pinia'
import { api } from '../services/api'

const ROLE_LABELS = {
  Admin: 'Quản trị viên',
  BGH: 'Ban Giám Hiệu',
  Teacher: 'Giáo viên',
  Accountant: 'Kế toán',
  Parent: 'Phụ huynh',
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    accessToken: localStorage.getItem('accessToken') || '',
    refreshToken: localStorage.getItem('refreshToken') || '',
    userId: localStorage.getItem('userId') || '',
    username: localStorage.getItem('username') || '',
    maGV: localStorage.getItem('maGV') || '',
    hoTen: localStorage.getItem('hoTen') || '',
    roles: JSON.parse(localStorage.getItem('roles') || '[]'),
    permissions: JSON.parse(localStorage.getItem('permissions') || '[]'),
  }),
  getters: {
    isAuthed: (s) => !!s.accessToken,
    primaryRole: (s) => s.roles[0] || '',
    roleLabel: (s) => ROLE_LABELS[s.roles[0]] || s.roles[0] || 'Người dùng',
    isAdmin: (s) => s.roles.includes('Admin'),
    isBGH: (s) => s.roles.includes('BGH'),
    isTeacher: (s) => s.roles.includes('Teacher'),
    isAccountant: (s) => s.roles.includes('Accountant'),
    isParent: (s) => s.roles.includes('Parent'),
    hasPermission: (s) => (perm) => s.permissions.includes(perm),
  },
  actions: {
    async login(username, password) {
      const { data } = await api.post('/api/auth/login', { username, password })
      this.accessToken = data.AccessToken || data.accessToken || ''
      this.refreshToken = data.RefreshToken || data.refreshToken || ''
      this.userId = data.UserId || data.userId || ''
      this.username = data.Username || data.username || ''
      this.roles = data.Roles || data.roles || []
      this.permissions = data.Permissions || data.permissions || []
      this.maGV = data.MaGV ?? data.maGV ?? ''
      this.hoTen = data.HoTen ?? data.hoTen ?? ''

      localStorage.setItem('accessToken', this.accessToken)
      localStorage.setItem('refreshToken', this.refreshToken)
      localStorage.setItem('userId', this.userId)
      localStorage.setItem('username', this.username)
      localStorage.setItem('roles', JSON.stringify(this.roles))
      localStorage.setItem('permissions', JSON.stringify(this.permissions))
      if (this.maGV) localStorage.setItem('maGV', this.maGV)
      else localStorage.removeItem('maGV')
      if (this.hoTen) localStorage.setItem('hoTen', this.hoTen)
      else localStorage.removeItem('hoTen')
    },
    logout() {
      this.accessToken = ''
      this.refreshToken = ''
      this.userId = ''
      this.username = ''
      this.roles = []
      this.permissions = []
      this.maGV = ''
      this.hoTen = ''
      localStorage.removeItem('accessToken')
      localStorage.removeItem('refreshToken')
      localStorage.removeItem('userId')
      localStorage.removeItem('username')
      localStorage.removeItem('roles')
      localStorage.removeItem('permissions')
      localStorage.removeItem('maGV')
      localStorage.removeItem('hoTen')
    },
  },
})
