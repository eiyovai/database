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

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getBlacklist, removeBlacklist } from '@/api/admin'
import { ElMessage, ElMessageBox } from 'element-plus'

const list = ref([])

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

async function handleRemove(row) {
  try {
    await ElMessageBox.confirm(`确定将 ${row.user?.name || row.name} 移出黑名单？`, '确认')
    await removeBlacklist(row.id)
    ElMessage.success('已移出黑名单')
    await fetchList()
  } catch {}
}

onMounted(() => { fetchList() })
</script>

<style scoped>
.blacklist-page { max-width: 1400px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.page-title { font-size: 20px; color: #303133; margin: 0; }
.section-card { margin-bottom: 20px; }
.card-title { font-weight: 600; }
</style>
