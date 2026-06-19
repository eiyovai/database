import request from './request'

// 入校核验（安保）
export function entryCheck(data) {
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

// 获取出入校记录
export function getEntryExitRecords(params) {
  return request.get('/entry-exit/records', { params })
}
