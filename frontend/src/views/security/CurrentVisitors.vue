<template>
  <div class="current-visitors-page">
    <el-row :gutter="16" class="summary-cards">
      <el-col :span="8">
        <el-card shadow="never">
          <div class="summary-item">
            <div class="num primary">89</div>
            <div class="label">当前在校访客</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="never">
          <div class="summary-item">
            <div class="num success">156</div>
            <div class="label">今日累计入校</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="never">
          <div class="summary-item">
            <div class="num warning">500</div>
            <div class="label">今日容量上限</div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-card shadow="never">
      <template #header>
        <el-input v-model="keyword" placeholder="搜索姓名或手机号" clearable prefix-icon="Search" style="width: 300px" />
      </template>

      <el-table :data="visitors" stripe>
        <el-table-column prop="name" label="姓名" width="100" />
        <el-table-column prop="phone" label="手机号" width="130" />
        <el-table-column prop="visitorType" label="访客类型" width="100">
          <template #default="{ row }">{{ typeMap[row.visitorType] }}</template>
        </el-table-column>
        <el-table-column prop="entryTime" label="入校时间" width="150" />
        <el-table-column prop="entryGate" label="入校校门" width="100" />
        <el-table-column prop="expectedExit" label="预计离校" width="120" />
        <el-table-column label="停留时长" width="100">
          <template #default="{ row }">{{ row.duration || '计算中' }}</template>
        </el-table-column>
        <el-table-column label="状态" width="80">
          <template #default="{ row }">
            <el-tag type="success" size="small">在校</el-tag>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getCurrentVisitors } from '@/api/entryexit'

const keyword = ref('')
const visitors = ref([])

const typeMap = { parent: '家长', alumni: '校友', tourist: '游客', study_group: '研学团', partner: '合作单位' }

async function fetchVisitors() {
  try {
    const res = await getCurrentVisitors()
    visitors.value = res.items || res
  } catch {
    visitors.value = [
      { name: '张三', phone: '138****1234', visitorType: 'tourist', entryTime: '2026-06-22 09:15', entryGate: '南门', expectedExit: '12:00', duration: '2h 15min' },
      { name: '李四', phone: '139****5678', visitorType: 'alumni', entryTime: '2026-06-22 09:30', entryGate: '南门', expectedExit: '16:00', duration: '2h' },
      { name: '王五', phone: '137****9012', visitorType: 'study_group', entryTime: '2026-06-22 09:45', entryGate: '北门', expectedExit: '15:00', duration: '1h 45min' },
    ]
  }
}

onMounted(fetchVisitors)
</script>

<style scoped>
.current-visitors-page { max-width: 1400px; }
.summary-cards { margin-bottom: 20px; }
.summary-item { text-align: center; padding: 8px 0; }
.summary-item .num { font-size: 32px; font-weight: 700; }
.summary-item .num.primary { color: #409eff; }
.summary-item .num.success { color: #67c23a; }
.summary-item .num.warning { color: #e6a23c; }
.summary-item .label { font-size: 14px; color: #909399; margin-top: 4px; }
</style>
