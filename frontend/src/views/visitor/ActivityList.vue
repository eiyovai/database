<template>
  <div class="activity-page">
    <div class="page-header">
      <h1>校园活动</h1>
      <p>浏览并报名参加校园公开活动</p>
    </div>

    <!-- 筛选 -->
    <el-card shadow="never" class="filter-card">
      <el-row :gutter="16">
        <el-col :span="6">
          <el-input v-model="keyword" placeholder="搜索活动名称" clearable prefix-icon="Search" />
        </el-col>
        <el-col :span="6">
          <el-select v-model="statusFilter" placeholder="活动状态" clearable style="width: 100%">
            <el-option label="报名中" value="open" />
            <el-option label="已结束" value="ended" />
            <el-option label="全部" value="" />
          </el-select>
        </el-col>
      </el-row>
    </el-card>

    <!-- 活动列表 -->
    <el-row :gutter="20">
      <el-col v-for="item in activityList" :key="item.id" :xs="24" :sm="12" :lg="8">
        <el-card shadow="hover" class="activity-card">
          <div class="activity-cover">
            <el-icon :size="48"><Collection /></el-icon>
          </div>
          <div class="activity-body">
            <h3 class="activity-title">{{ item.title }}</h3>
            <div class="activity-meta">
              <span><el-icon><Timer /></el-icon> {{ item.startTime }}</span>
              <span><el-icon><Location /></el-icon> {{ item.location }}</span>
            </div>
            <p class="activity-desc">{{ item.description }}</p>
            <div class="activity-footer">
              <span class="registrations">
                已报名: <strong>{{ item.registered }}/{{ item.maxParticipants }}</strong>
              </span>
              <el-button
                type="primary"
                size="small"
                :disabled="item.registered >= item.maxParticipants"
                @click="handleRegister(item)"
              >
                {{ item.registered >= item.maxParticipants ? '已满' : '立即报名' }}
              </el-button>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 空状态 -->
    <el-empty v-if="!activityList.length" description="暂无活动" />

    <!-- 报名对话框 -->
    <el-dialog v-model="registerDialog" title="确认报名" width="400px">
      <p>确认报名活动「{{ currentActivity?.title }}」？</p>
      <template #footer>
        <el-button @click="registerDialog = false">取消</el-button>
        <el-button type="primary" :loading="registering" @click="confirmRegister">确认报名</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getPublicActivities, registerActivity } from '@/api/activity'
import { ElMessage } from 'element-plus'

const activityList = ref([])
const keyword = ref('')
const statusFilter = ref('')
const registerDialog = ref(false)
const registering = ref(false)
const currentActivity = ref(null)

async function fetchActivities() {
  try {
    const res = await getPublicActivities({
      keyword: keyword.value,
      status: statusFilter.value,
    })
    activityList.value = res.items || res
  } catch {
    // 模拟数据
    activityList.value = [
      {
        id: 1,
        title: '校史馆公益讲解',
        startTime: '2026-06-22 10:00',
        location: '校史馆',
        description: '由专业讲解员带领参观校史馆，了解学校百年发展历程。',
        registered: 25,
        maxParticipants: 50,
      },
      {
        id: 2,
        title: '实验室开放参观',
        startTime: '2026-06-23 14:00',
        location: '理工楼 A 座',
        description: '参观最新科研实验室，体验前沿科技设备。',
        registered: 45,
        maxParticipants: 50,
      },
      {
        id: 3,
        title: '招生宣讲会',
        startTime: '2026-06-25 09:00',
        location: '学术报告厅',
        description: '招生办老师现场解答报考相关问题，了解招生政策。',
        registered: 120,
        maxParticipants: 200,
      },
    ]
  }
}

function handleRegister(item) {
  currentActivity.value = item
  registerDialog.value = true
}

async function confirmRegister() {
  registering.value = true
  try {
    await registerActivity({ activityId: currentActivity.value.id })
    ElMessage.success('报名成功！')
    registerDialog.value = false
    await fetchActivities()
  } catch {
    // handled
  } finally {
    registering.value = false
  }
}

onMounted(fetchActivities)
</script>

<style scoped>
.activity-page {
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  margin-bottom: 20px;
}

.page-header h1 {
  font-size: 24px;
  color: #303133;
}

.page-header p {
  color: #909399;
  font-size: 14px;
  margin-top: 4px;
}

.filter-card {
  margin-bottom: 20px;
}

.activity-card {
  margin-bottom: 20px;
  cursor: pointer;
}

.activity-cover {
  height: 140px;
  background: linear-gradient(135deg, #e8f4fd 0%, #d3eaff 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #409eff;
}

.activity-body {
  padding: 16px 0;
}

.activity-title {
  font-size: 16px;
  color: #303133;
  margin-bottom: 8px;
}

.activity-meta {
  display: flex;
  gap: 16px;
  font-size: 13px;
  color: #909399;
  margin-bottom: 8px;
}

.activity-meta span {
  display: flex;
  align-items: center;
  gap: 4px;
}

.activity-desc {
  font-size: 13px;
  color: #606266;
  line-height: 1.6;
  margin-bottom: 12px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.activity-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.registrations {
  font-size: 13px;
  color: #909399;
}
</style>
