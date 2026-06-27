<template>
  <div class="entry-check-page">
    <el-row :gutter="24">
      <el-col :span="14">
        <el-card shadow="never">
          <template #header>
            <div class="card-header">
              <span><el-icon><Select /></el-icon> 入校核验</span>
            </div>
          </template>

          <el-form label-position="top" size="large">
            <el-form-item label="预约编号 / 身份证号 / 手机号">
              <el-input
                v-model="query"
                placeholder="输入预约编号、身份证号或手机号搜索"
                size="large"
                clearable
                @keyup.enter="handleSearch"
              >
                <template #prefix>
                  <el-icon><Search /></el-icon>
                </template>
              </el-input>
            </el-form-item>
            <el-button type="primary" size="large" :loading="searching" @click="handleSearch">
              查询核验
            </el-button>
          </el-form>

          <!-- 核验结果 -->
          <div v-if="searchResult" class="check-result">
            <el-divider />
            <el-descriptions :column="2" border size="small">
              <el-descriptions-item label="姓名">{{ searchResult.visitorName }}</el-descriptions-item>
              <el-descriptions-item label="访客类型">{{ typeMap[searchResult.visitorType] }}</el-descriptions-item>
              <el-descriptions-item label="入校日期">{{ searchResult.visitDate?.split('T')[0] }}</el-descriptions-item>
              <el-descriptions-item label="时段">{{ timeSlotMap[searchResult.timeSlot] }}</el-descriptions-item>
              <el-descriptions-item label="同行人数">{{ searchResult.companions }}</el-descriptions-item>
              <el-descriptions-item label="状态">
                <el-tag :type="statusType(searchResult.status)" size="small">
                  {{ statusMap[searchResult.status] }}
                </el-tag>
              </el-descriptions-item>
            </el-descriptions>

            <div class="result-actions">
              <el-button
                v-if="searchResult.status === 'approved'"
                type="success"
                size="large"
                :loading="checking"
                @click="handleEntry"
              >
                <el-icon><Check /></el-icon> 确认入校
              </el-button>
              <el-tag v-else type="danger" size="large" effect="dark">
                该预约当前不可入校
              </el-tag>
            </div>
          </div>
        </el-card>
      </el-col>

      <el-col :span="10">
        <el-card shadow="never">
          <template #header>今日核验统计</template>
          <div class="today-stats">
            <div class="stats-row">
              <span>今日已核验入校</span>
              <span class="num success">{{ stats.todayEntries }}</span>
            </div>
            <div class="stats-row">
              <span>今日已离校</span>
              <span class="num">{{ stats.todayExits }}</span>
            </div>
            <div class="stats-row">
              <span>当前在校</span>
              <span class="num primary">{{ stats.currentVisitors }}</span>
            </div>
          </div>
        </el-card>

        <el-card shadow="never" style="margin-top: 16px">
          <template #header>最近入校记录</template>
          <el-timeline>
            <el-timeline-item v-for="r in recentEntries" :key="r.time" :timestamp="r.time" placement="top">
              <p>{{ r.name }} - {{ r.gate }}</p>
            </el-timeline-item>
          </el-timeline>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { searchReservation, confirmEntry } from '@/api/entryexit'
import request from '@/api/request'
import { ElMessage } from 'element-plus'

const query = ref('')
const searching = ref(false)
const checking = ref(false)
const searchResult = ref(null)

const typeMap = { parent: '家长', alumni: '校友', tourist: '游客', study_group: '研学团', partner: '合作单位' }
const timeSlotMap = { morning: '上午', afternoon: '下午', full_day: '全天' }
const statusMap = { pending: '待审核', approved: '已通过', rejected: '已拒绝', cancelled: '已取消', checked_in: '已入校', checked_out: '已离校' }
const statusType = (s) => ({ pending: 'warning', approved: 'success', rejected: 'danger', checked_in: 'primary', checked_out: 'info' }[s] || 'info')

const recentEntries = ref([])
const stats = ref({ todayEntries: 0, todayExits: 0, currentVisitors: 0 })

async function fetchStats() {
  try {
    const res = await request.get('/entry-exit/stats').catch(() => ({}))
    stats.value.todayEntries = res.todayEntries || 0
    stats.value.todayExits = res.todayExits || 0
    stats.value.currentVisitors = res.currentVisitors || 0
  } catch { /* 静默失败 */ }
}

async function handleSearch() {
  if (!query.value.trim()) {
    ElMessage.warning('请输入查询条件')
    return
  }
  searching.value = true
  searchResult.value = null
  try {
    const res = await searchReservation({ query: query.value })
    searchResult.value = res
  } catch {
    ElMessage.error('未找到匹配的预约记录')
  } finally {
    searching.value = false
  }
}

async function handleEntry() {
  if (!searchResult.value) return
  checking.value = true
  try {
    await confirmEntry({ id: searchResult.value.id, gate: '南门' })
    ElMessage.success(`${searchResult.value.visitorName} 已成功入校`)
    searchResult.value = null
    query.value = ''
    await fetchRecent()
    await fetchStats()
  } catch {
    // 错误已在拦截器中处理
  } finally {
    checking.value = false
  }
}

async function fetchRecent() {
  try {
    const res = await request.get('/entry-exit/recent').catch(() => [])
    const items = Array.isArray(res) ? res : []
    recentEntries.value = items.map(r => ({
      name: r.name || '',
      gate: r.gate || '',
      time: r.entryTime ? new Date(r.entryTime).toLocaleString('zh-CN', { month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' }) : '',
    }))
  } catch {
    recentEntries.value = []
  }
}

onMounted(() => {
  fetchRecent()
  fetchStats()
})
</script>

<style scoped>
.entry-check-page {
  max-width: 1200px;
}

.card-header {
  display: flex;
  align-items: center;
  gap: 8px;
}

.check-result {
  margin-top: 16px;
}

.result-actions {
  margin-top: 20px;
  text-align: center;
}

.today-stats {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.stats-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 15px;
}

.stats-row .num {
  font-size: 24px;
  font-weight: 700;
}

.stats-row .num.success { color: #67c23a; }
.stats-row .num.primary { color: #409eff; }
.stats-row .num.danger { color: #f56c6c; }
</style>
