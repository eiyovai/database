# 校园开放预约与访客管理系统

> **课程作业** — 数据库系统课程设计

#### 📋 项目简介

本系统为校园一体化访客管理数据库系统，整合了校园开放预约、访客权限管控、校园活动报名、客流容量控制、安全审计追溯等核心功能，解决传统校园访客登记零散、客流管控无序、活动管理混乱等问题。

系统面向 **校外访客**、**校园管理员**、**安保人员**、**志愿工作人员** 等多角色，实现访客入校全流程数字化、规范化、智能化管理。

## 👥 小组成员

- 范伟程
- 韩中信
- 崔鲡沣

## 🏗️ 系统架构

```
┌─────────────────────────────────────────────┐
│            前端 (Vue 3 + Element Plus)        │
│  访客预约端  │  管理员后台  │  安保工作站      │
└──────────────────┬──────────────────────────┘
                   │ HTTP (Axios)
┌──────────────────▼──────────────────────────┐
│        后端 (ASP.NET Core Web API)            │
│  Auth / Reservation / Activity / EntryExit   │
│  Report / Admin 等 6 大控制器 + 5 个 Service │
└──────────────────┬──────────────────────────┘
                   │ Entity Framework Core
┌──────────────────▼──────────────────────────┐
│          SQL Server 2019 (16张表)             │
│  预约表 / 用户表 / 区域表 / 活动表 / ...     │
└─────────────────────────────────────────────┘
```

## 🚀 快速开始

### 🐳 方式一：Docker Compose（推荐，无需手动安装任何依赖）

**前置要求：** 安装 [Docker Desktop](https://www.docker.com/products/docker-desktop/)

**一条命令启动全部服务：**

```bash
docker compose up -d
```

等待约 1-2 分钟（SQL Server 首次启动需初始化），然后打开浏览器访问 `http://localhost`。

Docker 会自动完成：
- ✅ 启动 SQL Server 2022
- ✅ 创建 16 张数据库表
- ✅ 插入测试数据
- ✅ 启动后端 API（端口 5000）
- ✅ 启动前端页面（端口 80）

**停止服务：**
```bash
docker compose down
```

**删除数据重新开始：**
```bash
docker compose down -v
```

### 📦 方式二：本地手动部署

### 第一步：初始化数据库

**方式一：使用 SSMS（推荐）**

1. 打开 SSMS，连接到你的 SQL Server
2. 打开 `database/01-init.sql`，按 `F5` 执行（创建数据库和 16 张表）
3. 打开 `database/02-seed-data.sql`，按 `F5` 执行（插入测试数据）

**方式二：使用命令行**

```bash
sqlcmd -S localhost -E -i database/01-init.sql
sqlcmd -S localhost -E -i database/02-seed-data.sql
```

### 第二步：启动后端 API

```bash
cd backend
dotnet restore
dotnet run --urls "http://localhost:5000"
```

> 后端启动后会自动连接 SQL Server，监听 `http://localhost:5000`

### 第三步：启动前端

```bash
cd frontend
npm install
npm run dev
```

> 前端默认运行在 `http://localhost:5173`，自动代理 API 请求到后端

### 第四步：访问系统

打开浏览器访问 `http://localhost:5173`

## 👤 测试账号

所有账号密码统一为：**`123456`**

| 角色                   | 手机号          | 说明                 |
| ---------------------- | --------------- | -------------------- |
| 👑**管理员**     | `13800138000` | 后台管理、审核、配置 |
| 🧑**访客张三**   | `13800138001` | 可预约、报名活动     |
| 🧑**访客李四**   | `13800138002` | 可预约、报名活动     |
| 🧑**访客王五**   | `13800138003` | 可预约、报名活动     |
| 🧑**访客赵六**   | `13800138004` | 可预约、报名活动     |
| 🧑**访客孙七**   | `13800138005` | 可预约、报名活动     |
| 🛡️**安保张三** | `13900139001` | 入校核验、离校登记   |
| 🛡️**安保李四** | `13900139002` | 入校核验、离校登记   |
| 🎤**讲解员小王** | `13700137001` | 排班管理             |
| 🎤**志愿者小刘** | `13700137002` | 排班管理             |

## 📁 项目结构

```
DataBase/
├── README.md                     # 本文件
├── .gitignore
│
├── frontend/                     # Vue 3 前端
│   ├── src/
│   │   ├── api/                  # API 接口层
│   │   ├── stores/               # 状态管理 (Pinia)
│   │   ├── router/               # 路由配置
│   │   ├── components/           # 布局组件
│   │   └── views/
│   │       ├── auth/             # 登录/注册
│   │       ├── visitor/          # 访客端
│   │       ├── admin/            # 管理后台
│   │       └── security/         # 安保工作站
│   └── package.json
│
├── backend/                      # ASP.NET Core Web API
│   ├── Models/                   # 16个实体模型
│   ├── Data/                     # DbContext
│   ├── DTOs/                     # 数据传输对象
│   ├── Services/                 # 业务逻辑层
│   ├── Controllers/              # API 控制器
│   └── Program.cs                # 启动配置
│
└── database/                     # 数据库脚本
    ├── 01-init.sql               # 建库建表
    ├── 02-seed-data.sql          # 测试数据
    └── README.md                 # 数据库设计文档
```

## 🗄️ 数据库设计

共 **16 张表**：

| 表名                  | 说明                         |
| --------------------- | ---------------------------- |
| Users                 | 用户表（所有角色）           |
| Visitors              | 访客信息扩展                 |
| Reservations          | **预约申请表（核心）** |
| ReservationStatusLog  | 预约状态变更日志             |
| CampusAreas           | 校园区域                     |
| AreaPermissions       | 区域权限                     |
| Gates                 | 校门                         |
| OpenRules             | 开放规则                     |
| Activities            | 活动                         |
| ActivityRegistrations | 活动报名                     |
| EntryExitRecords      | 出入校记录                   |
| ViolationRecords      | 违规记录                     |
| Blacklist             | 黑名单                       |
| Reports               | 举报                         |
| StaffSchedules        | 排班                         |
| AuditLogs             | 审计日志                     |

### 预约状态流转

```
提交 → 待审核 → 审核通过 → 入校核验 → 离校登记
                ↘ 审核拒绝                ↘ 爽约标记
                ↘ 用户取消
```

## 🔌 API 接口一览

| 方法 | 端点                              | 说明     | 权限   |
| ---- | --------------------------------- | -------- | ------ |
| POST | `/api/auth/login`               | 登录     | 公开   |
| POST | `/api/auth/register`            | 注册     | 公开   |
| GET  | `/api/auth/userinfo`            | 用户信息 | 登录   |
| POST | `/api/reservations`             | 提交预约 | 访客   |
| GET  | `/api/reservations/my`          | 我的预约 | 访客   |
| GET  | `/api/reservations`             | 预约列表 | 管理员 |
| PUT  | `/api/reservations/{id}/review` | 审核预约 | 管理员 |
| GET  | `/api/activities/public`        | 公开活动 | 公开   |
| POST | `/api/activities/register`      | 报名活动 | 登录   |
| POST | `/api/entry-exit/entry`         | 入校核验 | 安保   |
| PUT  | `/api/entry-exit/exit`          | 离校登记 | 安保   |
| GET  | `/api/admin/dashboard`          | 数据看板 | 管理员 |
| GET  | `/api/open-rules`               | 开放规则 | 管理员 |
| GET  | `/api/audit-logs`               | 审计日志 | 管理员 |

## 🛠️ 技术栈

| 层面                | 技术                      |
| ------------------- | ------------------------- |
| **前端框架**  | Vue 3 + Vite              |
| **UI 组件库** | Element Plus              |
| **状态管理**  | Pinia                     |
| **后端框架**  | ASP.NET Core 8.0          |
| **ORM**       | Entity Framework Core     |
| **数据库**    | SQL Server 2019           |
| **认证**      | JWT (BCrypt 密码加密)     |
| **包管理**    | npm (前端) / NuGet (后端) |

## 📝 课程作业说明

本项目的完整源代码和数据库脚本已包含在此仓库中。小组各成员可：

1. **克隆仓库**到本地
2. 按上述步骤**初始化数据库**
3. **启动后端 + 前端** 即可运行

数据库中已包含演示数据，可直接展示系统功能。
