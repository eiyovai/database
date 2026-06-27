<template>
  <div class="review-page">
    <h2 class="page-title">预约审核</h2>

    <el-tabs v-model="activeTab">
      <el-tab-pane label="入校预约审核" name="reservation">
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
            <el-table-column label="操作" width="160" fixed="right" align="center">
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
      </el-tab-pane>

      <el-tab-pane label="活动报名审核" name="activity">
        <el-card shadow="never">
          <el-table :data="activityRegs" v-loading="actLoading" stripe>
            <el-table-column prop="activityTitle" label="活动名称" min-width="140" />
            <el-table-column prop="visitorName" label="姓名" width="90" />
            <el-table-column prop="visitorPhone" label="手机号" width="130" />
            <el-table-column label="报名时间" width="160">
              <template #default="{ row }">{{ row.createdAt }}</template>
            </el-table-column>
            <el-table-column label="操作" width="180" align="center">
              <template #default="{ row }">
                <el-button type="success" size="small" @click="handleActApprove(row)">通过</el-button>
                <el-button type="danger" size="small" @click="handleActReject(row)">拒绝</el-button>
              </template>
            </el-table-column>
          </el-table>
          <div v-if="!activityRegs.length && !actLoading" style="text-align:center;padding:40px;color:#999">暂无待审核的活动报名</div>
        </el-card>
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { getReservationList, reviewReservation } from '@/api/reservation'
import { getAllPendingRegistrations, approveRegistration, rejectRegistration } from '@/api/activity'
import { ElMessage, ElMessageBox } from 'element-plus'

const activeTab = ref('reservation')
const list = ref([])
const loading = ref(false)
const page = ref(1)
const pageSize = ref(15)
const total = ref(0)

const activityRegs = ref([])
const actLoading = ref(false)

const statusMap = { pending: '待审核', approved: '已通过', rejected: '已拒绝' }
const statusType = (s) => ({ pending: 'warning', approved: 'success', rejected: 'danger' }[s])
const typeMap = { parent: '家长', alumni: '校友', tourist: '游客', study_group: '研学团', partner: '合作单位' }
const timeSlotMap = { morning: '上午', afternoon: '下午', full_day: '全天' }

async function fetchList() {
  loading.value = true
  try {
    const res = await getReservationList({ page: page.value, pageSize: pageSize.value })
    list.value = res.items || []
    total.value = res.total || 0
  } catch {
    list.value = []
    total.value = 0
  } finally {
    loading.value = false
  }
}

async function fetchActivityRegs() {
  actLoading.value = true
  try {
    const res = await getAllPendingRegistrations()
    activityRegs.value = res.items || res || []
  } catch {
    activityRegs.value = []
  } finally {
    actLoading.value = false
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

async function handleActApprove(row) {
  try {
    await ElMessageBox.confirm(`确定通过 ${row.visitorName} 的活动报名？`, '确认')
    await approveRegistration(row.id)
    ElMessage.success('报名已通过')
    await fetchActivityRegs()
  } catch {}
}

async function handleActReject(row) {
  try {
    await ElMessageBox.confirm(`确定拒绝 ${row.visitorName} 的活动报名？`, '确认')
    await rejectRegistration(row.id)
    ElMessage.success('报名已拒绝')
    await fetchActivityRegs()
  } catch {}
}

watch(activeTab, (tab) => {
  if (tab === 'activity') fetchActivityRegs()
})

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
