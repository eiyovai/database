<template>
  <div class="blacklist-page">
    <div class="page-header">
      <h2 class="page-title">黑名单管理</h2>
    </div>

    <el-card shadow="never" class="section-card">
      <template #header><span class="card-title">黑名单列表</span></template>
      <el-table :data="list" stripe>
        <el-table-column label="姓名" width="100">
          <template #default="{ row }">{{ row.user?.name || row.name }}</template>
        </el-table-column>
        <el-table-column label="手机号" width="130">
          <template #default="{ row }">{{ row.user?.phone || row.phone }}</template>
        </el-table-column>
        <el-table-column label="违规次数" width="100">
          <template #default="{ row }">{{ row.violationCount }} 次</template>
        </el-table-column>
        <el-table-column label="拉黑时间" width="120">
          <template #default="{ row }">{{ formatDate(row.blacklistedAt) }}</template>
        </el-table-column>
        <el-table-column label="到期时间" width="120">
          <template #default="{ row }">{{ formatDate(row.expiresAt) }}</template>
        </el-table-column>
        <el-table-column prop="reason" label="拉黑原因" min-width="200" show-overflow-tooltip />
        <el-table-column label="操作" width="80" fixed="right" align="center">
          <template #default="{ row }">
            <el-button type="success" size="small" text @click="handleRemove(row)">移出</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-card shadow="never" class="section-card">
      <template #header><span class="card-title">违规记录</span></template>
      <el-table :data="violations" stripe max-height="400">
        <el-table-column label="姓名" width="100">
          <template #default="{ row }">{{ row.userName }}</template>
        </el-table-column>
        <el-table-column label="违规类型" width="100">
          <template #default="{ row }">
            <el-tag :type="severityType(row.severity)" size="small">{{ violationTypeMap[row.violationType] || row.violationType }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="description" label="违规描述" min-width="250" show-overflow-tooltip />
        <el-table-column prop="location" label="地点" width="120" />
        <el-table-column label="来源" width="90">
          <template #default="{ row }">
            <el-tag :type="row.sourceType === 'system' ? 'info' : 'warning'" size="small">
              {{ row.sourceType === 'system' ? '系统检测' : row.sourceType === 'report' ? '举报' : '人工' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="发生时间" width="120">
          <template #default="{ row }">{{ formatDate(row.occurredAt) }}</template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getBlacklist, removeBlacklist, getViolations } from '@/api/admin'
import { ElMessage, ElMessageBox } from 'element-plus'

const list = ref([])
const violations = ref([])

const violationTypeMap = {
  no_show: '预约爽约', overstay: '超时滞留', trespass: '越权进入',
  duplicate: '恶意重复预约', absence: '活动缺席',
}
const severityType = (s) => ({ minor: 'info', major: 'warning', critical: 'danger' }[s] || 'info')

function formatDate(date) {
  if (!date) return '-'
  return date.includes('T') ? date.split('T')[0] : date.substring(0, 10)
}

async function fetchList() {
  try {
    const res = await getBlacklist()
    list.value = res.items || res
  } catch {
    list.value = []
  }
}

async function fetchViolations() {
  try {
    const res = await getViolations()
    violations.value = res.items || res || []
  } catch {
    violations.value = []
  }
}

async function handleRemove(row) {
  try {
    await ElMessageBox.confirm(`确定将 ${row.user?.name || row.name} 移出黑名单？`, '确认')
    await removeBlacklist(row.id)
    ElMessage.success('已移出黑名单')
    await fetchList()
  } catch {}
}

onMounted(() => { fetchList(); fetchViolations() })
</script>

<style scoped>
.blacklist-page { max-width: 1400px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.page-title { font-size: 20px; color: #303133; margin: 0; }
.section-card { margin-bottom: 20px; }
.card-title { font-weight: 600; }
</style>
