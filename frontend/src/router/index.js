import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const routes = [
  {
    path: '/',
    redirect: (to) => {
      // 动态重定向：根据 localStorage 中的角色信息决定跳转
      const token = localStorage.getItem('token')
      if (!token) return '/visitor/reservation'
      try {
        const payload = JSON.parse(atob(token.split('.')[1]))
        const roleRoutes = { admin: '/admin/dashboard', security: '/security/entry-check', staff: '/visitor/reservation', visitor: '/visitor/reservation' }
        return roleRoutes[payload.role] || '/visitor/reservation'
      } catch {
        return '/visitor/reservation'
      }
    },
  },
  // ========== 访客端 ==========
  {
    path: '/visitor',
    component: () => import('@/components/VisitorLayout.vue'),
    children: [
      {
        path: 'reservation',
        name: 'Reservation',
        component: () => import('@/views/visitor/Reservation.vue'),
        meta: { title: '入校预约' },
      },
      {
        path: 'activities',
        name: 'Activities',
        component: () => import('@/views/visitor/ActivityList.vue'),
        meta: { title: '校园活动' },
      },
      {
        path: 'my-reservations',
        name: 'MyReservations',
        component: () => import('@/views/visitor/MyReservations.vue'),
        meta: { title: '我的预约', requiresAuth: true },
      },
      {
        path: 'profile',
        name: 'VisitorProfile',
        component: () => import('@/views/visitor/VisitorProfile.vue'),
        meta: { title: '个人中心', requiresAuth: true },
      },
    ],
  },
  // ========== 管理员端 ==========
  {
    path: '/admin',
    component: () => import('@/components/AdminLayout.vue'),
    meta: { requiresAuth: true, role: 'admin' },
    children: [
      {
        path: 'dashboard',
        name: 'Dashboard',
        component: () => import('@/views/admin/Dashboard.vue'),
        meta: { title: '数据看板' },
      },
      {
        path: 'review',
        name: 'ReservationReview',
        component: () => import('@/views/admin/ReservationReview.vue'),
        meta: { title: '预约审核' },
      },
      {
        path: 'open-rules',
        name: 'OpenRules',
        component: () => import('@/views/admin/OpenRules.vue'),
        meta: { title: '开放规则' },
      },
      {
        path: 'areas',
        name: 'AreaManagement',
        component: () => import('@/views/admin/AreaManagement.vue'),
        meta: { title: '区域管理' },
      },
      {
        path: 'activities',
        name: 'ActivityManagement',
        component: () => import('@/views/admin/ActivityManagement.vue'),
        meta: { title: '活动管理' },
      },
      {
        path: 'blacklist',
        name: 'Blacklist',
        component: () => import('@/views/admin/Blacklist.vue'),
        meta: { title: '黑名单管理' },
      },
      {
        path: 'schedules',
        name: 'StaffSchedule',
        component: () => import('@/views/admin/StaffSchedule.vue'),
        meta: { title: '排班管理' },
      },
      {
        path: 'audit-logs',
        name: 'AuditLog',
        component: () => import('@/views/admin/AuditLog.vue'),
        meta: { title: '审计日志' },
      },
      {
        path: 'reports',
        name: 'ReportManagement',
        component: () => import('@/views/admin/ReportManagement.vue'),
        meta: { title: '举报管理' },
      },
    ],
  },
  // ========== 安保端 ==========
  {
    path: '/security',
    component: () => import('@/components/SecurityLayout.vue'),
    meta: { requiresAuth: true, role: 'security' },
    children: [
      {
        path: 'entry-check',
        name: 'EntryCheck',
        component: () => import('@/views/security/EntryCheck.vue'),
        meta: { title: '入校核验' },
      },
      {
        path: 'exit-record',
        name: 'ExitRecord',
        component: () => import('@/views/security/ExitRecord.vue'),
        meta: { title: '离校登记' },
      },
      {
        path: 'current-visitors',
        name: 'CurrentVisitors',
        component: () => import('@/views/security/CurrentVisitors.vue'),
        meta: { title: '在校访客' },
      },
      {
        path: 'report',
        name: 'SecurityReport',
        component: () => import('@/views/security/SecurityReport.vue'),
        meta: { title: '违规举报' },
      },
    ],
  },
  // ========== 认证页 ==========
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/auth/Login.vue'),
    meta: { title: '登录' },
  },
  {
    path: '/register',
    name: 'Register',
    component: () => import('@/views/auth/Register.vue'),
    meta: { title: '注册' },
  },
  // ========== 404 ==========
  {
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    component: () => import('@/views/NotFound.vue'),
    meta: { title: '页面不存在' },
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

// 路由守卫 - 权限控制
router.beforeEach(async (to, from, next) => {
  document.title = to.meta.title ? `${to.meta.title} - 校园访客管理系统` : '校园访客管理系统'

  const authStore = useAuthStore()

  // 如果有 token 但未加载用户信息，自动拉取
  if (authStore.isLoggedIn && !authStore.userInfo) {
    try {
      await authStore.fetchUserInfo()
    } catch {
      authStore.logout()
      return next({ path: '/login' })
    }
  }

  // 需要认证
  if (to.meta.requiresAuth && !authStore.isLoggedIn) {
    return next({ path: '/login', query: { redirect: to.fullPath } })
  }

  // 角色权限校验
  if (to.meta.role && authStore.userRole !== to.meta.role) {
    return next({ path: '/login' })
  }

  next()
})

export default router
