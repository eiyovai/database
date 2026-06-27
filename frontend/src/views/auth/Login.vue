<template>
  <div class="login-page">
    <div class="login-card">
      <div class="login-header">
        <el-icon :size="40" color="#409eff"><School /></el-icon>
        <h2>校园开放预约系统</h2>
        <p>欢迎回来，请登录您的账户</p>
      </div>

      <el-form
        ref="formRef"
        :model="form"
        :rules="rules"
        label-position="top"
        size="large"
        @submit.prevent="handleLogin"
      >
        <el-form-item label="手机号" prop="phone">
          <el-input
            v-model="form.phone"
            placeholder="请输入手机号"
            prefix-icon="Iphone"
          />
        </el-form-item>

        <el-form-item label="密码" prop="password">
          <el-input
            v-model="form.password"
            type="password"
            placeholder="请输入密码"
            prefix-icon="Lock"
            show-password
          />
        </el-form-item>

        <el-form-item>
          <el-button type="primary" :loading="loading" class="submit-btn" @click="handleLogin">
            登 录
          </el-button>
        </el-form-item>
      </el-form>

      <div class="login-footer">
        <span>还没有账户？</span>
        <el-link type="primary" @click="$router.push('/register')">立即注册</el-link>
      </div>

      <el-divider>快速入口</el-divider>
      <div class="quick-entries">
        <el-button type="success" plain @click="quickLogin('visitor')">
          <el-icon><User /></el-icon>访客入口
        </el-button>
        <el-button type="warning" plain @click="quickLogin('admin')">
          <el-icon><Setting /></el-icon>管理员
        </el-button>
        <el-button type="info" plain @click="quickLogin('security')">
          <el-icon><Monitor /></el-icon>安保入口
        </el-button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { ElMessage } from 'element-plus'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const formRef = ref(null)
const loading = ref(false)

const form = reactive({
  phone: '',
  password: '',
})

const rules = {
  phone: [
    { required: true, message: '请输入手机号', trigger: 'blur' },
    { pattern: /^1[3-9]\d{9}$/, message: '请输入正确的手机号', trigger: 'blur' },
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 6, message: '密码至少6位', trigger: 'blur' },
  ],
}

async function handleLogin() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return

  loading.value = true
  try {
    await authStore.login({
      phone: form.phone,
      password: form.password,
    })
    ElMessage.success('登录成功')
    // 根据角色自动跳转
    const roleRoutes = { admin: '/admin/dashboard', security: '/security/entry-check', visitor: '/visitor/reservation' }
    const redirect = route.query.redirect || roleRoutes[authStore.userRole] || '/'
    router.push(redirect)
  } catch {
    // 错误已在拦截器中处理
  } finally {
    loading.value = false
  }
}

function quickLogin(role) {
  const accounts = {
    visitor: { phone: '13800138001', password: '123456' },
    admin: { phone: '13800138000', password: '123456' },
    security: { phone: '13900139001', password: '123456' },
  }
  const acc = accounts[role]
  form.phone = acc.phone
  form.password = acc.password
  handleLogin()
}
</script>

<style scoped>
.login-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.login-card {
  width: 420px;
  padding: 40px;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.15);
}

.login-header {
  text-align: center;
  margin-bottom: 32px;
}

.login-header h2 {
  margin: 12px 0 8px;
  font-size: 22px;
  color: #303133;
}

.login-header p {
  color: #909399;
  font-size: 14px;
}

.submit-btn {
  width: 100%;
}

.login-footer {
  text-align: center;
  font-size: 14px;
  color: #909399;
}

.quick-entries {
  display: flex;
  justify-content: center;
  gap: 12px;
  flex-wrap: wrap;
}
</style>
