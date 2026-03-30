import { defineStore } from 'pinia'
import { api } from '../services/api'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    accessToken: localStorage.getItem('accessToken') || '',
    refreshToken: localStorage.getItem('refreshToken') || '',
    username: localStorage.getItem('username') || '',
    role: localStorage.getItem('role') || '',
  }),
  getters: {
    isAuthed: (s) => !!s.accessToken,
  },
  actions: {
    async login(username, password) {
      try {
        const { data } = await api.post('/api/auth/login', { username, password })
        
        this.accessToken = data.accessToken
        this.refreshToken = data.refreshToken
        this.username = data.username
        this.role = data.role
        
        localStorage.setItem('accessToken', this.accessToken)
        localStorage.setItem('refreshToken', this.refreshToken)
        localStorage.setItem('username', this.username)
        localStorage.setItem('role', this.role)
      } catch (error) {
        throw error
      }
    },
    logout() {
      this.accessToken = ''
      this.refreshToken = ''
      this.username = ''
      this.role = ''
      localStorage.removeItem('accessToken')
      localStorage.removeItem('refreshToken')
      localStorage.removeItem('username')
      localStorage.removeItem('role')
    },
  },
})

