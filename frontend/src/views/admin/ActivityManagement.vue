<template>
  <div class="activity-mgmt">
    <div class="page-header">
      <h2 class="page-title">活动管理</h2>
      <el-button type="primary" @click="showDialog(null)">发布活动</el-button>
    </div>

    <el-card shadow="never">
      <el-table :data="activities" stripe>
        <el-table-column prop="title" label="活动名称" min-width="160" />
        <el-table-column prop="location" label="地点" width="120" />
        <el-table-column prop="startTime" label="开始时间" width="160" />
        <el-table-column prop="endTime" label="结束时间" width="160" />
        <el-table-column label="报名情况" width="120">
          <template #default="{ row }">
            {{ row.registered }}/{{ row.maxParticipants }}
          </template>
        </el-table-column>
        <el-table-column prop="status" label="状态" width="90">
          <template #default="{ row }">
            <el-tag :type="row.status === 'open' ? 'success' : 'info'" size="small">
              {{ row.status === 'open' ? '报名中' : '已结束' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="160" fixed="right" align="center">
          <template #default="{ row }">
            <el-button type="primary" size="small" text @click="showDialog(row)">编辑</el-button>
            <el-button type="danger" size="small" text @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑活动' : '发布活动'" width="600px">
      <el-form ref="formRef" :model="form" label-width="100px">
        <el-form-item label="活动名称" prop="title">
          <el-input v-model="form.title" />
        </el-form-item>
        <el-form-item label="活动地点" prop="location">
          <el-input v-model="form.location" />
        </el-form-item>
        <el-form-item label="开始时间" prop="startTime">
          <el-date-picker v-model="form.startTime" type="datetime" style="width: 100%" />
        </el-form-item>
        <el-form-item label="结束时间" prop="endTime">
          <el-date-picker v-model="form.endTime" type="datetime" style="width: 100%" />
        </el-form-item>
        <el-form-item label="人数上限" prop="maxParticipants">
          <el-input-number v-model="form.maxParticipants" :min="1" :max="1000" style="width: 100%" />
        </el-form-item>
        <el-form-item label="活动描述" prop="description">
          <el-input v-model="form.description" type="textarea" :rows="4" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveActivity">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { getActivityList, createActivity, updateActivity, deleteActivity } from '@/api/activity'
import { ElMessage, ElMessageBox } from 'element-plus'

const activities = ref([])
const dialogVisible = ref(false)
const isEdit = ref(false)
const formRef = ref(null)

const form = reactive({
  title: '', location: '', startTime: '', endTime: '', maxParticipants: 100, description: '',
})

async function fetchActivities() {
  try {
    const res = await getActivityList()
    activities.value = res.items || res
  } catch {
    activities.value = []
  }
}

function showDialog(row) {
  isEdit.value = !!row
  Object.assign(form, row || { title: '', location: '', startTime: '', endTime: '', maxParticipants: 100, description: '' })
  dialogVisible.value = true
}

async function saveActivity() {
  try {
    if (isEdit.value) {
      await updateActivity(form.id, form)
    } else {
      await createActivity(form)
    }
    ElMessage.success(isEdit.value ? '活动已更新' : '活动已发布')
    dialogVisible.value = false
    await fetchActivities()
  } catch {}
}

async function handleDelete(row) {
  try {
    await ElMessageBox.confirm('确定删除该活动？', '确认')
    await deleteActivity(row.id)
    ElMessage.success('活动已删除')
    await fetchActivities()
  } catch {}
}

onMounted(fetchActivities)
</script>

<style scoped>
.activity-mgmt {
  max-width: 1400px;
}
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}
.page-title { font-size: 20px; margin: 0; }
</style>
