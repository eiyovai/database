# 校园开放预约与访客管理系统

> **数据库系统课程设计** | 小组成员：范伟程(34520232200939)、韩中信(34520232200973)、崔鲡沣(34520232200924)
> 完整课程报告见 [`课程报告-校园开放预约与访客管理系统.md`](./课程报告-校园开放预约与访客管理系统.md) | 数据库设计文档见 [`database/README.md`](./database/README.md)

#### 📋 项目简介

本系统为校园一体化访客管理数据库系统，整合了校园开放预约、访客权限管控、校园活动报名、客流容量控制、安全审计追溯、**违规检测与黑名单自动化**等核心功能。系统面向**校外访客**、**校园管理员**、**安保人员**、**志愿工作人员**四类角色，实现访客入校全流程数字化、规范化、智能化管理。

## 🏗️ 系统架构

```
┌─────────────────────────────────────────────┐
│         前端 Vue 3.5 + Element Plus 2.9      │
│  访客端 (5页) │ 管理后台 (11页) │ 安保端 (4页) │
└──────────────────┬──────────────────────────┘
                   │ HTTP (Axios 1.7)
┌──────────────────▼──────────────────────────┐
│      后端 ASP.NET Core 8.0 Web API           │
│  7 Controller + 5 Service + AutoMapper      │
│  JWT + RBAC + BCrypt.Net-Next + EF Core     │
└──────────────────┬──────────────────────────┘
                   │ Entity Framework Core 8.0
┌──────────────────▼──────────────────────────┐
│        SQL Server 2019 (16表 + 1触发器)       │
│  预约表 / 用户表 / 区域表 / 违规记录 / ...    │
└─────────────────────────────────────────────┘
```

## 🚀 快速开始

### 前置要求

| 软件       | 版本  | 说明       |
| ---------- | ----- | ---------- |
| SQL Server | 2019+ | 数据库引擎 |
| .NET SDK   | 8.0   | 后端运行时 |
| Node.js    | 18+   | 前端运行时 |

### 第一步：初始化数据库

> **推荐使用 `00-full-backup.sql` 一键部署（建库+建表+索引+种子数据）**

在 SSMS 中打开 `database/00-full-backup.sql`，按 `F5` 执行即可。或使用命令行：

```bash
sqlcmd -S localhost -E -i database/00-full-backup.sql
```

### 第二步：启动后端

```bash
cd backend
dotnet run --urls "http://localhost:5000"
```

> 启动时自动：创建触发器 `trg_Reservations_StatusChange` + 将种子密码 `CHANGE_ME` 重哈希为 BCrypt

### 第三步：启动前端

```bash
cd frontend
npm install
npm run dev
```

> 默认运行在 `http://localhost:5173`，Vite 自动代理 API 请求到 `http://localhost:5000`

### 第四步：访问系统

打开浏览器访问 `http://localhost:5173`

## 👤 测试账号

所有账号密码统一为：**`123456`**（首次启动自动从 `CHANGE_ME` 重哈希为 BCrypt）

| 角色          | 手机号          | 说明                  |
| ------------- | --------------- | --------------------- |
| 👑 管理员     | `13800138000` | 后台管理、审核、配置  |
| 🧑 张三       | `13800138001` | 访客                  |
| 🧑 李四       | `13800138002` | 访客                  |
| 🧑 王五       | `13800138003` | 访客                  |
| 🧑 赵六       | `13800138004` | 访客（⚠️ 黑名单）   |
| 🧑 孙七       | `13800138005` | 访客                  |
| 🛡️ 安保张三 | `13900139001` | 入校核验、离校登记    |
| 🛡️ 安保李四 | `13900139002` | 入校核验、离校登记    |
| 🎤 讲解员小王 | `13700137001` | 工作人员              |
| 🎤 志愿者小刘 | `13700137002` | 工作人员              |

> ⚠️ 赵六已被系统自动拉黑（累计爽约+超时滞留），登录时返回 401 "您已被列入黑名单"。

## 📁 项目结构

```
DataBase/
├── README.md                          # 本文件
├── GITHUB_GUIDE.md                    # GitHub 操作指南
├── 课程报告-校园开放预约与访客管理系统.md  # 课程设计报告
│
├── frontend/                          # Vue 3 前端
│   ├── src/
│   │   ├── api/                       # API 接口层 (7 个模块)
│   │   ├── stores/auth.js             # Pinia 状态管理
│   │   ├── router/index.js            # 路由 + 角色守卫
│   │   ├── components/                # 3 套布局组件
│   │   └── views/                     # 20+ 页面
│   │       ├── auth/                  # 登录/注册
│   │       ├── visitor/               # 访客端
│   │       ├── admin/                 # 管理后台
│   │       └── security/              # 安保工作站
│   └── package.json
│
├── backend/                           # ASP.NET Core Web API
│   ├── Models/                        # 16 个实体模型
│   ├── Data/CampusVisitorDbContext.cs # EF Core 上下文
│   ├── DTOs/                          # 7 组数据传输对象
│   ├── Services/                      # 5 个业务 Service
│   ├── Controllers/                   # 7 个 API 控制器
│   ├── Mappings/MappingProfile.cs     # AutoMapper 配置
│   └── Program.cs                     # 启动 + DI + 触发器初始化
│
├── database/                          # 数据库脚本
│   ├── 00-full-backup.sql             # 一键部署（推荐）
│   ├── 01-init.sql                    # 建表 + 约束 + 索引 + 触发器
│   ├── 02-seed-data.sql               # 种子数据
│   └── README.md                      # 数据库设计文档
│
└── images/                            # 24 张运行截图
```

## 🗄️ 数据库设计

共 **16 张表**，26+ 外键约束，8 个 UNIQUE，1 个触发器。

| 表名                  | 说明                 | 核心字段                                           |
| --------------------- | -------------------- | -------------------------------------------------- |
| Users                 | 用户表               | Phone(UNIQUE), Role(CHECK), PasswordHash           |
| Visitors              | 访客扩展             | VisitorType(CHECK), EmergencyContact               |
| Reservations          | **预约核心表** | Status(pending→...→checked_out/no_show)          |
| ReservationStatusLog  | 状态日志             | 触发器自动写入                                     |
| CampusAreas           | 校园区域             | MorningStart~AfternoonEnd, AccessLevel             |
| AreaPermissions       | 区域权限             | UNIQUE(AreaId,VisitorType)                         |
| Gates                 | 校门                 | Code(UNIQUE)                                       |
| OpenRules             | 开放规则             | AreaId(FK→CampusAreas), MorningStart~AfternoonEnd |
| Activities            | 活动                 | CurrentCount(冗余), EndTime→过期自动closed        |
| ActivityRegistrations | 活动报名             | UNIQUE(ActivityId,UserId)                          |
| EntryExitRecords      | 出入记录             | EntryTime/ExitTime, EntryGate/ExitGate             |
| ViolationRecords      | 违规记录             | SourceType(system/report/manual), SourceId         |
| Blacklist             | 黑名单               | ExpiresAt, IsActive, ViolationCount                |
| Reports               | 举报                 | Evidence, ReviewerId                               |
| StaffSchedules        | 排班                 | UNIQUE(StaffId,WorkDate,Shift)                     |
| AuditLogs             | 审计日志             | BIGINT IDENTITY, ActionType(CHECK)                 |

### 核心业务流

```
预约提交 → 审核 → 入校核验 → 在校参观 → 离校登记
                ↓                          ↓
           爽约检测 ←─── AutoDetectViolationsAsync ───→ 超时滞留检测
                ↓                          ↓
          ViolationRecord ←── 举报审核通过 ──→ ViolationRecord
                ↓
      累计 ≥3 次 → 自动拉黑 Blacklist → 登录401拦截 + 预约拒绝
```

## 🔌 API 接口

| 方法   | 端点                                         | 说明                     | 权限   |
| ------ | -------------------------------------------- | ------------------------ | ------ |
| POST   | `/api/auth/login`                          | 登录（含黑名单检查）     | 公开   |
| POST   | `/api/auth/register`                       | 注册                     | 公开   |
| GET    | `/api/auth/userinfo`                       | 用户信息                 | 登录   |
| PUT    | `/api/auth/changepwd`                      | 修改密码                 | 登录   |
| POST   | `/api/reservations`                        | 提交预约（含黑名单检查） | 访客   |
| GET    | `/api/reservations/my`                     | 我的预约                 | 访客   |
| PUT    | `/api/reservations/{id}/cancel`            | 取消预约                 | 访客   |
| GET    | `/api/reservations`                        | 全部预约列表             | 管理员 |
| PUT    | `/api/reservations/{id}/review`            | 审核预约                 | 管理员 |
| GET    | `/api/activities/public`                   | 公开活动（自动过期关闭） | 公开   |
| GET    | `/api/activities/{id}`                     | 活动详情                 | 登录   |
| POST   | `/api/activities/register`                 | 报名活动                 | 登录   |
| GET    | `/api/activities/my-registrations`         | 我的报名                 | 登录   |
| PUT    | `/api/activities/{id}/cancel-registration` | 取消报名                 | 登录   |
| GET    | `/api/activities`                          | 活动管理列表             | 管理员 |
| POST   | `/api/activities`                          | 创建活动                 | 管理员 |
| PUT    | `/api/activities/{id}`                     | 更新活动                 | 管理员 |
| DELETE | `/api/activities/{id}`                     | 删除活动                 | 管理员 |
| PUT    | `/api/activities/checkin/{id}`             | 活动签到                 | 管理员 |
| POST   | `/api/entry-exit/search`                   | 搜索预约                 | 安保   |
| POST   | `/api/entry-exit/entry`                    | 确认入校                 | 安保   |
| PUT    | `/api/entry-exit/exit`                     | 登记离校                 | 安保   |
| GET    | `/api/entry-exit/current`                  | 在校访客列表             | 安保   |
| GET    | `/api/entry-exit/stats`                    | 今日核验统计             | 安保   |
| GET    | `/api/entry-exit/recent`                   | 最近入校记录             | 安保   |
| GET    | `/api/admin/dashboard`                     | 数据看板（触发违规检测） | 管理员 |
| GET    | `/api/open-rules`                          | 开放规则列表             | 管理员 |
| POST   | `/api/open-rules`                          | 创建/更新规则            | 管理员 |
| DELETE | `/api/open-rules/{id}`                     | 删除规则                 | 管理员 |
| GET    | `/api/areas`                               | 区域列表                 | 管理员 |
| POST   | `/api/areas`                               | 创建/更新区域            | 管理员 |
| DELETE | `/api/areas/{id}`                          | 删除区域                 | 管理员 |
| GET    | `/api/blacklist`                           | 黑名单列表               | 管理员 |
| DELETE | `/api/blacklist/{id}`                      | 移出黑名单               | 管理员 |
| GET    | `/api/violations`                          | 违规记录列表             | 管理员 |
| GET    | `/api/reports`                             | 举报列表                 | 管理员 |
| PUT    | `/api/reports/{id}/review`                 | 审核举报                 | 管理员 |
| GET    | `/api/schedules`                           | 排班列表                 | 管理员 |
| POST   | `/api/schedules`                           | 创建排班                 | 管理员 |
| PUT    | `/api/schedules/{id}`                      | 更新排班                 | 管理员 |
| DELETE | `/api/schedules/{id}`                      | 删除排班                 | 管理员 |
| GET    | `/api/audit-logs`                          | 审计日志                 | 管理员 |
| GET    | `/api/public/campus-status`                | 校园状态                 | 公开   |

## 🛠️ 技术栈

| 层     | 技术              | 版本    |
| ------ | ----------------- | ------- |
| 数据库 | SQL Server        | 2019    |
| 后端   | ASP.NET Core      | 8.0.11  |
| ORM    | EF Core SqlServer | 8.0.11  |
| 认证   | JWT Bearer        | 8.0.11  |
| 密码   | BCrypt.Net-Next   | 4.0.3   |
| 映射   | AutoMapper        | 12.0.1  |
| 前端   | Vue 3             | 3.5.13  |
| UI     | Element Plus      | 2.9.0   |
| 构建   | Vite              | 6.1.0   |
| HTTP   | Axios             | 1.7.0   |
| 路由   | Vue Router        | 4.5.0   |
| 状态   | Pinia             | 3.0.0   |
| 图表   | ECharts           | 5.6.0   |
| 日期   | Day.js            | 1.11.13 |

## 📝 课程作业说明

本项目的完整源代码和数据库脚本已包含在此仓库中。小组各成员可：

1. **克隆仓库**到本地
2. 按上述步骤**初始化数据库**
3. **启动后端 + 前端** 即可运行

数据库中已包含演示数据，可直接展示系统功能。
