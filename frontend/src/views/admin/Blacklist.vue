<template>
  <div class="blacklist-page">
    <h2 class="page-title">黑名单管理</h2>

    <div style="margin-bottom: 16px">
      <el-button type="danger" @click="showAddDialog = true">
        <el-icon><Plus /></el-icon>添加黑名单
      </el-button>
    </div>

    <!-- 添加黑名单对话框 -->
    <el-dialog v-model="showAddDialog" title="添加黑名单" width="420px">
      <el-form :model="addForm" label-width="80px">
        <el-form-item label="用户名">
          <el-select v-model="addForm.userName" filterable placeholder="请选择用户" style="width: 100%">
            <el-option v-for="u in userList" :key="u.id" :label="`${u.name} (${u.phone})`" :value="u.name" />
          </el-select>
        </el-form-item>
        <el-form-item label="拉黑原因">
          <el-input v-model="addForm.reason" type="textarea" :rows="3" placeholder="请输入拉黑原因" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showAddDialog = false">取消</el-button>
        <el-button type="danger" @click="handleAdd" :loading="adding">确定拉黑</el-button>
      </template>
    </el-dialog>

    <el-card shadow="never">
      <el-table :data="list" stripe>
        <el-table-column label="姓名" width="100">
          <template #default="{ row }">{{ row.user?.name || row.name }}</template>
        </el-table-column>
        <el-table-column label="手机号" width="130">
          <template #default="{ row }">{{ row.user?.phone || row.phone }}</template>
        </el-table-column>
        <el-table-column label="违规次数" width="100">
          <template #default="{ row }">{{ row.violationCount }} 次</template>
        </el-table-column>
        <el-table-column label="违规类型" min-width="180">
          <template #default="{ row }">
            <el-tag size="small">{{ row.reason?.substring(0, 10) || '多次违规' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="拉黑时间" width="160">
          <template #default="{ row }">{{ formatDate(row.blacklistedAt) }}</template>
        </el-table-column>
        <el-table-column prop="reason" label="拉黑原因" min-width="160" show-overflow-tooltip />
        <el-table-column label="操作" width="100" fixed="right">
          <template #default="{ row }">
            <el-button type="success" size="small" text @click="handleRemove(row)">移出</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getBlacklist, removeBlacklist, addBlacklist, getUserList } from '@/api/admin'
import { ElMessage, ElMessageBox } from 'element-plus'

const list = ref([])
const showAddDialog = ref(false)
const adding = ref(false)
const userList = ref([])
const addForm = ref({ userName: '', reason: '' })

const violationTypeMap = {
  no_show: '预约爽约', overstay: '超时滞留', trespass: '越权进入',
  duplicate: '恶意重复预约', absence: '活动多次缺席',
}

function formatDate(date) {
  if (!date) return '-'
  return date.includes('T') ? date.split('T')[0] : date
}

async function fetchList() {
  try {
    const res = await getBlacklist()
    list.value = res.items || res
  } catch {
    list.value = []
  }
}

async function fetchUsers() {
  try {
    const res = await getUserList()
    userList.value = res.items || res || []
  } catch {}
}

async function handleAdd() {
  if (!addForm.value.userName) {
    ElMessage.warning('请选择用户')
    return
  }
  if (!addForm.value.reason) {
    ElMessage.warning('请输入拉黑原因')
    return
  }
  adding.value = true
  try {
    await addBlacklist(addForm.value)
    ElMessage.success('已加入黑名单')
    showAddDialog.value = false
    addForm.value = { userName: '', reason: '' }
    await fetchList()
  } catch (e) {
    ElMessage.error(e?.response?.data?.message || '添加失败')
  } finally {
    adding.value = false
  }
}

async function handleRemove(row) {
  try {
    await ElMessageBox.confirm(`确定将 ${row.name} 移出黑名单？`, '确认')
    await removeBlacklist(row.id)
    ElMessage.success('已移出黑名单')
    await fetchList()
  } catch {}
}

onMounted(() => {
  fetchList()
  fetchUsers()
})
</script>

<style scoped>
.blacklist-page { max-width: 1400px; }
.page-title { font-size: 20px; margin-bottom: 20px; }
</style>
