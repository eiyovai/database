<template>
  <el-container class="security-container">
    <!-- 侧边栏 -->
    <el-aside :width="isCollapse ? '64px' : '200px'" class="sidebar">
      <div class="sidebar-header">
        <el-icon :size="24"><Monitor /></el-icon>
        <span v-show="!isCollapse" class="sidebar-title">安保工作站</span>
      </div>

      <el-menu
        :default-active="currentRoute"
        :collapse="isCollapse"
        router
        class="sidebar-menu"
      >
        <el-menu-item index="/security/entry-check">
          <el-icon><Select /></el-icon>
          <template #title>入校核验</template>
        </el-menu-item>
        <el-menu-item index="/security/exit-record">
          <el-icon><CircleCheck /></el-icon>
          <template #title>离校登记</template>
        </el-menu-item>
        <el-menu-item index="/security/current-visitors">
          <el-icon><UserFilled /></el-icon>
          <template #title>在校访客</template>
        </el-menu-item>
        <el-menu-item index="/security/report">
          <el-icon><ChatDotSquare /></el-icon>
          <template #title>违规举报</template>
        </el-menu-item>
      </el-menu>
    </el-aside>

    <!-- 主区域 -->
    <el-container class="main-area">
      <el-header class="security-header">
        <el-button :icon="isCollapse ? 'Expand' : 'Fold'" text @click="toggleCollapse" />
        <div class="header-right">
          <span class="current-time">{{ currentTime }}</span>
          <el-dropdown trigger="click">
            <span class="user-info">
              <el-avatar :size="32" icon="UserFilled" />
              <span>{{ authStore.userName || '安保' }}</span>
            </span>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item @click="handleLogout">退出登录</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </el-header>

      <div class="breadcrumb-area">
        <el-breadcrumb>
          <el-breadcrumb-item :to="'/'">首页</el-breadcrumb-item>
          <el-breadcrumb-item>{{ route.meta?.title || '安保' }}</el-breadcrumb-item>
        </el-breadcrumb>
      </div>

      <el-main class="security-content">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import dayjs from 'dayjs'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const isCollapse = ref(false)
const currentTime = ref(dayjs().format('YYYY-MM-DD HH:mm:ss'))
let timer = null

const currentRoute = computed(() => route.path)

// 非安保角色自动跳转
watch(
  () => authStore.userRole,
  (role) => {
    if (role && role !== 'security') {
      const dashboard = { admin: '/admin/dashboard', visitor: '/visitor/reservation', staff: '/visitor/reservation' }
      router.replace(dashboard[role] || '/')
    }
  },
  { immediate: true }
)

onMounted(() => {
  timer = setInterval(() => {
    currentTime.value = dayjs().format('YYYY-MM-DD HH:mm:ss')
  }, 1000)
})

onUnmounted(() => {
  clearInterval(timer)
})

function toggleCollapse() {
  isCollapse.value = !isCollapse.value
}

function handleLogout() {
  authStore.logout()
  router.push('/login')
}
</script>

<style scoped>
.security-container {
  height: 100vh;
}

.sidebar {
  background: #1d1e1f;
  transition: width 0.3s;
  overflow: hidden;
}

.sidebar-header {
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  color: #ffd700;
  background: rgba(0, 0, 0, 0.2);
}

.sidebar-title {
  font-size: 16px;
  font-weight: 600;
}

.sidebar-menu {
  border-right: none !important;
  background: transparent !important;
}

.sidebar-menu .el-menu-item {
  color: #bfcbd9 !important;
}

.sidebar-menu .el-menu-item:hover {
  background: rgba(255, 255, 255, 0.05) !important;
}

.sidebar-menu .el-menu-item.is-active {
  color: #ffd700 !important;
  background: rgba(255, 215, 0, 0.1) !important;
}

.main-area {
  display: flex;
  flex-direction: column;
}

.security-header {
  height: 50px !important;
  background: #1d1e1f;
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 16px;
}

.current-time {
  font-family: monospace;
  font-size: 16px;
  color: #ffd700;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  color: #fff;
}

.breadcrumb-area {
  padding: 12px 20px;
  background: #fff;
  border-bottom: 1px solid #f2f2f2;
}

.security-content {
  flex: 1;
  background: #f0f2f5;
  padding: 20px;
  overflow-y: auto;
}
</style>
