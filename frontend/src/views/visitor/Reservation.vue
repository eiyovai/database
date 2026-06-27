<template>
  <div class="reservation-page">
    <!-- 横幅 -->
    <div class="banner">
      <div class="banner-content">
        <h1>欢迎预约参观校园</h1>
        <p>选择您希望参观的日期和时段，提交预约申请</p>
      </div>
    </div>

    <div class="content-wrapper">
      <el-row :gutter="24">
        <!-- 预约表单 -->
        <el-col :span="16">
          <el-card shadow="never">
            <template #header>
              <span class="card-title">提交入校预约</span>
            </template>

            <el-form
              ref="formRef"
              :model="form"
              :rules="rules"
              label-width="100px"
              size="large"
            >
              <el-form-item label="姓名" prop="name">
                <el-input v-model="form.name" placeholder="请输入您的姓名" />
              </el-form-item>

              <el-form-item label="手机号" prop="phone">
                <el-input v-model="form.phone" placeholder="请输入手机号" />
              </el-form-item>

              <el-form-item label="访客类型" prop="visitorType">
                <el-select v-model="form.visitorType" placeholder="请选择访客类型" style="width: 100%">
                  <el-option label="家长" value="parent" />
                  <el-option label="校友" value="alumni" />
                  <el-option label="普通游客" value="tourist" />
                  <el-option label="研学团队" value="study_group" />
                  <el-option label="合作单位" value="partner" />
                </el-select>
              </el-form-item>

              <el-form-item label="入校日期" prop="visitDate">
                <el-date-picker
                  v-model="form.visitDate"
                  type="date"
                  placeholder="选择入校日期"
                  :disabled-date="disabledDate"
                  style="width: 100%"
                />
              </el-form-item>

              <el-form-item label="预计时段" prop="timeSlot">
                <el-select v-model="form.timeSlot" placeholder="请选择入校时段" style="width: 100%">
                  <el-option label="上午 (08:00-12:00)" value="morning" />
                  <el-option label="下午 (12:00-17:00)" value="afternoon" />
                  <el-option label="全天 (08:00-17:00)" value="full_day" />
                </el-select>
              </el-form-item>

              <el-form-item label="同行人数" prop="companions">
                <el-input-number v-model="form.companions" :min="0" :max="50" />
                <span class="form-tip">（不含本人）</span>
              </el-form-item>

              <el-form-item label="预计停留" prop="stayDuration">
                <el-select v-model="form.stayDuration" placeholder="预计停留时间" style="width: 100%">
                  <el-option label="1小时以内" value="1h" />
                  <el-option label="1-2小时" value="2h" />
                  <el-option label="2-4小时" value="4h" />
                  <el-option label="半天" value="half_day" />
                  <el-option label="全天" value="full_day" />
                </el-select>
              </el-form-item>

              <el-form-item label="入校事由" prop="purpose">
                <el-input
                  v-model="form.purpose"
                  type="textarea"
                  :rows="3"
                  placeholder="请简要说明入校事由"
                  maxlength="200"
                  show-word-limit
                />
              </el-form-item>

              <el-form-item>
                <el-button type="primary" :loading="submitting" size="large" @click="handleSubmit">
                  提交预约申请
                </el-button>
                <el-button size="large" @click="resetForm">重置</el-button>
              </el-form-item>
            </el-form>
          </el-card>
        </el-col>

        <!-- 右侧信息 -->
        <el-col :span="8">
          <el-card shadow="never" class="info-card">
            <template #header>
              <span class="card-title">预约须知</span>
            </template>
            <ul class="notice-list">
              <li>请如实填写个人信息，入校时将核验身份</li>
              <li>预约需经管理员审核，请耐心等待</li>
              <li>入校时请配合安保人员核验预约信息</li>
              <li>请在预约时段内入校，逾时预约将失效</li>
              <li>校园开放时间以管理员配置为准</li>
              <li>如无法前往，请提前取消预约</li>
            </ul>
          </el-card>

          <el-card shadow="never" class="info-card">
            <template #header>
              <span class="card-title">今日校园状态</span>
            </template>
            <div class="status-info">
              <div class="status-item">
                <span class="label">开放状态</span>
                <el-tag :type="campusStatus.isOpen ? 'success' : 'danger'" effect="dark">
                  {{ campusStatus.isOpen ? '今日开放' : '暂停开放' }}
                </el-tag>
              </div>
              <div class="status-item">
                <span class="label">已预约人数</span>
                <span class="value">{{ campusStatus.todayReservations }} / {{ campusStatus.maxCapacity }}</span>
              </div>
              <div class="status-item">
                <span class="label">在校访客</span>
                <span class="value">{{ campusStatus.currentVisitors }} 人</span>
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { createReservation } from '@/api/reservation'
import request from '@/api/request'
import { ElMessage } from 'element-plus'

const authStore = useAuthStore()
const formRef = ref(null)
const submitting = ref(false)

const campusStatus = ref({
  isOpen: true,
  todayReservations: 0,
  currentVisitors: 0,
  maxCapacity: 0,
})

async function fetchCampusStatus() {
  try {
    const res = await request.get('/public/campus-status')
    campusStatus.value = res
  } catch {
    // 保持默认值
  }
}

onMounted(fetchCampusStatus)

const form = reactive({
  name: '',
  phone: '',
  visitorType: '',
  visitDate: '',
  timeSlot: '',
  companions: 0,
  stayDuration: '',
  purpose: '',
})

const rules = {
  name: [{ required: true, message: '请输入姓名', trigger: 'blur' }],
  phone: [
    { required: true, message: '请输入手机号', trigger: 'blur' },
    { pattern: /^1[3-9]\d{9}$/, message: '请输入正确的手机号', trigger: 'blur' },
  ],
  visitorType: [{ required: true, message: '请选择访客类型', trigger: 'change' }],
  visitDate: [{ required: true, message: '请选择入校日期', trigger: 'change' }],
  timeSlot: [{ required: true, message: '请选择入校时段', trigger: 'change' }],
  purpose: [{ required: true, message: '请输入入校事由', trigger: 'blur' }],
}

// 禁用过去的日期
function disabledDate(time) {
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  return time.getTime() < today.getTime()
}

async function handleSubmit() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return

  submitting.value = true
  try {
    await createReservation(form)
    ElMessage.success('预约申请已提交，请等待审核')
    resetForm()
  } catch {
    // 错误已在拦截器中处理
  } finally {
    submitting.value = false
  }
}

function resetForm() {
  formRef.value.resetFields()
  form.companions = 0
}
</script>

<style scoped>
.reservation-page {
  max-width: 1200px;
  margin: 0 auto;
}

.banner {
  background: linear-gradient(135deg, #409eff 0%, #337ecc 100%);
  border-radius: 12px;
  padding: 40px;
  margin-bottom: 24px;
  color: #fff;
}

.banner-content h1 {
  font-size: 28px;
  margin-bottom: 8px;
}

.banner-content p {
  font-size: 15px;
  opacity: 0.9;
}

.content-wrapper {
  margin-top: 8px;
}

.card-title {
  font-size: 16px;
  font-weight: 600;
}

.form-tip {
  margin-left: 8px;
  color: #909399;
  font-size: 13px;
}

.info-card {
  margin-bottom: 16px;
}

.notice-list {
  padding-left: 20px;
  line-height: 2;
  color: #606266;
  font-size: 14px;
}

.status-info {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.status-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.status-item .label {
  color: #606266;
  font-size: 14px;
}

.status-item .value {
  font-size: 16px;
  font-weight: 600;
  color: #303133;
}
</style>
