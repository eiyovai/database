<template>
  <div class="my-reservations-page">
    <div class="page-header">
      <h1>我的预约</h1>
      <p>查看和管理您的入校预约记录</p>
    </div>

    <!-- 状态标签页 -->
    <el-tabs v-model="activeTab" @tab-change="fetchReservations">
      <el-tab-pane label="全部" name="all" />
      <el-tab-pane label="待审核" name="pending" />
      <el-tab-pane label="已通过" name="approved" />
      <el-tab-pane label="已入校" name="checked_in" />
      <el-tab-pane label="已拒绝/已取消" name="closed" />
    </el-tabs>

    <!-- 预约列表 -->
    <el-table :data="list" v-loading="loading" stripe style="width: 100%">
      <el-table-column prop="id" label="预约编号" width="100" />
      <el-table-column prop="visitDate" label="参观日期" width="120" />
      <el-table-column prop="timeSlot" label="时段" width="120">
        <template #default="{ row }">
          {{ timeSlotMap[row.timeSlot] || row.timeSlot }}
        </template>
      </el-table-column>
      <el-table-column prop="visitorType" label="访客类型" width="100">
        <template #default="{ row }">
          {{ visitorTypeMap[row.visitorType] || row.visitorType }}
        </template>
      </el-table-column>
      <el-table-column prop="companions" label="同行人数" width="90" />
      <el-table-column prop="purpose" label="事由" min-width="150" show-overflow-tooltip />
      <el-table-column prop="status" label="状态" width="100">
        <template #default="{ row }">
          <el-tag :type="statusType(row.status)" effect="plain">
            {{ statusMap[row.status] || row.status }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="120" fixed="right">
        <template #default="{ row }">
          <el-button
            v-if="row.status === 'pending'"
            type="danger"
            size="small"
            @click="handleCancel(row)"
          >
            取消预约
          </el-button>
          <el-button v-else type="primary" size="small" text @click="showDetail(row)">
            详情
          </el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 分页 -->
    <div class="pagination-wrap">
      <el-pagination
        v-model:current-page="page"
        v-model:page-size="pageSize"
        :total="total"
        layout="total, prev, pager, next"
        @current-change="fetchReservations"
      />
    </div>

    <!-- 空状态 -->
    <el-empty v-if="!loading && !list.length" description="暂无预约记录" />

    <!-- 详情对话框 -->
    <el-dialog v-model="detailVisible" title="预约详情" width="500px">
      <el-descriptions v-if="detail" :column="2" border>
        <el-descriptions-item label="预约编号">{{ detail.id }}</el-descriptions-item>
        <el-descriptions-item label="访客类型">{{ visitorTypeMap[detail.visitorType] }}</el-descriptions-item>
        <el-descriptions-item label="入校日期">{{ detail.visitDate }}</el-descriptions-item>
        <el-descriptions-item label="时段">{{ timeSlotMap[detail.timeSlot] }}</el-descriptions-item>
        <el-descriptions-item label="同行人数">{{ detail.companions }}</el-descriptions-item>
        <el-descriptions-item label="状态">
          <el-tag :type="statusType(detail.status)" size="small">{{ statusMap[detail.status] }}</el-tag>
        </el-descriptions-item>
        <el-descriptions-item label="入校事由" :span="2">{{ detail.purpose }}</el-descriptions-item>
        <el-descriptions-item label="入校时间" v-if="detail.entryTime">{{ detail.entryTime }}</el-descriptions-item>
        <el-descriptions-item label="离校时间" v-if="detail.exitTime">{{ detail.exitTime }}</el-descriptions-item>
        <el-descriptions-item label="入校校门" v-if="detail.entryGate">{{ detail.entryGate }}</el-descriptions-item>
        <el-descriptions-item label="创建时间" :span="2">{{ detail.createdAt }}</el-descriptions-item>
      </el-descriptions>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getMyReservations, cancelReservation, getReservationDetail } from '@/api/reservation'
import { ElMessage, ElMessageBox } from 'element-plus'

const activeTab = ref('all')
const list = ref([])
const loading = ref(false)
const page = ref(1)
const pageSize = ref(10)
const total = ref(0)
const detailVisible = ref(false)
const detail = ref(null)

const statusMap = {
  pending: '待审核',
  approved: '已通过',
  rejected: '已拒绝',
  cancelled: '已取消',
  checked_in: '已入校',
  checked_out: '已离校',
  no_show: '已爽约',
}

const statusType = (s) => ({
  pending: 'warning',
  approved: 'success',
  rejected: 'danger',
  cancelled: 'info',
  checked_in: 'primary',
  checked_out: '',
  no_show: 'danger',
}[s] || 'info')

const visitorTypeMap = {
  parent: '家长',
  alumni: '校友',
  tourist: '普通游客',
  study_group: '研学团队',
  partner: '合作单位',
}

const timeSlotMap = {
  morning: '08:00-12:00',
  afternoon: '12:00-17:00',
  full_day: '08:00-17:00',
}

async function fetchReservations() {
  loading.value = true
  try {
    const params = {
      page: page.value,
      pageSize: pageSize.value,
    }
    if (activeTab.value !== 'all') {
      params.status = activeTab.value
    }
    const res = await getMyReservations(params)
    list.value = res.items || res
    total.value = res.total || res.length || 0
  } catch {
    // 模拟数据
    list.value = [
      { id: 'R20260601', visitDate: '2026-06-22', timeSlot: 'morning', visitorType: 'tourist', companions: 2, purpose: '带孩子参观校园', status: 'approved', createdAt: '2026-06-19 10:30' },
      { id: 'R20260602', visitDate: '2026-06-25', timeSlot: 'afternoon', visitorType: 'alumni', companions: 0, purpose: '回母校看看', status: 'pending', createdAt: '2026-06-19 11:00' },
    ]
    total.value = list.value.length
  } finally {
    loading.value = false
  }
}

async function handleCancel(row) {
  try {
    await ElMessageBox.confirm('确定要取消该预约吗？', '确认取消')
    await cancelReservation(row.id)
    ElMessage.success('预约已取消')
    await fetchReservations()
  } catch {
    // cancelled
  }
}

async function showDetail(row) {
  try {
    const res = await getReservationDetail(row.id)
    detail.value = res
  } catch {
    detail.value = row
  }
  detailVisible.value = true
}

onMounted(fetchReservations)
</script>

<style scoped>
.my-reservations-page {
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

.pagination-wrap {
  margin-top: 20px;
  display: flex;
  justify-content: center;
}
</style>
