<template>
  <div class="visitor-report-page">
    <h2 class="page-title">违规举报</h2>

    <el-row :gutter="24">
      <el-col :span="16">
        <el-card shadow="never">
          <template #header>提交举报</template>
          <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
            <el-form-item label="举报对象" prop="target">
              <el-input v-model="form.target" placeholder="被举报对象姓名或描述" />
            </el-form-item>
            <el-form-item label="违规类型" prop="violationType">
              <el-select v-model="form.violationType" style="width: 100%">
                <el-option label="越权进入封闭区域" value="trespass" />
                <el-option label="扰乱校园秩序" value="disturbance" />
                <el-option label="损坏公物" value="damage" />
                <el-option label="超时滞留" value="overstay" />
                <el-option label="其他" value="other" />
              </el-select>
            </el-form-item>
            <el-form-item label="发生地点" prop="location">
              <el-input v-model="form.location" placeholder="违规发生的地点" />
            </el-form-item>
            <el-form-item label="发生时间" prop="occurredAt">
              <el-date-picker v-model="form.occurredAt" type="datetime" style="width: 100%" placeholder="选择违规发生时间" />
            </el-form-item>
            <el-form-item label="违规描述" prop="description">
              <el-input v-model="form.description" type="textarea" :rows="4" placeholder="请详细描述违规情况" />
            </el-form-item>
            <el-form-item>
              <el-button type="primary" size="large" :loading="submitting" @click="handleSubmit">
                提交举报
              </el-button>
              <el-button size="large" @click="formRef.resetFields()">重置</el-button>
            </el-form-item>
          </el-form>
        </el-card>
      </el-col>

      <el-col :span="8">
        <el-card shadow="never">
          <template #header>我的举报记录</template>
          <el-timeline v-if="myReports.length > 0">
            <el-timeline-item
              v-for="r in myReports"
              :key="r.id"
              :timestamp="formatTime(r.createdAt)"
              :type="r.status === 'approved' ? 'success' : r.status === 'pending' ? 'warning' : 'info'"
            >
              <p>{{ r.targetName }} - {{ violationTypeMap[r.violationType] || r.violationType }}</p>
              <el-tag :type="statusType(r.status)" size="small">
                {{ statusMap[r.status] || r.status }}
              </el-tag>
            </el-timeline-item>
          </el-timeline>
          <el-empty v-else description="暂无举报记录" />
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { submitReport, getReportList } from '@/api/report'
import { ElMessage } from 'element-plus'

const formRef = ref(null)
const submitting = ref(false)
const myReports = ref([])

const violationTypeMap = {
  trespass: '越权进入', disturbance: '扰乱秩序', damage: '损坏公物', overstay: '超时滞留', other: '其他',
}
const statusMap = { pending: '待审核', approved: '已通过', rejected: '已驳回' }

function statusType(status) {
  return status === 'approved' ? 'success' : status === 'pending' ? 'warning' : 'info'
}

function formatTime(date) {
  if (!date) return '-'
  return date.includes('T') ? date.replace('T', ' ').substring(0, 16) : date
}

const form = reactive({
  target: '', violationType: '', location: '', occurredAt: '', description: '',
})

const rules = {
  target: [{ required: true, message: '请输入举报对象', trigger: 'blur' }],
  violationType: [{ required: true, message: '请选择违规类型', trigger: 'change' }],
  location: [{ required: true, message: '请输入发生地点', trigger: 'blur' }],
  description: [{ required: true, message: '请输入违规描述', trigger: 'blur' }],
}

async function handleSubmit() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  submitting.value = true
  try {
    await submitReport({ ...form })
    ElMessage.success('举报已提交，等待审核')
    formRef.value.resetFields()
    await fetchMyReports()
  } catch {} finally {
    submitting.value = false
  }
}

async function fetchMyReports() {
  try {
    const res = await getReportList({ my: true })
    myReports.value = (res.items || res).slice(0, 20)
  } catch {
    myReports.value = []
  }
}

onMounted(fetchMyReports)
</script>

<style scoped>
.visitor-report-page {
  max-width: 1200px;
  margin: 0 auto;
}

.page-title {
  font-size: 20px;
  color: #303133;
  margin-bottom: 20px;
}
</style>
