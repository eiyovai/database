import request from './request'

// 搜索预约（安保核验查询）
export function searchReservation(data) {
  return request.post('/entry-exit/search', data)
}

// 确认入校（安保）
export function confirmEntry(data) {
  return request.post('/entry-exit/entry', data)
}

// 离校登记（安保）
export function exitRecord(data) {
  return request.put('/entry-exit/exit', data)
}

// 获取在校访客列表
export function getCurrentVisitors(params) {
  return request.get('/entry-exit/current', { params })
}

// 获取最近入校记录
export function getRecentEntries(params) {
  return request.get('/entry-exit/recent', { params })
}
