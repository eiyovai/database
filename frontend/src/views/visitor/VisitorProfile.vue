<template>
  <div class="profile-page">
    <el-row :gutter="24">
      <el-col :span="8">
        <el-card shadow="never">
          <div class="user-card">
            <el-avatar :size="80" icon="UserFilled" />
            <h3>{{ authStore.userName || '用户' }}</h3>
            <span class="role-tag">
              <el-tag type="primary">访客</el-tag>
            </span>
          </div>
        </el-card>

        <el-card shadow="never" class="stats-card">
          <template #header><span>预约统计</span></template>
          <el-row :gutter="12">
            <el-col :span="8" class="stat-item">
              <div class="stat-value">12</div>
              <div class="stat-label">总预约</div>
            </el-col>
            <el-col :span="8" class="stat-item">
              <div class="stat-value">8</div>
              <div class="stat-label">已入校</div>
            </el-col>
            <el-col :span="8" class="stat-item">
              <div class="stat-value">0</div>
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
              <el-input :model-value="authStore.userInfo?.phone || '138****0001'" disabled />
            </el-form-item>
            <el-form-item label="注册时间">
              <el-input value="2026-01-15" disabled />
            </el-form-item>
            <el-form-item label="账户类型">
              <el-tag type="primary">访客</el-tag>
            </el-form-item>
          </el-form>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
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
