<template>
  <div class="open-rules-page">
    <div class="page-header">
      <h2 class="page-title">开放规则配置</h2>
      <el-button type="primary" @click="showDialog(null)">新增规则</el-button>
    </div>

    <el-card shadow="never">
      <el-table :data="rules" stripe>
        <el-table-column prop="dateType" label="日期类型" width="120">
          <template #default="{ row }">
            <el-tag>{{ dateTypeMap[row.dateType] }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="开始日期" width="120">
          <template #default="{ row }">{{ formatDate(row.startDate) }}</template>
        </el-table-column>
        <el-table-column label="结束日期" width="120">
          <template #default="{ row }">{{ formatDate(row.endDate) }}</template>
        </el-table-column>
        <el-table-column prop="timeSlot" label="开放时段" width="150">
          <template #default="{ row }">{{ row.timeSlot === 'all_day' ? '全天' : row.timeSlot }}</template>
        </el-table-column>
        <el-table-column prop="maxCapacity" label="最大接待量" width="110" />
        <el-table-column label="状态" width="80">
          <template #default="{ row }">
            <el-tag :type="row.isActive ? 'success' : 'danger'" size="small">
              {{ row.isActive ? '启用' : '停用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="remark" label="备注" min-width="160" show-overflow-tooltip />
        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <el-button type="primary" size="small" text @click="showDialog(row)">编辑</el-button>
            <el-button type="danger" size="small" text @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 编辑对话框 -->
    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑规则' : '新增规则'" width="550px">
      <el-form ref="formRef" :model="form" :rules="rulesValidator" label-width="100px">
        <el-form-item label="日期类型" prop="dateType">
          <el-select v-model="form.dateType" style="width: 100%">
            <el-option label="工作日" value="weekday" />
            <el-option label="周末" value="weekend" />
            <el-option label="节假日" value="holiday" />
            <el-option label="考试周" value="exam" />
            <el-option label="自定义" value="custom" />
          </el-select>
        </el-form-item>
        <el-form-item label="开始日期" prop="startDate">
          <el-date-picker v-model="form.startDate" type="date" style="width: 100%" />
        </el-form-item>
        <el-form-item label="结束日期" prop="endDate">
          <el-date-picker v-model="form.endDate" type="date" style="width: 100%" />
        </el-form-item>
        <el-form-item label="开放时段" prop="timeSlot">
          <el-select v-model="form.timeSlot" style="width: 100%">
            <el-option label="全天 (08:00-17:00)" value="all_day" />
            <el-option label="上午 (08:00-12:00)" value="morning" />
            <el-option label="下午 (12:00-17:00)" value="afternoon" />
          </el-select>
        </el-form-item>
        <el-form-item label="最大接待量" prop="maxCapacity">
          <el-input-number v-model="form.maxCapacity" :min="0" :max="10000" style="width: 100%" />
        </el-form-item>
        <el-form-item label="状态" prop="status">
          <el-switch v-model="form.status" active-value="active" inactive-value="inactive" />
        </el-form-item>
        <el-form-item label="备注" prop="remark">
          <el-input v-model="form.remark" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveRule">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { getOpenRules, saveOpenRule, deleteOpenRule } from '@/api/admin'
import { ElMessage, ElMessageBox } from 'element-plus'

const rules = ref([])
const dialogVisible = ref(false)
const isEdit = ref(false)
const formRef = ref(null)

const dateTypeMap = { weekday: '工作日', weekend: '周末', holiday: '节假日', exam: '考试周', custom: '自定义' }

const initForm = () => ({
  dateType: 'weekday',
  startDate: '',
  endDate: '',
  timeSlot: 'all_day',
  maxCapacity: 500,
  status: 'active',
  remark: '',
})

const form = reactive(initForm())
const rulesValidator = {}

function formatDate(date) {
  if (!date) return '-'
  return date.includes('T') ? date.split('T')[0] : date
}

async function fetchRules() {
  try {
    const res = await getOpenRules()
    rules.value = res.items || res
  } catch {
    rules.value = [
      { dateType: 'weekday', startDate: '2026-06-01', endDate: '2026-07-31', timeSlot: 'all_day', maxCapacity: 500, status: 'active', remark: '暑假期间工作日开放' },
      { dateType: 'weekend', startDate: '2026-06-01', endDate: '2026-07-31', timeSlot: 'all_day', maxCapacity: 800, status: 'active', remark: '周末开放，容量增加' },
      { dateType: 'exam', startDate: '2026-06-20', endDate: '2026-06-28', timeSlot: 'afternoon', maxCapacity: 200, status: 'active', remark: '考试周限流' },
    ]
  }
}

function showDialog(row) {
  isEdit.value = !!row
  if (row) {
    Object.assign(form, row)
  } else {
    Object.assign(form, initForm())
  }
  dialogVisible.value = true
}

async function saveRule() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  try {
    await saveOpenRule({ ...form })
    ElMessage.success(isEdit.value ? '规则已更新' : '规则已创建')
    dialogVisible.value = false
    await fetchRules()
  } catch {}
}

async function handleDelete(row) {
  try {
    await ElMessageBox.confirm('确定删除该规则？', '确认')
    await deleteOpenRule(row.id)
    ElMessage.success('规则已删除')
    await fetchRules()
  } catch {}
}

onMounted(fetchRules)
</script>

<style scoped>
.open-rules-page {
  max-width: 1400px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.page-title {
  font-size: 20px;
  color: #303133;
  margin: 0;
}
</style>
