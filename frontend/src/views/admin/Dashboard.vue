<template>
  <div class="dashboard">
    <h2 class="page-title">数据看板</h2>

    <!-- 统计卡片 -->
    <el-row :gutter="16" class="stat-cards">
      <el-col :span="6">
        <el-card shadow="never" class="stat-card">
          <div class="stat-card-body">
            <div class="stat-info">
              <div class="stat-num">{{ stats.todayReservations }}</div>
              <div class="stat-desc">今日预约人数</div>
            </div>
            <el-icon :size="48" color="#409eff"><UserFilled /></el-icon>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="never" class="stat-card">
          <div class="stat-card-body">
            <div class="stat-info">
              <div class="stat-num">{{ stats.currentVisitors }}</div>
              <div class="stat-desc">当前在校访客</div>
            </div>
            <el-icon :size="48" color="#67c23a"><Check /></el-icon>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="never" class="stat-card">
          <div class="stat-card-body">
            <div class="stat-info">
              <div class="stat-num">{{ stats.pendingReviews }}</div>
              <div class="stat-desc">待审核预约</div>
            </div>
            <el-icon :size="48" color="#e6a23c"><WarningFilled /></el-icon>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="never" class="stat-card">
          <div class="stat-card-body">
            <div class="stat-info">
              <div class="stat-num">{{ stats.blacklistCount }}</div>
              <div class="stat-desc">黑名单人数</div>
            </div>
            <el-icon :size="48" color="#f56c6c"><WarningFilled /></el-icon>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 图表区域 -->
    <el-row :gutter="16">
      <el-col :span="16">
        <el-card shadow="never">
          <template #header>本周客流趋势</template>
          <div class="chart-placeholder">
            <p>近7日预约/到访人数趋势图（待集成 ECharts）</p>
          </div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="never">
          <template #header>访客类型分布</template>
          <div class="chart-placeholder">
            <p>访客类型饼图（待集成 ECharts）</p>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 热门数据 -->
    <el-row :gutter="16" style="margin-top: 16px">
      <el-col :span="12">
        <el-card shadow="never">
          <template #header>系统概览</template>
          <div style="padding: 20px; text-align: center; color: #909399;">
            <p>今日预约 {{ stats.todayReservations }} 人 | 在校 {{ stats.currentVisitors }} 人</p>
            <p>待审核 {{ stats.pendingReviews }} 条 | 黑名单 {{ stats.blacklistCount }} 人</p>
          </div>
        </el-card>
      </el-col>
      <el-col :span="12">
        <el-card shadow="never">
          <template #header>快捷操作</template>
          <div style="padding: 20px; text-align: center;">
            <el-button type="primary" @click="$router.push('/admin/review')">预约审核</el-button>
            <el-button type="success" @click="$router.push('/admin/areas')">区域管理</el-button>
            <el-button type="warning" @click="$router.push('/admin/open-rules')">开放规则</el-button>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getDashboardStats } from '@/api/admin'

const stats = ref({
  todayReservations: 0,
  currentVisitors: 0,
  pendingReviews: 0,
  blacklistCount: 0,
})

async function fetchStats() {
  try {
    const res = await getDashboardStats()
    stats.value = {
      todayReservations: res.todayReservations || 0,
      currentVisitors: res.currentVisitors || 0,
      pendingReviews: res.pendingReviews || 0,
      blacklistCount: res.blacklistCount || 0,
    }
  } catch {
    // 保持默认 0 值
  }
}

onMounted(fetchStats)
</script>

<style scoped>
.dashboard {
  max-width: 1400px;
}

.page-title {
  font-size: 20px;
  color: #303133;
  margin-bottom: 20px;
}

.stat-cards {
  margin-bottom: 16px;
}

.stat-card :deep(.el-card__body) {
  padding: 20px;
}

.stat-card-body {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.stat-num {
  font-size: 32px;
  font-weight: 700;
  color: #303133;
}

.stat-desc {
  font-size: 14px;
  color: #909399;
  margin-top: 4px;
}

.chart-placeholder {
  height: 280px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #909399;
  background: #fafafa;
  border-radius: 4px;
}
</style>
