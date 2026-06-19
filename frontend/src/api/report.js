import request from './request'

// 提交违规举报
export function submitReport(data) {
  return request.post('/reports', data)
}

// 获取举报列表（管理员/安保）
export function getReportList(params) {
  return request.get('/reports', { params })
}

// 审核举报
export function reviewReport(id, data) {
  return request.put(`/reports/${id}/review`, data)
}
