<template>
  <div class="current-visitors-page">
    <el-row :gutter="16" class="summary-cards">
      <el-col :span="8">
        <el-card shadow="never">
          <div class="summary-item">
            <div class="num primary">{{ stats.currentVisitors }}</div>
            <div class="label">当前在校访客</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="never">
          <div class="summary-item">
            <div class="num success">{{ stats.todayEntries }}</div>
            <div class="label">今日累计入校</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="never">
          <div class="summary-item">
            <div class="num warning">{{ stats.maxCapacity }}</div>
            <div class="label">今日容量上限</div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-card shadow="never">
      <template #header>
        <el-input v-model="keyword" placeholder="搜索姓名或手机号" clearable prefix-icon="Search" style="width: 300px" />
      </template>

      <el-table :data="visitors" stripe>
        <el-table-column label="姓名" width="100">
          <template #default="{ row }">{{ row.reservation?.visitorName || row.visitorName }}</template>
        </el-table-column>
        <el-table-column label="手机号" width="130">
          <template #default="{ row }">{{ row.reservation?.visitorPhone || row.visitorPhone }}</template>
        </el-table-column>
        <el-table-column label="访客类型" width="100">
          <template #default="{ row }">{{ typeMap[row.reservation?.visitorType] || typeMap[row.visitorType] }}</template>
        </el-table-column>
        <el-table-column label="入校时间" width="160">
          <template #default="{ row }">{{ row.entryTime }}</template>
        </el-table-column>
        <el-table-column label="入校校门" width="100">
          <template #default="{ row }">{{ row.entryGate?.name || row.entryGate }}</template>
        </el-table-column>
        <el-table-column label="预计离校" width="120">
          <template #default="{ row }">{{ row.reservation?.stayDuration || '-' }}</template>
        </el-table-column>
        <el-table-column label="停留时长" width="100">
          <template #default="{ row }">
            {{ row.entryTime ? calcDuration(row.entryTime) : '-' }}
          </template>
        </el-table-column>
        <el-table-column label="状态" width="80">
          <template #default>
            <el-tag type="success" size="small">在校</el-tag>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { getCurrentVisitors } from '@/api/entryexit'
import request from '@/api/request'

const keyword = ref('')
const visitors = ref([])
const stats = ref({ currentVisitors: 0, todayEntries: 0, maxCapacity: 0 })

const typeMap = { parent: '家长', alumni: '校友', tourist: '游客', study_group: '研学团', partner: '合作单位' }

function calcDuration(entryTime) {
  if (!entryTime) return '-'
  const diff = Date.now() - new Date(entryTime).getTime()
  const hours = Math.floor(diff / 3600000)
  const mins = Math.floor((diff % 3600000) / 60000)
  return hours > 0 ? `${hours}h ${mins}min` : `${mins}min`
}

async function fetchStats() {
  try {
    const [campus, entryStats] = await Promise.all([
      request.get('/public/campus-status').catch(() => ({})),
      request.get('/entry-exit/stats').catch(() => ({}))
    ])
    stats.value.currentVisitors = entryStats.currentVisitors || campus.currentVisitors || 0
    stats.value.maxCapacity = campus.maxCapacity || 500
    stats.value.todayEntries = entryStats.todayEntries || 0
  } catch { /* 静默失败 */ }
}

async function fetchVisitors() {
  try {
    const res = await getCurrentVisitors()
    const items = Array.isArray(res) ? res : (res.items || [])
    visitors.value = items
  } catch {
    visitors.value = []
  }
}

watch(keyword, () => {
  if (!keyword.value.trim()) {
    fetchVisitors()
  } else {
    visitors.value = visitors.value.filter(v => {
      const name = v.reservation?.visitorName || v.visitorName || ''
      const phone = v.reservation?.visitorPhone || v.visitorPhone || ''
      return name.includes(keyword.value) || phone.includes(keyword.value)
    })
  }
})

onMounted(() => {
  fetchVisitors()
  fetchStats()
})
</script>

<style scoped>
.current-visitors-page { max-width: 1400px; }
.summary-cards { margin-bottom: 20px; }
.summary-item { text-align: center; padding: 8px 0; }
.summary-item .num { font-size: 32px; font-weight: 700; }
.summary-item .num.primary { color: #409eff; }
.summary-item .num.success { color: #67c23a; }
.summary-item .num.warning { color: #e6a23c; }
.summary-item .label { font-size: 14px; color: #909399; margin-top: 4px; }
</style>
