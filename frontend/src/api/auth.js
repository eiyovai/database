import request from './request'

// 登录
export function login(data) {
  return request.post('/auth/login', data)
}

// 注册
export function register(data) {
  return request.post('/auth/register', data)
}

// 获取当前用户信息
export function getUserInfo() {
  return request.get('/auth/userinfo')
}

// 修改密码
export function changePassword(data) {
  return request.put('/auth/password', data)
}
