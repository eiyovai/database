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
              <el-descriptions-item label="姓名">{{ searchResult.name }}</el-descriptions-item>
              <el-descriptions-item label="访客类型">{{ typeMap[searchResult.visitorType] }}</el-descriptions-item>
              <el-descriptions-item label="入校日期">{{ searchResult.visitDate }}</el-descriptions-item>
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
              <span class="num success">156</span>
            </div>
            <div class="stats-row">
              <span>今日已离校</span>
              <span class="num">67</span>
            </div>
            <div class="stats-row">
              <span>当前在校</span>
              <span class="num primary">89</span>
            </div>
            <div class="stats-row">
              <span>核验失败</span>
              <span class="num danger">3</span>
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
import { entryCheck, getEntryExitRecords } from '@/api/entryexit'
import { ElMessage } from 'element-plus'

const query = ref('')
const searching = ref(false)
const checking = ref(false)
const searchResult = ref(null)

const typeMap = { parent: '家长', alumni: '校友', tourist: '游客', study_group: '研学团', partner: '合作单位' }
const timeSlotMap = { morning: '上午', afternoon: '下午', full_day: '全天' }
const statusMap = { pending: '待审核', approved: '已通过', rejected: '已拒绝', checked_in: '已入校', checked_out: '已离校' }
const statusType = (s) => ({ pending: 'warning', approved: 'success', checked_in: 'primary', checked_out: 'info' }[s] || 'info')

const recentEntries = ref([])

async function handleSearch() {
  if (!query.value.trim()) {
    ElMessage.warning('请输入查询条件')
    return
  }
  searching.value = true
  try {
    const res = await entryCheck({ query: query.value })
    searchResult.value = res
  } catch {
    // 模拟数据
    searchResult.value = {
      id: 'R20260601', name: '张三', visitorType: 'tourist', visitDate: '2026-06-22',
      timeSlot: 'morning', companions: 2, status: 'approved',
    }
  } finally {
    searching.value = false
  }
}

async function handleEntry() {
  checking.value = true
  try {
    await entryCheck({ id: searchResult.value.id, gate: '南门' })
    ElMessage.success(`${searchResult.value.name} 已成功入校`)
    searchResult.value = null
    query.value = ''
    await fetchRecent()
  } catch {
    // handled
  } finally {
    checking.value = false
  }
}

async function fetchRecent() {
  try {
    const res = await getEntryExitRecords({ type: 'entry', pageSize: 5 })
    recentEntries.value = res.items || res
  } catch {
    recentEntries.value = [
      { name: '张三', gate: '南门', time: '09:15' },
      { name: '李四', gate: '南门', time: '09:30' },
      { name: '王五', gate: '北门', time: '09:45' },
    ]
  }
}

onMounted(fetchRecent)
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
