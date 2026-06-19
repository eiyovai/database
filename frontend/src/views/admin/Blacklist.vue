<template>
  <div class="blacklist-page">
    <h2 class="page-title">黑名单管理</h2>

    <el-card shadow="never">
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
        <el-table-column label="违规类型" min-width="180">
          <template #default="{ row }">
            <el-tag size="small">{{ row.reason?.substring(0, 10) || '多次违规' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="拉黑时间" width="160">
          <template #default="{ row }">{{ formatDate(row.blacklistedAt) }}</template>
        </el-table-column>
        <el-table-column prop="reason" label="拉黑原因" min-width="160" show-overflow-tooltip />
        <el-table-column label="操作" width="100" fixed="right">
          <template #default="{ row }">
            <el-button type="success" size="small" text @click="handleRemove(row)">移出</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getBlacklist, removeBlacklist } from '@/api/admin'
import { ElMessage, ElMessageBox } from 'element-plus'

const list = ref([])

const violationTypeMap = {
  no_show: '预约爽约', overstay: '超时滞留', trespass: '越权进入',
  duplicate: '恶意重复预约', absence: '活动多次缺席',
}

function formatDate(date) {
  if (!date) return '-'
  return date.includes('T') ? date.split('T')[0] : date
}

async function fetchList() {
  try {
    const res = await getBlacklist()
    list.value = res.items || res
  } catch {
    list.value = [
      { name: '赵六', phone: '136****4321', violationCount: 3, violations: ['no_show', 'overstay'], blacklistedAt: '2026-06-15 14:30', reason: '累计爽约2次+超时滞留1次' },
      { name: '钱七', phone: '135****8765', violationCount: 2, violations: ['trespass'], blacklistedAt: '2026-06-10 09:00', reason: '擅自进入封闭实验室区域' },
    ]
  }
}

async function handleRemove(row) {
  try {
    await ElMessageBox.confirm(`确定将 ${row.name} 移出黑名单？`, '确认')
    await removeBlacklist(row.id)
    ElMessage.success('已移出黑名单')
    await fetchList()
  } catch {}
}

onMounted(fetchList)
</script>

<style scoped>
.blacklist-page { max-width: 1400px; }
.page-title { font-size: 20px; margin-bottom: 20px; }
</style>
