<template>
  <div class="report-mgmt">
    <h2 class="page-title">举报管理</h2>

    <el-tabs v-model="activeTab" @tab-change="fetchReports">
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
            <el-button v-else type="primary" size="small" text @click="showDetail(row)">详情</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 举报详情弹窗 -->
    <el-dialog v-model="detailVisible" title="举报详情" width="600px">
      <template v-if="detailRow">
        <el-descriptions :column="2" border>
          <el-descriptions-item label="举报人">{{ detailRow.reporter?.name || '-' }}</el-descriptions-item>
          <el-descriptions-item label="举报对象">{{ detailRow.targetName }}</el-descriptions-item>
          <el-descriptions-item label="违规类型">{{ violationTypeMap[detailRow.violationType] || detailRow.violationType }}</el-descriptions-item>
          <el-descriptions-item label="发生地点">{{ detailRow.location }}</el-descriptions-item>
          <el-descriptions-item label="发生时间">{{ detailRow.occurredAt || detailRow.time || '-' }}</el-descriptions-item>
          <el-descriptions-item label="状态">
            <el-tag :type="detailRow.status === 'pending' ? 'warning' : detailRow.status === 'approved' ? 'success' : 'info'" size="small">
              {{ detailRow.status === 'pending' ? '待审核' : detailRow.status === 'approved' ? '已通过' : '已驳回' }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="描述" :span="2">{{ detailRow.description }}</el-descriptions-item>
          <el-descriptions-item v-if="detailRow.reviewRemark" label="审核备注" :span="2">{{ detailRow.reviewRemark }}</el-descriptions-item>
          <el-descriptions-item v-if="detailRow.reviewedAt" label="审核时间" :span="2">{{ detailRow.reviewedAt }}</el-descriptions-item>
        </el-descriptions>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getReportList, reviewReport } from '@/api/report'
import { ElMessage, ElMessageBox } from 'element-plus'

const activeTab = ref('pending')
const reports = ref([])
const detailVisible = ref(false)
const detailRow = ref(null)

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

function showDetail(row) {
  detailRow.value = row
  detailVisible.value = true
}

onMounted(fetchReports)
</script>

<style scoped>
.report-mgmt { max-width: 1400px; }
.page-title { font-size: 20px; margin-bottom: 20px; }
</style>
