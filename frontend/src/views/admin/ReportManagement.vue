<template>
  <div class="report-mgmt">
    <h2 class="page-title">举报管理</h2>

    <el-tabs v-model="activeTab">
      <el-tab-pane label="待审核" name="pending" />
      <el-tab-pane label="已通过" name="approved" />
      <el-tab-pane label="已驳回" name="rejected" />
    </el-tabs>

    <el-card shadow="never">
      <el-table :data="reports" stripe>
        <el-table-column label="举报人" width="100">
          <template #default="{ row }">{{ row.reporter?.name || row.reporter }}</template>
        </el-table-column>
        <el-table-column prop="targetName" label="举报对象" width="120" />
        <el-table-column prop="violationType" label="违规类型" width="120">
          <template #default="{ row }">
            <el-tag size="small">{{ violationTypeMap[row.violationType] }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="location" label="发生地点" width="120" />
        <el-table-column label="发生时间" width="180">
          <template #default="{ row }">{{ row.occurredAt || row.time }}</template>
        </el-table-column>
        <el-table-column prop="description" label="描述" min-width="180" show-overflow-tooltip />
        <el-table-column prop="status" label="状态" width="90">
          <template #default="{ row }">
            <el-tag :type="row.status === 'pending' ? 'warning' : row.status === 'approved' ? 'success' : 'info'" size="small">
              {{ row.status === 'pending' ? '待审核' : row.status === 'approved' ? '已通过' : '已驳回' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="150" fixed="right" align="center">
          <template #default="{ row }">
            <template v-if="row.status === 'pending'">
              <el-button type="success" size="small" @click="handleReview(row, 'approved')">通过</el-button>
              <el-button type="danger" size="small" @click="handleReview(row, 'rejected')">驳回</el-button>
            </template>
            <el-button v-else type="primary" size="small" text>详情</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getReportList, reviewReport } from '@/api/report'
import { ElMessage, ElMessageBox } from 'element-plus'

const activeTab = ref('pending')
const reports = ref([])

const violationTypeMap = {
  trespass: '越权进入', harassment: '骚扰行为', damage: '损坏公物',
  noise: '噪音扰民', other: '其他',
}

async function fetchReports() {
  try {
    const res = await getReportList({ status: activeTab.value })
    reports.value = res.items || res
  } catch {
    reports.value = []
  }
}

async function handleReview(row, action) {
  const text = action === 'approved' ? '通过' : '驳回'
  try {
    await ElMessageBox.confirm(`确定${text}该举报？`, '确认')
    await reviewReport(row.id, { status: action })
    ElMessage.success(`已${text}`)
    await fetchReports()
  } catch {}
}

onMounted(fetchReports)
</script>

<style scoped>
.report-mgmt { max-width: 1400px; }
.page-title { font-size: 20px; margin-bottom: 20px; }
</style>
