import request from './request'

// === 开放规则 ===

// 获取开放规则列表
export function getOpenRules(params) {
  return request.get('/open-rules', { params })
}

// 创建/更新开放规则
export function saveOpenRule(data) {
  return request.post('/open-rules', data)
}

// 删除开放规则
export function deleteOpenRule(id) {
  return request.delete(`/open-rules/${id}`)
}

// === 校园区域 ===

// 获取区域列表
export function getAreaList(params) {
  return request.get('/areas', { params })
}

// 创建/更新区域
export function saveArea(data) {
  return request.post('/areas', data)
}

// 删除区域
export function deleteArea(id) {
  return request.delete(`/areas/${id}`)
}

// === 黑名单 ===

// 获取黑名单列表
export function getBlacklist(params) {
  return request.get('/blacklist', { params })
}

// 移出黑名单
export function removeBlacklist(id) {
  return request.delete(`/blacklist/${id}`)
}

// 获取违规记录列表
export function getViolations(params) {
  return request.get('/violations', { params })
}

// 获取违规记录列表（别名，兼容新版组件调用）
export const getViolationList = getViolations

// === 排班管理 ===

// 获取排班列表
export function getScheduleList(params) {
  return request.get('/schedules', { params })
}

// 创建排班
export function createSchedule(data) {
  return request.post('/schedules', data)
}

// 更新排班
export function updateSchedule(id, data) {
  return request.put(`/schedules/${id}`, data)
}

// 删除排班
export function deleteSchedule(id) {
  return request.delete(`/schedules/${id}`)
}

// === 审计日志 ===

// 获取审计日志
export function getAuditLogs(params) {
  return request.get('/audit-logs', { params })
}

// === 数据统计 ===

// 获取仪表盘统计数据
export function getDashboardStats() {
  return request.get('/admin/dashboard')
}
