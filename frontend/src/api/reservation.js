import request from './request'

// 提交预约申请
export function createReservation(data) {
  return request.post('/reservations', data)
}

// 获取我的预约列表
export function getMyReservations(params) {
  return request.get('/reservations/my', { params })
}

// 获取预约详情
export function getReservationDetail(id) {
  return request.get(`/reservations/${id}`)
}

// 取消预约
export function cancelReservation(id) {
  return request.put(`/reservations/${id}/cancel`)
}

// === 管理员接口 ===

// 获取所有预约列表（分页+筛选）
export function getReservationList(params) {
  return request.get('/reservations', { params })
}

// 审核预约
export function reviewReservation(id, data) {
  return request.put(`/reservations/${id}/review`, data)
}

// 获取预约统计（复用dashboard接口）
export function getReservationStats(params) {
  return request.get('/admin/dashboard', { params })
}
