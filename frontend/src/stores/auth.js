import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { login as loginApi, getUserInfo } from '@/api/auth'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('token') || '')
  const userInfo = ref(null)

  const isLoggedIn = computed(() => !!token.value)
  const userRole = computed(() => userInfo.value?.role || '')
  const userName = computed(() => userInfo.value?.name || '')

  // 角色判断
  const isVisitor = computed(() => userRole.value === 'visitor')
  const isAdmin = computed(() => userRole.value === 'admin')
  const isSecurity = computed(() => userRole.value === 'security')

  async function login(credentials) {
    const res = await loginApi(credentials)
    token.value = res.token
    localStorage.setItem('token', res.token)
    await fetchUserInfo()
    return res
  }

  async function fetchUserInfo() {
    try {
      userInfo.value = await getUserInfo()
    } catch {
      logout()
    }
  }

  function logout() {
    token.value = ''
    userInfo.value = null
    localStorage.removeItem('token')
  }

  return {
    token,
    userInfo,
    isLoggedIn,
    userRole,
    userName,
    isVisitor,
    isAdmin,
    isSecurity,
    login,
    fetchUserInfo,
    logout,
  }
})
