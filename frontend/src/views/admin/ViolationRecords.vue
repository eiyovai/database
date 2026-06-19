<template>
  <div class="violation-page">
    <h2 class="page-title">违规记录</h2>

    <el-card shadow="never">
      <template #header>
        <div class="filter-bar">
          <el-radio-group v-model="sourceFilter" @change="fetchList">
            <el-radio-button value="">全部来源</el-radio-button>
            <el-radio-button value="report">举报审核</el-radio-button>
            <el-radio-button value="system">系统记录</el-radio-button>
            <el-radio-button value="manual">手动添加</el-radio-button>
          </el-radio-group>
        </div>
      </template>

      <el-table :data="list" stripe>
        <el-table-column label="姓名" width="100">
          <template #default="{ row }">{{ row.user?.name || row.name }}</template>
        </el-table-column>
        <el-table-column prop="violationType" label="违规类型" width="120">
          <template #default="{ row }">
            <el-tag size="small">{{ violationTypeMap[row.violationType] || row.violationType }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="severity" label="严重程度" width="100">
          <template #default="{ row }">
            <el-tag :type="severityType(row.severity)" size="small">
              {{ severityMap[row.severity] || row.severity }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="sourceType" label="来源" width="100">
          <template #default="{ row }">
            <el-tag :type="row.sourceType === 'report' ? 'warning' : 'info'" size="small">
              {{ sourceMap[row.sourceType] || row.sourceType }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip />
        <el-table-column prop="location" label="发生地点" width="120" show-overflow-tooltip />
        <el-table-column label="发生时间" width="160">
          <template #default="{ row }">{{ formatDate(row.occurredAt) }}</template>
        </el-table-column>
        <el-table-column label="记录时间" width="160">
          <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getViolationList } from '@/api/admin'

const list = ref([])
const sourceFilter = ref('')

const violationTypeMap = {
  trespass: '越权进入',
  damage: '损坏公物',
  disturbance: '扰序行为',
  overstay: '超时滞留',
  no_show: '预约爽约',
  absence: '活动缺席',
  duplicate: '重复预约',
  other: '其他',
}

const severityMap = {
  minor: '轻微',
  major: '严重',
  critical: '非常严重',
}

const sourceMap = {
  report: '举报审核',
  system: '系统记录',
  manual: '手动添加',
}

function severityType(severity) {
  return severity === 'critical' ? 'danger' : severity === 'major' ? 'warning' : 'info'
}

function formatDate(date) {
  if (!date) return '-'
  return date.includes('T') ? date.replace('T', ' ').substring(0, 16) : date
}

async function fetchList() {
  try {
    const params = sourceFilter.value ? { source: sourceFilter.value } : {}
    const res = await getViolationList(params)
    list.value = res.items || res
  } catch {
    list.value = []
  }
}

onMounted(fetchList)
</script>

<style scoped>
.violation-page { max-width: 1400px; }
.page-title { font-size: 20px; margin-bottom: 20px; }
.filter-bar { display: flex; align-items: center; gap: 12px; }
</style>
