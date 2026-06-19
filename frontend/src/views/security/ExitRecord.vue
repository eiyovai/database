<template>
  <div class="exit-page">
    <el-row :gutter="24">
      <el-col :span="14">
        <el-card shadow="never">
          <template #header>
            <span><el-icon><CircleCheck /></el-icon> 离校登记</span>
          </template>

          <el-form label-position="top" size="large">
            <el-form-item label="搜索在校访客">
              <el-input
                v-model="query"
                placeholder="输入姓名或手机号搜索在校访客"
                size="large"
                clearable
                @keyup.enter="handleSearch"
              >
                <template #prefix><el-icon><Search /></el-icon></template>
              </el-input>
            </el-form-item>
            <el-button type="primary" size="large" @click="handleSearch">搜索</el-button>
          </el-form>

          <div v-if="searchResult" class="result-card">
            <el-divider />
            <el-descriptions :column="2" border size="small">
              <el-descriptions-item label="姓名">{{ searchResult.name }}</el-descriptions-item>
              <el-descriptions-item label="入校时间">{{ searchResult.entryTime }}</el-descriptions-item>
              <el-descriptions-item label="入校校门">{{ searchResult.entryGate }}</el-descriptions-item>
              <el-descriptions-item label="同行人数">{{ searchResult.companions }}</el-descriptions-item>
            </el-descriptions>

            <div class="result-actions">
              <el-form-item label="选择出口校门">
                <el-select v-model="exitGate" style="width: 100%">
                  <el-option label="南门（正门）" value="南门" />
                  <el-option label="北门" value="北门" />
                  <el-option label="东门" value="东门" />
                  <el-option label="西门" value="西门" />
                </el-select>
              </el-form-item>
              <el-button type="warning" size="large" :loading="exiting" @click="handleExit">
                <el-icon><CircleCheck /></el-icon> 确认离校
              </el-button>
            </div>
          </div>
        </el-card>
      </el-col>

      <el-col :span="10">
        <el-card shadow="never">
          <template #header>今日离校统计</template>
          <div class="stats">
            <div class="stats-row">
              <span>已离校人数</span>
              <span class="num">67</span>
            </div>
            <div class="stats-row">
              <span>未离校（在校）</span>
              <span class="num primary">89</span>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { exitRecord } from '@/api/entryexit'
import { ElMessage } from 'element-plus'

const query = ref('')
const exitGate = ref('南门')
const searchResult = ref(null)
const exiting = ref(false)

function handleSearch() {
  if (!query.value.trim()) {
    ElMessage.warning('请输入搜索条件')
    return
  }
  searchResult.value = {
    name: '张三', entryTime: '2026-06-22 09:15', entryGate: '南门', companions: 2,
  }
}

async function handleExit() {
  exiting.value = true
  try {
    await exitRecord({ name: searchResult.value.name, gate: exitGate.value })
    ElMessage.success(`${searchResult.value.name} 已离校`)
    searchResult.value = null
    query.value = ''
  } catch {} finally {
    exiting.value = false
  }
}
</script>

<style scoped>
.exit-page { max-width: 1200px; }
.result-actions { margin-top: 20px; }
.stats { display: flex; flex-direction: column; gap: 16px; }
.stats-row { display: flex; justify-content: space-between; align-items: center; font-size: 15px; }
.stats .num { font-size: 24px; font-weight: 700; }
.stats .num.primary { color: #409eff; }
</style>
