<template>
  <div class="review-page">
    <h2 class="page-title">预约审核</h2>

    <el-card shadow="never">
      <el-table :data="list" v-loading="loading" stripe>
        <el-table-column prop="reservationNo" label="编号" width="110" />
        <el-table-column prop="visitorName" label="姓名" width="90" />
        <el-table-column prop="visitorPhone" label="手机号" width="130" />
        <el-table-column prop="visitorType" label="访客类型" width="100">
          <template #default="{ row }">
            {{ typeMap[row.visitorType] }}
          </template>
        </el-table-column>
        <el-table-column prop="visitDate" label="参观日期" width="110" />
        <el-table-column prop="timeSlot" label="时段" width="100">
          <template #default="{ row }">{{ timeSlotMap[row.timeSlot] }}</template>
        </el-table-column>
        <el-table-column prop="companions" label="同行" width="70" />
        <el-table-column prop="purpose" label="事由" min-width="140" show-overflow-tooltip />
        <el-table-column prop="status" label="状态" width="90">
          <template #default="{ row }">
            <el-tag :type="statusType(row.status)" size="small">{{ statusMap[row.status] }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <template v-if="row.status === 'pending'">
              <el-button type="success" size="small" @click="handleReview(row, 'approved')">通过</el-button>
              <el-button type="danger" size="small" @click="handleReview(row, 'rejected')">拒绝</el-button>
            </template>
            <el-button v-else type="primary" size="small" text>详情</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-wrap">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          :total="total"
          layout="total, prev, pager, next"
        />
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getReservationList, reviewReservation } from '@/api/reservation'
import { ElMessage, ElMessageBox } from 'element-plus'

const list = ref([])
const loading = ref(false)
const page = ref(1)
const pageSize = ref(15)
const total = ref(0)

const statusMap = { pending: '待审核', approved: '已通过', rejected: '已拒绝' }
const statusType = (s) => ({ pending: 'warning', approved: 'success', rejected: 'danger' }[s])
const typeMap = { parent: '家长', alumni: '校友', tourist: '游客', study_group: '研学团', partner: '合作单位' }
const timeSlotMap = { morning: '上午', afternoon: '下午', full_day: '全天' }

async function fetchList() {
  loading.value = true
  try {
    const res = await getReservationList({ page: page.value, pageSize: pageSize.value })
    list.value = res.items || res
    total.value = res.total || list.value.length
  } catch {
    list.value = [
      { id: 'R001', name: '张三', phone: '138****1234', visitorType: 'tourist', visitDate: '2026-06-22', timeSlot: 'morning', companions: 2, purpose: '带孩子参观校园', status: 'pending' },
      { id: 'R002', name: '李四', phone: '139****5678', visitorType: 'alumni', visitDate: '2026-06-23', timeSlot: 'afternoon', companions: 0, purpose: '回母校参观', status: 'pending' },
      { id: 'R003', name: '王五', phone: '137****9012', visitorType: 'study_group', visitDate: '2026-06-24', timeSlot: 'full_day', companions: 15, purpose: '研学团队参观实验室', status: 'approved' },
    ]
    total.value = list.value.length
  } finally {
    loading.value = false
  }
}

async function handleReview(row, action) {
  const actionText = action === 'approved' ? '通过' : '拒绝'
  try {
    await ElMessageBox.confirm(`确定${actionText}该预约申请吗？`, '确认')
    await reviewReservation(row.id, { status: action })
    ElMessage.success(`已${actionText}`)
    await fetchList()
  } catch {}
}

onMounted(fetchList)
</script>

<style scoped>
.review-page {
  max-width: 1400px;
}

.page-title {
  font-size: 20px;
  color: #303133;
  margin-bottom: 20px;
}

.pagination-wrap {
  margin-top: 16px;
  display: flex;
  justify-content: center;
}
</style>
