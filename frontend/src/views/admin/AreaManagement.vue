<template>
  <div class="area-page">
    <div class="page-header">
      <h2 class="page-title">校园区域管理</h2>
      <el-button type="primary" @click="showDialog(null)">新增区域</el-button>
    </div>

    <el-card shadow="never">
      <el-table :data="areas" stripe>
        <el-table-column prop="name" label="区域名称" width="150" />
        <el-table-column prop="code" label="区域编码" width="120" />
        <el-table-column prop="type" label="区域类型" width="100">
          <template #default="{ row }">
            <el-tag>{{ areaTypeMap[row.type] }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip />
        <el-table-column prop="accessLevel" label="开放等级" width="100">
          <template #default="{ row }">
            <el-tag :type="levelType(row.accessLevel)">{{ levelMap[row.accessLevel] }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="允许访客类型" min-width="180">
          <template #default="{ row }">
            <el-tag v-for="t in (row.areaPermissions?.map(p => p.visitorType) || row.allowedTypes || [])" :key="t" size="small" style="margin-right: 4px">
              {{ typeMap[t] }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <el-button type="primary" size="small" text @click="showDialog(row)">编辑</el-button>
            <el-button type="danger" size="small" text @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑区域' : '新增区域'" width="550px">
      <el-form ref="formRef" :model="form" label-width="100px">
        <el-form-item label="区域名称" prop="name">
          <el-input v-model="form.name" />
        </el-form-item>
        <el-form-item label="区域编码" prop="code">
          <el-input v-model="form.code" />
        </el-form-item>
        <el-form-item label="区域类型" prop="type">
          <el-select v-model="form.type" style="width: 100%">
            <el-option label="公共区域" value="public" />
            <el-option label="教学区域" value="academic" />
            <el-option label="办公区域" value="office" />
            <el-option label="生活区域" value="living" />
            <el-option label="实验区域" value="lab" />
            <el-option label="封闭区域" value="restricted" />
          </el-select>
        </el-form-item>
        <el-form-item label="开放等级" prop="accessLevel">
          <el-select v-model="form.accessLevel" style="width: 100%">
            <el-option label="公共开放" value="public" />
            <el-option label="限制开放" value="restricted" />
            <el-option label="禁止进入" value="forbidden" />
          </el-select>
        </el-form-item>
        <el-form-item label="允许访客" prop="allowedTypes">
          <el-checkbox-group v-model="form.allowedTypes">
            <el-checkbox label="parent">家长</el-checkbox>
            <el-checkbox label="alumni">校友</el-checkbox>
            <el-checkbox label="tourist">普通游客</el-checkbox>
            <el-checkbox label="study_group">研学团队</el-checkbox>
            <el-checkbox label="partner">合作单位</el-checkbox>
          </el-checkbox-group>
        </el-form-item>
        <el-form-item label="区域描述" prop="description">
          <el-input v-model="form.description" type="textarea" :rows="3" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
          <el-button type="primary" @click="handleSaveArea">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { getAreaList, saveArea as saveAreaApi, deleteArea } from '@/api/admin'
import { ElMessage, ElMessageBox } from 'element-plus'

const areas = ref([])
const dialogVisible = ref(false)
const isEdit = ref(false)
const formRef = ref(null)

const areaTypeMap = { public: '公共区域', academic: '教学区域', office: '办公区域', living: '生活区域', lab: '实验区域', restricted: '封闭区域' }
const levelMap = { public: '公共开放', restricted: '限制开放', forbidden: '禁止进入' }
const levelType = (l) => ({ public: 'success', restricted: 'warning', forbidden: 'danger' }[l])
const typeMap = { parent: '家长', alumni: '校友', tourist: '游客', study_group: '研学团', partner: '合作单位' }

const form = reactive({
  name: '', code: '', type: 'public', accessLevel: 'public', allowedTypes: [], description: '',
})

async function fetchAreas() {
  try {
    const res = await getAreaList()
    areas.value = res.items || res
  } catch {
    areas.value = [
      { name: '校门广场', code: 'A01', type: 'public', accessLevel: 'public', allowedTypes: ['parent', 'alumni', 'tourist', 'study_group', 'partner'], description: '校园入口区域' },
      { name: '校史馆', code: 'A02', type: 'public', accessLevel: 'restricted', allowedTypes: ['alumni', 'study_group'], description: '需预约参观' },
      { name: '实验室展示区', code: 'B01', type: 'lab', accessLevel: 'restricted', allowedTypes: ['study_group', 'partner'], description: '研学团队及合作单位可进入' },
      { name: '办公区域', code: 'C01', type: 'office', accessLevel: 'restricted', allowedTypes: ['partner'], description: '仅合作单位人员可通行' },
    ]
  }
}

function showDialog(row) {
  isEdit.value = !!row
  Object.assign(form, row ? { ...row, allowedTypes: row.areaPermissions?.map(p => p.visitorType) || row.allowedTypes || [] } : { name: '', code: '', type: 'public', accessLevel: 'public', allowedTypes: [], description: '' })
  dialogVisible.value = true
}

async function handleSaveArea() {
  try {
    await saveAreaApi({ ...form, allowedTypes: form.allowedTypes })
    ElMessage.success(isEdit.value ? '区域已更新' : '区域已创建')
    dialogVisible.value = false
    await fetchAreas()
  } catch {}
}

async function handleDelete(row) {
  try {
    await ElMessageBox.confirm('确定删除该区域？', '确认')
    await deleteArea(row.id)
    ElMessage.success('区域已删除')
    await fetchAreas()
  } catch {}
}

onMounted(fetchAreas)
</script>

<style scoped>
.area-page {
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
  margin: 0;
}
</style>
