<template>
  <el-container class="layout-container">
    <!-- 顶部导航 -->
    <el-header class="header">
      <div class="header-left">
        <div class="logo">
          <el-icon :size="28"><School /></el-icon>
          <span class="logo-text">校园开放预约系统</span>
        </div>
      </div>
      <div class="header-right">
        <el-menu
          :default-active="currentRoute"
          mode="horizontal"
          router
          class="nav-menu"
        >
          <el-menu-item index="/visitor/reservation">
            <el-icon><Calendar /></el-icon>
            入校预约
          </el-menu-item>
          <el-menu-item index="/visitor/activities">
            <el-icon><Collection /></el-icon>
            校园活动
          </el-menu-item>
          <el-menu-item index="/visitor/my-reservations">
            <el-icon><List /></el-icon>
            我的预约
          </el-menu-item>
          <el-menu-item index="/visitor/profile">
            <el-icon><User /></el-icon>
            个人中心
          </el-menu-item>
          <el-menu-item index="/visitor/report">
            <el-icon><WarningFilled /></el-icon>
            违规举报
          </el-menu-item>
        </el-menu>
        <div class="user-area">
          <template v-if="authStore.isLoggedIn">
            <el-dropdown trigger="click">
              <span class="user-info">
                <el-avatar :size="32" icon="UserFilled" />
                <span class="username">{{ authStore.userName || '用户' }}</span>
              </span>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item @click="$router.push('/visitor/profile')">
                    个人中心
                  </el-dropdown-item>
                  <el-dropdown-item divided @click="handleLogout">
                    退出登录
                  </el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </template>
          <template v-else>
            <el-button type="primary" size="small" @click="$router.push('/login')">登录</el-button>
            <el-button size="small" @click="$router.push('/register')">注册</el-button>
          </template>
        </div>
      </div>
    </el-header>

    <!-- 主体内容 -->
    <el-main class="main-content">
      <router-view />
    </el-main>

    <!-- 底部 -->
    <el-footer class="footer">
      <span>校园开放预约与访客管理系统 &copy; 2026</span>
    </el-footer>
  </el-container>
</template>

<script setup>
import { computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const currentRoute = computed(() => route.path)

// 非访客角色自动跳转到对应仪表盘
const roleDashboard = { admin: '/admin/dashboard', security: '/security/entry-check' }
watch(
  () => authStore.userRole,
  (role) => {
    if (role && role !== 'visitor' && role !== 'staff') {
      router.replace(roleDashboard[role] || '/')
    }
  },
  { immediate: true }
)

function handleLogout() {
  authStore.logout()
  router.push('/login')
}
</script>

<style scoped>
.layout-container {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.header {
  height: 64px !important;
  background: #fff;
  border-bottom: 1px solid #e4e7ed;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
  position: sticky;
  top: 0;
  z-index: 100;
}

.header-left {
  display: flex;
  align-items: center;
}

.logo {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #409eff;
}

.logo-text {
  font-size: 18px;
  font-weight: 600;
  white-space: nowrap;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 16px;
  flex: 1;
  justify-content: flex-end;
}

.nav-menu {
  flex: 1;
  justify-content: center;
  border-bottom: none !important;
}

.user-area {
  display: flex;
  align-items: center;
  gap: 8px;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
}

.username {
  font-size: 14px;
  color: #303133;
}

.main-content {
  flex: 1;
  background: #f5f7fa;
  padding: 20px;
}

.footer {
  height: 48px !important;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #909399;
  font-size: 13px;
  background: #fff;
  border-top: 1px solid #e4e7ed;
}
</style>
