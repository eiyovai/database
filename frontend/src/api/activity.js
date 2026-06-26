import request from './request'

// 获取公开活动列表
export function getPublicActivities(params) {
  return request.get('/activities/public', { params })
}

// 获取活动详情
export function getActivityDetail(id) {
  return request.get(`/activities/${id}`)
}

// 报名活动
export function registerActivity(data) {
  return request.post('/activities/register', data)
}

// 获取我的报名列表
export function getMyRegistrations(params) {
  return request.get('/activities/my-registrations', { params })
}

// 取消报名
export function cancelRegistration(id) {
  return request.put(`/activities/registrations/${id}/cancel`)
}

// === 管理员接口 ===

// 获取活动管理列表
export function getActivityList(params) {
  return request.get('/activities', { params })
}

// 创建活动
export function createActivity(data) {
  return request.post('/activities', data)
}

// 更新活动
export function updateActivity(id, data) {
  return request.put(`/activities/${id}`, data)
}

// 删除活动
export function deleteActivity(id) {
  return request.delete(`/activities/${id}`)
}

// 签到
export function checkInRegistration(id) {
  return request.put(`/activities/checkin/${id}`)
}
