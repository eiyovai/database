<template>
  <el-container class="admin-container">
    <!-- 侧边栏 -->
    <el-aside :width="isCollapse ? '64px' : '220px'" class="sidebar">
      <div class="sidebar-header">
        <el-icon :size="24"><Management /></el-icon>
        <span v-show="!isCollapse" class="sidebar-title">管理后台</span>
      </div>

      <el-menu
        :default-active="currentRoute"
        :collapse="isCollapse"
        router
        class="sidebar-menu"
      >
        <el-menu-item index="/admin/dashboard">
          <el-icon><DataAnalysis /></el-icon>
          <template #title>数据看板</template>
        </el-menu-item>
        <el-menu-item index="/admin/review">
          <el-icon><Edit /></el-icon>
          <template #title>预约审核</template>
        </el-menu-item>
        <el-menu-item index="/admin/open-rules">
          <el-icon><Setting /></el-icon>
          <template #title>开放规则</template>
        </el-menu-item>
        <el-menu-item index="/admin/areas">
          <el-icon><MapLocation /></el-icon>
          <template #title>区域管理</template>
        </el-menu-item>
        <el-menu-item index="/admin/activities">
          <el-icon><Collection /></el-icon>
          <template #title>活动管理</template>
        </el-menu-item>
        <el-menu-item index="/admin/schedules">
          <el-icon><Timer /></el-icon>
          <template #title>排班管理</template>
        </el-menu-item>
        <el-menu-item index="/admin/blacklist">
          <el-icon><WarningFilled /></el-icon>
          <template #title>黑名单管理</template>
        </el-menu-item>
        <el-menu-item index="/admin/reports">
          <el-icon><ChatDotSquare /></el-icon>
          <template #title>举报管理</template>
        </el-menu-item>
        <el-menu-item index="/admin/violations">
          <el-icon><Warning /></el-icon>
          <template #title>违规记录</template>
        </el-menu-item>
        <el-menu-item index="/admin/audit-logs">
          <el-icon><Document /></el-icon>
          <template #title>审计日志</template>
        </el-menu-item>
      </el-menu>
    </el-aside>

    <!-- 主区域 -->
    <el-container class="main-area">
      <!-- 顶栏 -->
      <el-header class="admin-header">
        <el-button :icon="isCollapse ? 'Expand' : 'Fold'" text @click="toggleCollapse" />
        <div class="header-right">
          <el-badge :value="pendingCount" :hidden="pendingCount === 0" class="badge-item">
            <el-button text circle @click="$router.push('/admin/review')">
              <el-icon><Bell /></el-icon>
            </el-button>
          </el-badge>
          <el-dropdown trigger="click">
            <span class="user-info">
              <el-avatar :size="32" icon="UserFilled" />
              <span>{{ authStore.userName || '管理员' }}</span>
            </span>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item @click="handleLogout">退出登录</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </el-header>

      <!-- 面包屑 -->
      <div class="breadcrumb-area">
        <el-breadcrumb>
          <el-breadcrumb-item :to="'/'">首页</el-breadcrumb-item>
          <el-breadcrumb-item>{{ route.meta?.title || '管理' }}</el-breadcrumb-item>
        </el-breadcrumb>
      </div>

      <!-- 内容 -->
      <el-main class="admin-content">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const isCollapse = ref(false)
const pendingCount = ref(0)

const currentRoute = computed(() => route.path)

// 非管理员角色自动跳转
watch(
  () => authStore.userRole,
  (role) => {
    if (role && role !== 'admin') {
      const dashboard = { security: '/security/entry-check', visitor: '/visitor/reservation', staff: '/visitor/reservation' }
      router.replace(dashboard[role] || '/')
    }
  },
  { immediate: true }
)

function toggleCollapse() {
  isCollapse.value = !isCollapse.value
}

function handleLogout() {
  authStore.logout()
  router.push('/login')
}
</script>

<style scoped>
.admin-container {
  height: 100vh;
}

.sidebar {
  background: #304156;
  transition: width 0.3s;
  overflow: hidden;
}

.sidebar-header {
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  color: #fff;
  background: rgba(0, 0, 0, 0.1);
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
  color: #409eff !important;
  background: rgba(64, 158, 255, 0.1) !important;
}

.main-area {
  display: flex;
  flex-direction: column;
}

.admin-header {
  height: 50px !important;
  background: #fff;
  border-bottom: 1px solid #e4e7ed;
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

.badge-item :deep(.el-badge__content) {
  top: 8px;
  right: 4px;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
}

.breadcrumb-area {
  padding: 12px 20px;
  background: #fff;
  border-bottom: 1px solid #f2f2f2;
}

.admin-content {
  flex: 1;
  background: #f0f2f5;
  padding: 20px;
  overflow-y: auto;
}
</style>
