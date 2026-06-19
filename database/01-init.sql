-- ============================================================
-- 校园开放预约与访客管理系统 - 数据库建表脚本
-- 目标数据库: SQL Server (适用 SSMS 执行)
-- ============================================================

-- 创建数据库
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'CampusVisitorDB')
BEGIN
    CREATE DATABASE [CampusVisitorDB]
END
GO

USE [CampusVisitorDB]
GO

-- ============================================================
-- 1. 用户表 (Users)
--    统一管理所有角色: 访客、管理员、安保人员、工作人员
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]'))
BEGIN
CREATE TABLE [dbo].[Users] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [Name]          NVARCHAR(50)    NOT NULL,                       -- 姓名
    [Phone]         VARCHAR(20)     NOT NULL,                       -- 手机号（登录账号）
    [PasswordHash]  VARCHAR(256)    NOT NULL,                       -- 密码哈希
    [Role]          VARCHAR(20)     NOT NULL DEFAULT 'visitor'      -- 角色: visitor/admin/security/staff
        CHECK ([Role] IN ('visitor', 'admin', 'security', 'staff')),
    [Email]         VARCHAR(100)    NULL,                           -- 邮箱
    [IsActive]      BIT             NOT NULL DEFAULT 1,             -- 是否激活
    [CreatedAt]     DATETIME2       NOT NULL DEFAULT GETDATE(),     -- 注册时间
    [UpdatedAt]     DATETIME2       NOT NULL DEFAULT GETDATE(),     -- 更新时间
    [LastLoginAt]   DATETIME2       NULL,                           -- 最后登录时间

    CONSTRAINT [UQ_Users_Phone] UNIQUE ([Phone])
)
END
GO

-- ============================================================
-- 2. 访客信息扩展表 (Visitors)
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Visitors]'))
BEGIN
CREATE TABLE [dbo].[Visitors] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [UserId]        INT             NOT NULL,                       -- 关联用户
    [IdCard]        VARCHAR(20)     NULL,                           -- 身份证号
    [Gender]        VARCHAR(4)      NULL CHECK ([Gender] IN ('男', '女', '其他')),
    [BirthDate]     DATE            NULL,                           -- 出生日期
    [Affiliation]   NVARCHAR(100)   NULL,                           -- 所属单位
    [VisitorType]   VARCHAR(20)     NOT NULL DEFAULT 'tourist'      -- 访客类型
        CHECK ([VisitorType] IN ('parent', 'alumni', 'tourist', 'study_group', 'partner')),
    [EmergencyContact]  VARCHAR(20) NULL,                           -- 紧急联系人
    [EmergencyPhone]    VARCHAR(20) NULL,                           -- 紧急联系电话
    [Remarks]       NVARCHAR(500)   NULL,                           -- 备注

    CONSTRAINT [FK_Visitors_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id])
)
END
GO

-- ============================================================
-- 3. 校园区域表 (CampusAreas)
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CampusAreas]'))
BEGIN
CREATE TABLE [dbo].[CampusAreas] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [Name]          NVARCHAR(100)   NOT NULL,                       -- 区域名称
    [Code]          VARCHAR(20)     NOT NULL,                       -- 区域编码
    [Type]          VARCHAR(20)     NOT NULL DEFAULT 'public'       -- 区域类型
        CHECK ([Type] IN ('public', 'academic', 'office', 'living', 'lab', 'restricted')),
    [AccessLevel]   VARCHAR(20)     NOT NULL DEFAULT 'public'       -- 开放等级
        CHECK ([AccessLevel] IN ('public', 'restricted', 'forbidden')),
    [Description]   NVARCHAR(500)   NULL,                           -- 区域描述
    [IsActive]      BIT             NOT NULL DEFAULT 1,             -- 是否启用
    [CreatedAt]     DATETIME2       NOT NULL DEFAULT GETDATE(),

    CONSTRAINT [UQ_CampusAreas_Code] UNIQUE ([Code])
)
END
GO

-- ============================================================
-- 4. 区域权限表 (AreaPermissions)
--    访客类型与区域的多对多关联
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AreaPermissions]'))
BEGIN
CREATE TABLE [dbo].[AreaPermissions] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [AreaId]        INT             NOT NULL,                       -- 区域ID
    [VisitorType]   VARCHAR(20)     NOT NULL                        -- 允许的访客类型
        CHECK ([VisitorType] IN ('parent', 'alumni', 'tourist', 'study_group', 'partner')),

    CONSTRAINT [FK_AP_Areas] FOREIGN KEY ([AreaId]) REFERENCES [dbo].[CampusAreas]([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_AP_AreaType] UNIQUE ([AreaId], [VisitorType])
)
END
GO

-- ============================================================
-- 5. 校门表 (Gates)
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Gates]'))
BEGIN
CREATE TABLE [dbo].[Gates] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [Name]          NVARCHAR(50)    NOT NULL,                       -- 校门名称
    [Code]          VARCHAR(20)     NOT NULL,                       -- 校门编码
    [IsActive]      BIT             NOT NULL DEFAULT 1,             -- 是否启用

    CONSTRAINT [UQ_Gates_Code] UNIQUE ([Code])
)
END
GO

-- ============================================================
-- 6. 开放规则表 (OpenRules)
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OpenRules]'))
BEGIN
CREATE TABLE [dbo].[OpenRules] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [DateType]      VARCHAR(20)     NOT NULL                        -- 日期类型
        CHECK ([DateType] IN ('weekday', 'weekend', 'holiday', 'exam', 'custom')),
    [StartDate]     DATE            NOT NULL,                       -- 开始日期
    [EndDate]       DATE            NOT NULL,                       -- 结束日期
    [TimeSlot]      VARCHAR(20)     NOT NULL                        -- 开放时段
        CHECK ([TimeSlot] IN ('morning', 'afternoon', 'full_day')),
    [MorningStart]  TIME            NULL DEFAULT '08:00',           -- 上午开始
    [MorningEnd]    TIME            NULL DEFAULT '12:00',           -- 上午结束
    [AfternoonStart] TIME           NULL DEFAULT '12:00',           -- 下午开始
    [AfternoonEnd]  TIME            NULL DEFAULT '17:00',           -- 下午结束
    [MaxCapacity]   INT             NOT NULL DEFAULT 500,           -- 最大接待人数
    [IsActive]      BIT             NOT NULL DEFAULT 1,             -- 是否启用
    [Remark]        NVARCHAR(500)   NULL,                           -- 备注
    [CreatedAt]     DATETIME2       NOT NULL DEFAULT GETDATE(),
    [UpdatedAt]     DATETIME2       NOT NULL DEFAULT GETDATE()
)
END
GO

-- ============================================================
-- 7. 预约申请表 (Reservations) - 核心表
--    状态流转: pending -> approved -> checked_in -> checked_out
--                       -> rejected / cancelled / no_show
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reservations]'))
BEGIN
CREATE TABLE [dbo].[Reservations] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [UserId]        INT             NOT NULL,                       -- 预约用户
    [ReservationNo] VARCHAR(30)     NOT NULL,                       -- 预约编号 (R+日期+序号)
    [VisitorType]   VARCHAR(20)     NOT NULL                        -- 访客类型
        CHECK ([VisitorType] IN ('parent', 'alumni', 'tourist', 'study_group', 'partner')),
    [VisitorName]   NVARCHAR(50)    NOT NULL,                       -- 访客姓名
    [VisitorPhone]  VARCHAR(20)     NOT NULL,                       -- 访客手机号
    [VisitDate]     DATE            NOT NULL,                       -- 参观日期
    [TimeSlot]      VARCHAR(20)     NOT NULL                        -- 预约时段
        CHECK ([TimeSlot] IN ('morning', 'afternoon', 'full_day')),
    [Companions]    INT             NOT NULL DEFAULT 0,             -- 同行人数
    [StayDuration]  VARCHAR(20)     NULL,                           -- 预计停留
    [Purpose]       NVARCHAR(500)   NOT NULL,                       -- 入校事由
    [Status]        VARCHAR(20)     NOT NULL DEFAULT 'pending'      -- 预约状态
        CHECK ([Status] IN ('pending', 'approved', 'rejected', 'cancelled',
                            'checked_in', 'checked_out', 'no_show')),
    [ReviewerId]    INT             NULL,                           -- 审核人ID
    [ReviewRemark]  NVARCHAR(500)   NULL,                           -- 审核备注
    [ReviewedAt]    DATETIME2       NULL,                           -- 审核时间
    [CreatedAt]     DATETIME2       NOT NULL DEFAULT GETDATE(),
    [UpdatedAt]     DATETIME2       NOT NULL DEFAULT GETDATE(),

    CONSTRAINT [FK_Reservations_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_Reservations_Reviewer] FOREIGN KEY ([ReviewerId]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [UQ_Reservations_No] UNIQUE ([ReservationNo])
)
END
GO

-- ============================================================
-- 8. 预约状态变更日志表 (ReservationStatusLog)
--    全程记录状态流转轨迹，实现可追溯
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReservationStatusLog]'))
BEGIN
CREATE TABLE [dbo].[ReservationStatusLog] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [ReservationId] INT             NOT NULL,                       -- 预约ID
    [FromStatus]    VARCHAR(20)     NULL,                           -- 原状态
    [ToStatus]      VARCHAR(20)     NOT NULL,                       -- 新状态
    [OperatorId]    INT             NULL,                           -- 操作人
    [Remark]        NVARCHAR(500)   NULL,                           -- 备注
    [CreatedAt]     DATETIME2       NOT NULL DEFAULT GETDATE(),

    CONSTRAINT [FK_RSL_Reservations] FOREIGN KEY ([ReservationId])
        REFERENCES [dbo].[Reservations]([Id]),
    CONSTRAINT [FK_RSL_Operator] FOREIGN KEY ([OperatorId])
        REFERENCES [dbo].[Users]([Id])
)
END
GO

-- ============================================================
-- 9. 活动表 (Activities)
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Activities]'))
BEGIN
CREATE TABLE [dbo].[Activities] (
    [Id]                INT IDENTITY(1,1) PRIMARY KEY,
    [Title]             NVARCHAR(200)   NOT NULL,                   -- 活动标题
    [Location]          NVARCHAR(200)   NOT NULL,                   -- 活动地点
    [Description]       NVARCHAR(2000)  NULL,                       -- 活动描述
    [StartTime]         DATETIME2       NOT NULL,                   -- 开始时间
    [EndTime]           DATETIME2       NOT NULL,                   -- 结束时间
    [MaxParticipants]   INT             NOT NULL DEFAULT 100,       -- 报名人数上限
    [CurrentCount]      INT             NOT NULL DEFAULT 0,         -- 当前报名人数
    [Status]            VARCHAR(20)     NOT NULL DEFAULT 'draft'    -- 状态: draft/open/closed/cancelled
        CHECK ([Status] IN ('draft', 'open', 'closed', 'cancelled')),
    [CoverImage]        VARCHAR(500)    NULL,                       -- 封面图URL
    [ContactPerson]     NVARCHAR(50)    NULL,                       -- 联系人
    [ContactPhone]      VARCHAR(20)    NULL,                        -- 联系电话
    [CreatedBy]         INT             NOT NULL,                   -- 创建人
    [CreatedAt]         DATETIME2       NOT NULL DEFAULT GETDATE(),
    [UpdatedAt]         DATETIME2       NOT NULL DEFAULT GETDATE(),

    CONSTRAINT [FK_Activities_Creator] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users]([Id])
)
END
GO

-- ============================================================
-- 10. 活动报名表 (ActivityRegistrations)
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActivityRegistrations]'))
BEGIN
CREATE TABLE [dbo].[ActivityRegistrations] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [ActivityId]    INT             NOT NULL,                       -- 活动ID
    [UserId]        INT             NOT NULL,                       -- 用户ID
    [VisitorName]   NVARCHAR(50)    NOT NULL,                       -- 报名人姓名
    [VisitorPhone]  VARCHAR(20)     NOT NULL,                       -- 报名人手机号
    [Companions]    INT             NOT NULL DEFAULT 0,             -- 同行人数
    [Status]        VARCHAR(20)     NOT NULL DEFAULT 'registered'   -- 状态: registered/cancelled/checked_in/absent
        CHECK ([Status] IN ('registered', 'cancelled', 'checked_in', 'absent')),
    [CheckedInAt]   DATETIME2       NULL,                           -- 签到时间
    [CreatedAt]     DATETIME2       NOT NULL DEFAULT GETDATE(),

    CONSTRAINT [FK_AR_Activities] FOREIGN KEY ([ActivityId])
        REFERENCES [dbo].[Activities]([Id]),
    CONSTRAINT [FK_AR_Users] FOREIGN KEY ([UserId])
        REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [UQ_AR_ActivityUser] UNIQUE ([ActivityId], [UserId])
)
END
GO

-- ============================================================
-- 11. 出入校记录表 (EntryExitRecords)
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EntryExitRecords]'))
BEGIN
CREATE TABLE [dbo].[EntryExitRecords] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [ReservationId] INT             NOT NULL,                       -- 关联预约
    [UserId]        INT             NOT NULL,                       -- 用户ID
    [EntryTime]     DATETIME2       NULL,                           -- 入校时间
    [EntryGateId]   INT             NULL,                           -- 入校校门
    [ExitTime]      DATETIME2       NULL,                           -- 离校时间
    [ExitGateId]    INT             NULL,                           -- 离校校门
    [OperatorId]    INT             NULL,                           -- 核验操作人（安保）
    [CreatedAt]     DATETIME2       NOT NULL DEFAULT GETDATE(),

    CONSTRAINT [FK_EER_Reservations] FOREIGN KEY ([ReservationId])
        REFERENCES [dbo].[Reservations]([Id]),
    CONSTRAINT [FK_EER_Users] FOREIGN KEY ([UserId])
        REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_EER_EntryGate] FOREIGN KEY ([EntryGateId])
        REFERENCES [dbo].[Gates]([Id]),
    CONSTRAINT [FK_EER_ExitGate] FOREIGN KEY ([ExitGateId])
        REFERENCES [dbo].[Gates]([Id]),
    CONSTRAINT [FK_EER_Operator] FOREIGN KEY ([OperatorId])
        REFERENCES [dbo].[Users]([Id])
)
END
GO

-- ============================================================
-- 12. 违规记录表 (ViolationRecords)
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ViolationRecords]'))
BEGIN
CREATE TABLE [dbo].[ViolationRecords] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [UserId]        INT             NOT NULL,                       -- 违规用户
    [ViolationType] VARCHAR(20)     NOT NULL                        -- 违规类型
        CHECK ([ViolationType] IN ('no_show', 'overstay', 'trespass', 'duplicate',
                                   'absence', 'disturbance', 'damage', 'other')),
    [Description]   NVARCHAR(1000)  NOT NULL,                       -- 违规描述
    [OccurredAt]    DATETIME2       NOT NULL,                       -- 违规时间
    [Location]      NVARCHAR(200)   NULL,                           -- 违规地点
    [Severity]      VARCHAR(10)     NOT NULL DEFAULT 'minor'        -- 严重程度
        CHECK ([Severity] IN ('minor', 'major', 'critical')),
    [SourceType]    VARCHAR(20)     NOT NULL DEFAULT 'system'       -- 来源: system/report/manual
        CHECK ([SourceType] IN ('system', 'report', 'manual')),
    [SourceId]      INT             NULL,                           -- 来源ID（举报ID等）
    [CreatedAt]     DATETIME2       NOT NULL DEFAULT GETDATE(),

    CONSTRAINT [FK_VR_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id])
)
END
GO

-- ============================================================
-- 13. 黑名单表 (Blacklist)
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Blacklist]'))
BEGIN
CREATE TABLE [dbo].[Blacklist] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [UserId]        INT             NOT NULL,                       -- 被拉黑用户
    [Reason]        NVARCHAR(500)   NOT NULL,                       -- 拉黑原因
    [ViolationCount] INT            NOT NULL DEFAULT 0,             -- 累计违规次数
    [BlacklistedAt] DATETIME2       NOT NULL DEFAULT GETDATE(),     -- 拉黑时间
    [ExpiresAt]     DATETIME2       NULL,                           -- 到期时间（NULL=永久）
    [IsActive]      BIT             NOT NULL DEFAULT 1,             -- 是否生效
    [OperatorId]    INT             NOT NULL,                       -- 操作人
    [UnblacklistedAt] DATETIME2     NULL,                           -- 移出时间
    [UnblacklistReason] NVARCHAR(500) NULL,                         -- 移出原因

    CONSTRAINT [FK_BL_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_BL_Operator] FOREIGN KEY ([OperatorId]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [UQ_Blacklist_User] UNIQUE ([UserId])
)
END
GO

-- ============================================================
-- 14. 举报表 (Reports)
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reports]'))
BEGIN
CREATE TABLE [dbo].[Reports] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [ReporterId]    INT             NOT NULL,                       -- 举报人
    [TargetName]    NVARCHAR(100)   NOT NULL,                       -- 被举报人/对象
    [ViolationType] VARCHAR(20)     NOT NULL                        -- 违规类型
        CHECK ([ViolationType] IN ('trespass', 'disturbance', 'damage', 'overstay', 'other')),
    [Location]      NVARCHAR(200)   NOT NULL,                       -- 发生地点
    [OccurredAt]    DATETIME2       NOT NULL,                       -- 发生时间
    [Description]   NVARCHAR(2000)  NOT NULL,                       -- 违规描述
    [Evidence]      NVARCHAR(MAX)   NULL,                           -- 证据材料（图片URL等）
    [Status]        VARCHAR(20)     NOT NULL DEFAULT 'pending'      -- 状态: pending/approved/rejected
        CHECK ([Status] IN ('pending', 'approved', 'rejected')),
    [ReviewerId]    INT             NULL,                           -- 审核人
    [ReviewRemark]  NVARCHAR(500)   NULL,                           -- 审核意见
    [ReviewedAt]    DATETIME2       NULL,                           -- 审核时间
    [CreatedAt]     DATETIME2       NOT NULL DEFAULT GETDATE(),

    CONSTRAINT [FK_Rep_Reporter] FOREIGN KEY ([ReporterId]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_Rep_Reviewer] FOREIGN KEY ([ReviewerId]) REFERENCES [dbo].[Users]([Id])
)
END
GO

-- ============================================================
-- 15. 排班表 (StaffSchedules)
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StaffSchedules]'))
BEGIN
CREATE TABLE [dbo].[StaffSchedules] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [StaffId]       INT             NOT NULL,                       -- 工作人员ID
    [StaffRole]     VARCHAR(20)     NOT NULL                        -- 角色
        CHECK ([StaffRole] IN ('volunteer', 'guide', 'security')),
    [WorkDate]      DATE            NOT NULL,                       -- 工作日期
    [Shift]         VARCHAR(20)     NOT NULL                        -- 班次
        CHECK ([Shift] IN ('morning', 'afternoon', 'evening', 'full_day')),
    [StartTime]     TIME            NOT NULL,                       -- 开始时间
    [EndTime]       TIME            NOT NULL,                       -- 结束时间
    [Location]      NVARCHAR(200)   NOT NULL,                       -- 工作地点
    [Task]          NVARCHAR(500)   NULL,                           -- 工作内容
    [CreatedBy]     INT             NOT NULL,                       -- 排班创建人
    [CreatedAt]     DATETIME2       NOT NULL DEFAULT GETDATE(),
    [UpdatedAt]     DATETIME2       NOT NULL DEFAULT GETDATE(),

    CONSTRAINT [FK_SS_Staff] FOREIGN KEY ([StaffId]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_SS_Creator] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [UQ_SS_StaffDateShift] UNIQUE ([StaffId], [WorkDate], [Shift])
)
END
GO

-- ============================================================
-- 16. 审计日志表 (AuditLogs)
--    全量记录后台操作，实现操作可审计、可追溯
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]'))
BEGIN
CREATE TABLE [dbo].[AuditLogs] (
    [Id]            BIGINT IDENTITY(1,1) PRIMARY KEY,
    [OperatorId]    INT             NULL,                           -- 操作人
    [ActionType]    VARCHAR(30)     NOT NULL                        -- 操作类型
        CHECK ([ActionType] IN ('login', 'review', 'config', 'user_mgmt', 'data_export',
                                'blacklist', 'report', 'system', 'other')),
    [ActionDetail]  NVARCHAR(500)   NOT NULL,                       -- 操作描述
    [TargetType]    VARCHAR(50)     NULL,                           -- 操作对象类型
    [TargetId]      INT             NULL,                           -- 操作对象ID
    [IpAddress]     VARCHAR(50)     NULL,                           -- 请求IP
    [Result]        VARCHAR(10)     NOT NULL DEFAULT 'success'      -- 结果: success/fail
        CHECK ([Result] IN ('success', 'fail')),
    [CreatedAt]     DATETIME2       NOT NULL DEFAULT GETDATE(),

    CONSTRAINT [FK_AL_Operator] FOREIGN KEY ([OperatorId]) REFERENCES [dbo].[Users]([Id])
)
END
GO

-- ============================================================
-- 创建索引 - 提升查询性能
-- ============================================================

-- 预约表索引
CREATE NONCLUSTERED INDEX [IX_Reservations_UserId] ON [dbo].[Reservations]([UserId])
CREATE NONCLUSTERED INDEX [IX_Reservations_Status] ON [dbo].[Reservations]([Status])
CREATE NONCLUSTERED INDEX [IX_Reservations_VisitDate] ON [dbo].[Reservations]([VisitDate])
CREATE NONCLUSTERED INDEX [IX_Reservations_VisitorPhone] ON [dbo].[Reservations]([VisitorPhone])

-- 预约状态日志索引
CREATE NONCLUSTERED INDEX [IX_RSL_ReservationId] ON [dbo].[ReservationStatusLog]([ReservationId])

-- 出入校记录索引
CREATE NONCLUSTERED INDEX [IX_EER_UserId] ON [dbo].[EntryExitRecords]([UserId])
CREATE NONCLUSTERED INDEX [IX_EER_EntryTime] ON [dbo].[EntryExitRecords]([EntryTime])

-- 活动报名索引
CREATE NONCLUSTERED INDEX [IX_AR_ActivityId] ON [dbo].[ActivityRegistrations]([ActivityId])

-- 违规记录索引
CREATE NONCLUSTERED INDEX [IX_VR_UserId] ON [dbo].[ViolationRecords]([UserId])

-- 审计日志索引
CREATE NONCLUSTERED INDEX [IX_AL_OperatorId] ON [dbo].[AuditLogs]([OperatorId])
CREATE NONCLUSTERED INDEX [IX_AL_CreatedAt] ON [dbo].[AuditLogs]([CreatedAt])
CREATE NONCLUSTERED INDEX [IX_AL_ActionType] ON [dbo].[AuditLogs]([ActionType])

-- 举报索引
CREATE NONCLUSTERED INDEX [IX_Rep_Status] ON [dbo].[Reports]([Status])

-- 排班索引
CREATE NONCLUSTERED INDEX [IX_SS_WorkDate] ON [dbo].[StaffSchedules]([WorkDate])

-- ============================================================
-- 创建存储过程: 预约编号生成
-- ============================================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GenerateReservationNo]'))
    DROP FUNCTION [dbo].[GenerateReservationNo]
GO

CREATE FUNCTION [dbo].[GenerateReservationNo]()
RETURNS VARCHAR(30)
AS
BEGIN
    DECLARE @dateStr VARCHAR(10) = FORMAT(GETDATE(), 'yyyyMMdd')
    DECLARE @seq INT

    SELECT @seq = ISNULL(MAX(CAST(RIGHT([ReservationNo], 4) AS INT)), 0) + 1
    FROM [dbo].[Reservations]
    WHERE [ReservationNo] LIKE 'R' + @dateStr + '%'

    RETURN 'R' + @dateStr + RIGHT('0000' + CAST(@seq AS VARCHAR), 4)
END
GO

-- ============================================================
-- 创建触发器: 预约状态变更自动记录日志
-- ============================================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[trg_Reservations_StatusChange]'))
    DROP TRIGGER [dbo].[trg_Reservations_StatusChange]
GO

CREATE TRIGGER [dbo].[trg_Reservations_StatusChange]
ON [dbo].[Reservations]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON

    IF UPDATE([Status])
    BEGIN
        INSERT INTO [dbo].[ReservationStatusLog]
            ([ReservationId], [FromStatus], [ToStatus], [OperatorId], [Remark])
        SELECT
            i.[Id],
            d.[Status],
            i.[Status],
            i.[ReviewerId],
            CASE i.[Status]
                WHEN 'approved'   THEN N'审核通过'
                WHEN 'rejected'   THEN N'审核拒绝: ' + ISNULL(i.[ReviewRemark], N'')
                WHEN 'cancelled'  THEN N'用户取消预约'
                WHEN 'checked_in' THEN N'已入校核验'
                WHEN 'checked_out' THEN N'已离校登记'
                WHEN 'no_show'    THEN N'系统标记爽约'
                ELSE N'状态变更'
            END
        FROM INSERTED i
        INNER JOIN DELETED d ON i.[Id] = d.[Id]
    END
END
GO

PRINT '数据库初始化完成！'
GO
