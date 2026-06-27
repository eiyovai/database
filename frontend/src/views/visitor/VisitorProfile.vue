<template>
  <div class="profile-page">
    <el-row :gutter="24">
      <el-col :span="8">
        <el-card shadow="never">
          <div class="user-card">
            <el-avatar :size="80" icon="UserFilled" />
            <h3>{{ authStore.userName || '用户' }}</h3>
            <span class="role-tag">
              <el-tag type="primary">{{ roleMap[authStore.userRole] || '访客' }}</el-tag>
            </span>
          </div>
        </el-card>

        <el-card shadow="never" class="stats-card">
          <template #header><span>预约统计</span></template>
          <el-row :gutter="12">
            <el-col :span="8" class="stat-item">
              <div class="stat-value">{{ stats.totalReservations }}</div>
              <div class="stat-label">总预约</div>
            </el-col>
            <el-col :span="8" class="stat-item">
              <div class="stat-value">{{ stats.checkedIn }}</div>
              <div class="stat-label">已入校</div>
            </el-col>
            <el-col :span="8" class="stat-item">
              <div class="stat-value">{{ stats.noShow }}</div>
              <div class="stat-label">爽约</div>
            </el-col>
          </el-row>
        </el-card>
      </el-col>

      <el-col :span="16">
        <el-card shadow="never">
          <template #header><span>个人资料</span></template>
          <el-form label-width="100px" size="large">
            <el-form-item label="姓名">
              <el-input :model-value="authStore.userName" disabled />
            </el-form-item>
            <el-form-item label="手机号">
              <el-input :model-value="authStore.userInfo?.phone || ''" disabled />
            </el-form-item>
            <el-form-item label="注册时间">
              <el-input :model-value="profile.createdAt || ''" disabled />
            </el-form-item>
            <el-form-item label="账户类型">
              <el-tag type="primary">{{ roleMap[authStore.userRole] || '访客' }}</el-tag>
            </el-form-item>
          </el-form>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { getMyReservations } from '@/api/reservation'

const authStore = useAuthStore()
const profile = ref({ createdAt: '' })
const stats = ref({ totalReservations: 0, checkedIn: 0, noShow: 0 })
const roleMap = { visitor: '访客', admin: '管理员', security: '安保人员', staff: '工作人员' }

async function fetchStats() {
  try {
    const res = await getMyReservations({ page: 1, pageSize: 100 })
    const items = res.items || []
    stats.value.totalReservations = items.length
    stats.value.checkedIn = items.filter(r => r.status === 'checked_in' || r.status === 'checked_out').length
    stats.value.noShow = items.filter(r => r.status === 'no_show').length
  } catch { /* 静默失败 */ }
}

async function fetchProfile() {
  try {
    const res = await authStore.fetchUserInfo()
    if (authStore.userInfo) {
      profile.value.createdAt = authStore.userInfo.createdAt || ''
    }
  } catch { /* 静默失败 */ }
}

onMounted(() => {
  fetchStats()
  fetchProfile()
})
</script>

<style scoped>
.profile-page {
  max-width: 1200px;
  margin: 0 auto;
}

.user-card {
  text-align: center;
  padding: 20px 0;
}

.user-card h3 {
  margin: 12px 0 8px;
  font-size: 18px;
}

.stats-card {
  margin-top: 16px;
}

.stat-item {
  text-align: center;
}

.stat-value {
  font-size: 24px;
  font-weight: 600;
  color: #409eff;
}

.stat-label {
  font-size: 13px;
  color: #909399;
  margin-top: 4px;
}
</style>
