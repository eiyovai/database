<template>
  <div class="schedule-page">
    <div class="page-header">
      <h2 class="page-title">排班管理</h2>
      <el-button type="primary" @click="showDialog(null)">新增排班</el-button>
    </div>

    <el-card shadow="never">
      <el-table :data="schedules" stripe>
        <el-table-column label="姓名" width="100">
          <template #default="{ row }">{{ row.staff?.name || row.staffName || '-' }}</template>
        </el-table-column>
        <el-table-column prop="staffRole" label="角色" width="100">
          <template #default="{ row }">
            <el-tag>{{ roleMap[row.staffRole] }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="日期" width="120">
          <template #default="{ row }">{{ formatDate(row.workDate) }}</template>
        </el-table-column>
        <el-table-column prop="shift" label="班次" width="120">
          <template #default="{ row }">{{ shiftMap[row.shift] }}</template>
        </el-table-column>
        <el-table-column prop="location" label="工作地点" width="120" />
        <el-table-column prop="task" label="工作内容" min-width="160" show-overflow-tooltip />
        <el-table-column label="操作" width="140" fixed="right" align="center">
          <template #default="{ row }">
            <div class="action-btns">
              <el-button type="primary" size="small" link @click="showDialog(row)">编辑</el-button>
              <el-button type="danger" size="small" link @click="handleDelete(row)">删除</el-button>
            </div>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑排班' : '新增排班'" width="500px">
      <el-form ref="formRef" :model="form" label-width="100px">
        <el-form-item label="姓名" prop="staffName">
          <el-input v-model="form.staffName" />
        </el-form-item>
        <el-form-item label="角色" prop="staffRole">
          <el-select v-model="form.staffRole" style="width: 100%">
            <el-option label="志愿者" value="volunteer" />
            <el-option label="讲解员" value="guide" />
            <el-option label="安保人员" value="security" />
          </el-select>
        </el-form-item>
        <el-form-item label="日期" prop="workDate">
          <el-date-picker v-model="form.workDate" type="date" style="width: 100%" />
        </el-form-item>
        <el-form-item label="班次" prop="shift">
          <el-select v-model="form.shift" style="width: 100%">
            <el-option label="早班 08:00-12:00" value="morning" />
            <el-option label="中班 12:00-16:00" value="afternoon" />
            <el-option label="晚班 16:00-20:00" value="evening" />
            <el-option label="全天 08:00-17:00" value="full_day" />
          </el-select>
        </el-form-item>
        <el-form-item label="工作地点" prop="location">
          <el-input v-model="form.location" />
        </el-form-item>
        <el-form-item label="工作内容" prop="task">
          <el-input v-model="form.task" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveSchedule">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { getScheduleList, createSchedule, updateSchedule, deleteSchedule } from '@/api/admin'
import { ElMessage, ElMessageBox } from 'element-plus'

const schedules = ref([])
const dialogVisible = ref(false)
const isEdit = ref(false)
const formRef = ref(null)

const roleMap = { volunteer: '志愿者', guide: '讲解员', security: '安保人员' }
const shiftMap = { morning: '早班 08-12', afternoon: '中班 12-16', evening: '晚班 16-20', full_day: '全天' }

const form = reactive({
  staffName: '', staffRole: '', workDate: '', shift: '', location: '', task: '',
})

async function fetchSchedules() {
  try {
    const res = await getScheduleList()
    schedules.value = res.items || res
  } catch {
    schedules.value = []
  }
}

function formatDate(date) {
  if (!date) return '-'
  return date.split('T')[0] || date
}

function showDialog(row) {
  isEdit.value = !!row
  Object.assign(form, row || { staffName: '', staffRole: '', workDate: '', shift: '', location: '', task: '' })
  dialogVisible.value = true
}

async function saveSchedule() {
  try {
    if (isEdit.value) {
      await updateSchedule(form.id, form)
    } else {
      await createSchedule(form)
    }
    ElMessage.success(isEdit.value ? '排班已更新' : '排班已创建')
    dialogVisible.value = false
    await fetchSchedules()
  } catch {}
}

async function handleDelete(row) {
  try {
    await ElMessageBox.confirm('确定删除该排班？', '确认')
    await deleteSchedule(row.id)
    ElMessage.success('排班已删除')
    await fetchSchedules()
  } catch {}
}

onMounted(fetchSchedules)
</script>

<style scoped>
.schedule-page { max-width: 1400px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.page-title { font-size: 20px; margin: 0; }

.action-btns {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
}
</style>
