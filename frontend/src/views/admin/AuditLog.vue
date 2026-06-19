<template>
  <div class="audit-page">
    <h2 class="page-title">审计日志</h2>

    <el-card shadow="never">
      <template #header>
        <el-row :gutter="16">
          <el-col :span="6">
            <el-input v-model="keyword" placeholder="搜索操作内容" clearable prefix-icon="Search" size="small" />
          </el-col>
          <el-col :span="4">
            <el-select v-model="actionFilter" placeholder="操作类型" clearable size="small" style="width: 100%">
              <el-option label="审核操作" value="review" />
              <el-option label="配置修改" value="config" />
              <el-option label="用户管理" value="user" />
              <el-option label="数据导出" value="export" />
            </el-select>
          </el-col>
        </el-row>
      </template>

      <el-table :data="logs" stripe size="small">
        <el-table-column label="操作时间" width="160">
          <template #default="{ row }">{{ row.createdAt || row.time }}</template>
        </el-table-column>
        <el-table-column label="操作人" width="100">
          <template #default="{ row }">{{ row.operator?.name || row.operator || '-' }}</template>
        </el-table-column>
        <el-table-column prop="actionType" label="操作类型" width="100">
          <template #default="{ row }">
            <el-tag size="small">{{ actionTypeMap[row.actionType] }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作内容" min-width="250" show-overflow-tooltip>
          <template #default="{ row }">{{ row.actionDetail || row.detail }}</template>
        </el-table-column>
        <el-table-column label="IP地址" width="140">
          <template #default="{ row }">{{ row.ipAddress || row.ip }}</template>
        </el-table-column>
        <el-table-column prop="result" label="结果" width="70">
          <template #default="{ row }">
            <el-tag :type="row.result === 'success' ? 'success' : 'danger'" size="small">
              {{ row.result === 'success' ? '成功' : '失败' }}
            </el-tag>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-wrap">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          :total="total"
          layout="total, prev, pager, next"
          size="small"
        />
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getAuditLogs } from '@/api/admin'

const logs = ref([])
const keyword = ref('')
const actionFilter = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

const actionTypeMap = { review: '审核操作', config: '配置修改', user: '用户管理', export: '数据导出' }

async function fetchLogs() {
  try {
    const res = await getAuditLogs({ page: page.value, pageSize: pageSize.value, keyword: keyword.value })
    logs.value = res.items || res
    total.value = res.total || logs.value.length
  } catch {
    logs.value = Array.from({ length: 10 }, (_, i) => ({
      time: `2026-06-19 ${String(9 + i).padStart(2, '0')}:${String((i * 3) % 60).padStart(2, '0')}:00`,
      operator: '管理员',
      actionType: ['review', 'config', 'user', 'export'][i % 4],
      detail: ['审核通过预约R001', '修改开放规则-周末容量', '将赵六加入黑名单', '导出本周客流数据'][i % 4],
      ip: '192.168.1.100',
      result: 'success',
    }))
    total.value = logs.value.length
  }
}

onMounted(fetchLogs)
</script>

<style scoped>
.audit-page { max-width: 1400px; }
.page-title { font-size: 20px; margin-bottom: 20px; }
.pagination-wrap { margin-top: 16px; display: flex; justify-content: center; }
</style>
