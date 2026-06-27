-- ============================================================
-- 校园开放预约与访客管理系统 - 完整数据库备份脚本
-- 适用: 在目标电脑上一次性创建数据库并导入全部数据
-- 用法: 在 SSMS 中打开此文件，按 F5 执行
-- ============================================================

-- 创建数据库
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'CampusVisitorDB')
BEGIN
    ALTER DATABASE [CampusVisitorDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
    DROP DATABASE [CampusVisitorDB]
END
GO

CREATE DATABASE [CampusVisitorDB]
GO

USE [CampusVisitorDB]
GO

-- ============================================================
-- 以下内容由 01-init.sql + 02-seed-data.sql 合并生成
-- ============================================================

-- 1. Users 用户表
CREATE TABLE [dbo].[Users] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Name] NVARCHAR(50) NOT NULL,
    [Phone] VARCHAR(20) NOT NULL,
    [PasswordHash] VARCHAR(256) NOT NULL,
    [Role] VARCHAR(20) NOT NULL DEFAULT 'visitor' CHECK ([Role] IN ('visitor','admin','security','staff')),
    [Email] VARCHAR(100) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [LastLoginAt] DATETIME2 NULL,
    CONSTRAINT [UQ_Users_Phone] UNIQUE ([Phone])
)
GO

-- 2. Visitors 访客信息扩展表
CREATE TABLE [dbo].[Visitors] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [UserId] INT NOT NULL,
    [IdCard] VARCHAR(20) NULL,
    [Gender] VARCHAR(4) NULL CHECK ([Gender] IN ('男','女','其他')),
    [BirthDate] DATE NULL,
    [Affiliation] NVARCHAR(100) NULL,
    [VisitorType] VARCHAR(20) NOT NULL DEFAULT 'tourist' CHECK ([VisitorType] IN ('parent','alumni','tourist','study_group','partner')),
    [EmergencyContact] NVARCHAR(50) NULL,
    [EmergencyPhone] VARCHAR(20) NULL,
    [Remarks] NVARCHAR(500) NULL,
    CONSTRAINT [FK_Visitors_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id])
)
GO

-- 3. CampusAreas 校园区域表
CREATE TABLE [dbo].[CampusAreas] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL,
    [Code] VARCHAR(20) NOT NULL,
    [Type] VARCHAR(20) NOT NULL DEFAULT 'public' CHECK ([Type] IN ('public','academic','office','living','lab','restricted')),
    [AccessLevel] VARCHAR(20) NOT NULL DEFAULT 'public' CHECK ([AccessLevel] IN ('public','restricted','forbidden')),
    [Description] NVARCHAR(500) NULL,
    [MorningStart] TIME NULL,
    [MorningEnd] TIME NULL,
    [AfternoonStart] TIME NULL,
    [AfternoonEnd] TIME NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [UQ_CampusAreas_Code] UNIQUE ([Code])
)
GO

-- 4. AreaPermissions 区域权限表
CREATE TABLE [dbo].[AreaPermissions] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [AreaId] INT NOT NULL,
    [VisitorType] VARCHAR(20) NOT NULL CHECK ([VisitorType] IN ('parent','alumni','tourist','study_group','partner')),
    CONSTRAINT [FK_AP_Areas] FOREIGN KEY ([AreaId]) REFERENCES [dbo].[CampusAreas]([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_AP_AreaType] UNIQUE ([AreaId],[VisitorType])
)
GO

-- 5. Gates 校门表
CREATE TABLE [dbo].[Gates] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Name] NVARCHAR(50) NOT NULL,
    [Code] VARCHAR(20) NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [UQ_Gates_Code] UNIQUE ([Code])
)
GO

-- 6. OpenRules 开放规则表
CREATE TABLE [dbo].[OpenRules] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [AreaId] INT NULL,
    [DateType] VARCHAR(20) NOT NULL CHECK ([DateType] IN ('weekday','weekend','holiday','exam','custom')),
    [StartDate] DATE NOT NULL,
    [EndDate] DATE NOT NULL,
    [TimeSlot] VARCHAR(20) NOT NULL CHECK ([TimeSlot] IN ('morning','afternoon','full_day')),
    [MorningStart] TIME NULL DEFAULT '08:00',
    [MorningEnd] TIME NULL DEFAULT '12:00',
    [AfternoonStart] TIME NULL DEFAULT '12:00',
    [AfternoonEnd] TIME NULL DEFAULT '17:00',
    [MaxCapacity] INT NOT NULL DEFAULT 500,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [Remark] NVARCHAR(500) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_OpenRules_Areas] FOREIGN KEY ([AreaId]) REFERENCES [dbo].[CampusAreas]([Id]) ON DELETE SET NULL
)
GO

-- 7. Reservations 预约申请表（核心）
CREATE TABLE [dbo].[Reservations] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [UserId] INT NOT NULL,
    [ReservationNo] VARCHAR(30) NOT NULL,
    [VisitorType] VARCHAR(20) NOT NULL CHECK ([VisitorType] IN ('parent','alumni','tourist','study_group','partner')),
    [VisitorName] NVARCHAR(50) NOT NULL,
    [VisitorPhone] VARCHAR(20) NOT NULL,
    [VisitDate] DATE NOT NULL,
    [TimeSlot] VARCHAR(20) NOT NULL CHECK ([TimeSlot] IN ('morning','afternoon','full_day')),
    [Companions] INT NOT NULL DEFAULT 0,
    [StayDuration] VARCHAR(20) NULL,
    [Purpose] NVARCHAR(500) NOT NULL,
    [Status] VARCHAR(20) NOT NULL DEFAULT 'pending' CHECK ([Status] IN ('pending','approved','rejected','cancelled','checked_in','checked_out','no_show')),
    [ReviewerId] INT NULL,
    [ReviewRemark] NVARCHAR(500) NULL,
    [ReviewedAt] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_Reservations_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_Reservations_Reviewer] FOREIGN KEY ([ReviewerId]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [UQ_Reservations_No] UNIQUE ([ReservationNo])
)
GO

-- 8. ReservationStatusLog 预约状态变更日志
CREATE TABLE [dbo].[ReservationStatusLog] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [ReservationId] INT NOT NULL,
    [FromStatus] VARCHAR(20) NULL,
    [ToStatus] VARCHAR(20) NOT NULL,
    [OperatorId] INT NULL,
    [Remark] NVARCHAR(500) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_RSL_Reservations] FOREIGN KEY ([ReservationId]) REFERENCES [dbo].[Reservations]([Id]),
    CONSTRAINT [FK_RSL_Operator] FOREIGN KEY ([OperatorId]) REFERENCES [dbo].[Users]([Id])
)
GO

-- 9. Activities 活动表
CREATE TABLE [dbo].[Activities] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Title] NVARCHAR(200) NOT NULL,
    [Location] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(2000) NULL,
    [StartTime] DATETIME2 NOT NULL,
    [EndTime] DATETIME2 NOT NULL,
    [MaxParticipants] INT NOT NULL DEFAULT 100,
    [CurrentCount] INT NOT NULL DEFAULT 0,
    [Status] VARCHAR(20) NOT NULL DEFAULT 'draft' CHECK ([Status] IN ('draft','open','closed','cancelled')),
    [CoverImage] VARCHAR(500) NULL,
    [ContactPerson] NVARCHAR(50) NULL,
    [ContactPhone] VARCHAR(20) NULL,
    [CreatedBy] INT NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_Activities_Creator] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users]([Id])
)
GO

-- 10. ActivityRegistrations 活动报名表
CREATE TABLE [dbo].[ActivityRegistrations] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [ActivityId] INT NOT NULL,
    [UserId] INT NOT NULL,
    [VisitorName] NVARCHAR(50) NOT NULL,
    [VisitorPhone] VARCHAR(20) NOT NULL,
    [Companions] INT NOT NULL DEFAULT 0,
    [Status] VARCHAR(20) NOT NULL DEFAULT 'pending' CHECK ([Status] IN ('pending','registered','rejected','cancelled','checked_in','absent')),
    [CheckedInAt] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_AR_Activities] FOREIGN KEY ([ActivityId]) REFERENCES [dbo].[Activities]([Id]),
    CONSTRAINT [FK_AR_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [UQ_AR_ActivityUser] UNIQUE ([ActivityId],[UserId])
)
GO

-- 11. EntryExitRecords 出入校记录表
CREATE TABLE [dbo].[EntryExitRecords] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [ReservationId] INT NOT NULL,
    [UserId] INT NOT NULL,
    [EntryTime] DATETIME2 NULL,
    [EntryGateId] INT NULL,
    [ExitTime] DATETIME2 NULL,
    [ExitGateId] INT NULL,
    [OperatorId] INT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_EER_Reservations] FOREIGN KEY ([ReservationId]) REFERENCES [dbo].[Reservations]([Id]),
    CONSTRAINT [FK_EER_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_EER_EntryGate] FOREIGN KEY ([EntryGateId]) REFERENCES [dbo].[Gates]([Id]),
    CONSTRAINT [FK_EER_ExitGate] FOREIGN KEY ([ExitGateId]) REFERENCES [dbo].[Gates]([Id]),
    CONSTRAINT [FK_EER_Operator] FOREIGN KEY ([OperatorId]) REFERENCES [dbo].[Users]([Id])
)
GO

-- 12. ViolationRecords 违规记录表
CREATE TABLE [dbo].[ViolationRecords] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [UserId] INT NOT NULL,
    [ViolationType] VARCHAR(20) NOT NULL CHECK ([ViolationType] IN ('no_show','overstay','trespass','duplicate','absence','disturbance','damage','other')),
    [Description] NVARCHAR(1000) NOT NULL,
    [OccurredAt] DATETIME2 NOT NULL,
    [Location] NVARCHAR(200) NULL,
    [Severity] VARCHAR(10) NOT NULL DEFAULT 'minor' CHECK ([Severity] IN ('minor','major','critical')),
    [SourceType] VARCHAR(20) NOT NULL DEFAULT 'system' CHECK ([SourceType] IN ('system','report','manual')),
    [SourceId] INT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_VR_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id])
)
GO

-- 13. Blacklist 黑名单表
CREATE TABLE [dbo].[Blacklist] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [UserId] INT NOT NULL,
    [Reason] NVARCHAR(500) NOT NULL,
    [ViolationCount] INT NOT NULL DEFAULT 0,
    [BlacklistedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [ExpiresAt] DATETIME2 NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [OperatorId] INT NOT NULL,
    [UnblacklistedAt] DATETIME2 NULL,
    [UnblacklistReason] NVARCHAR(500) NULL,
    CONSTRAINT [FK_BL_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_BL_Operator] FOREIGN KEY ([OperatorId]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [UQ_Blacklist_User] UNIQUE ([UserId])
)
GO

-- 14. Reports 举报表
CREATE TABLE [dbo].[Reports] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [ReporterId] INT NOT NULL,
    [TargetName] NVARCHAR(100) NOT NULL,
    [ViolationType] VARCHAR(20) NOT NULL CHECK ([ViolationType] IN ('trespass','disturbance','damage','overstay','other')),
    [Location] NVARCHAR(200) NOT NULL,
    [OccurredAt] DATETIME2 NOT NULL,
    [Description] NVARCHAR(2000) NOT NULL,
    [Evidence] NVARCHAR(MAX) NULL,
    [Status] VARCHAR(20) NOT NULL DEFAULT 'pending' CHECK ([Status] IN ('pending','approved','rejected')),
    [ReviewerId] INT NULL,
    [ReviewRemark] NVARCHAR(500) NULL,
    [ReviewedAt] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_Rep_Reporter] FOREIGN KEY ([ReporterId]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_Rep_Reviewer] FOREIGN KEY ([ReviewerId]) REFERENCES [dbo].[Users]([Id])
)
GO

-- 15. StaffSchedules 排班表
CREATE TABLE [dbo].[StaffSchedules] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [StaffId] INT NOT NULL,
    [StaffRole] VARCHAR(20) NOT NULL CHECK ([StaffRole] IN ('volunteer','guide','security')),
    [WorkDate] DATE NOT NULL,
    [Shift] VARCHAR(20) NOT NULL CHECK ([Shift] IN ('morning','afternoon','evening','full_day')),
    [StartTime] TIME NOT NULL,
    [EndTime] TIME NOT NULL,
    [Location] NVARCHAR(200) NOT NULL,
    [Task] NVARCHAR(500) NULL,
    [CreatedBy] INT NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_SS_Staff] FOREIGN KEY ([StaffId]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_SS_Creator] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [UQ_SS_StaffDateShift] UNIQUE ([StaffId],[WorkDate],[Shift])
)
GO

-- 16. AuditLogs 审计日志表
CREATE TABLE [dbo].[AuditLogs] (
    [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
    [OperatorId] INT NULL,
    [ActionType] VARCHAR(30) NOT NULL CHECK ([ActionType] IN ('login','review','config','user_mgmt','data_export','blacklist','report','system','other')),
    [ActionDetail] NVARCHAR(500) NOT NULL,
    [TargetType] VARCHAR(50) NULL,
    [TargetId] INT NULL,
    [IpAddress] VARCHAR(50) NULL,
    [Result] VARCHAR(10) NOT NULL DEFAULT 'success' CHECK ([Result] IN ('success','fail')),
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_AL_Operator] FOREIGN KEY ([OperatorId]) REFERENCES [dbo].[Users]([Id])
)
GO

-- ============================================================
-- 索引
-- ============================================================
CREATE NONCLUSTERED INDEX [IX_Reservations_UserId] ON [dbo].[Reservations]([UserId])
CREATE NONCLUSTERED INDEX [IX_Reservations_Status] ON [dbo].[Reservations]([Status])
CREATE NONCLUSTERED INDEX [IX_Reservations_VisitDate] ON [dbo].[Reservations]([VisitDate])
CREATE NONCLUSTERED INDEX [IX_RSL_ReservationId] ON [dbo].[ReservationStatusLog]([ReservationId])
CREATE NONCLUSTERED INDEX [IX_EER_UserId] ON [dbo].[EntryExitRecords]([UserId])
CREATE NONCLUSTERED INDEX [IX_EER_EntryTime] ON [dbo].[EntryExitRecords]([EntryTime])
CREATE NONCLUSTERED INDEX [IX_AR_ActivityId] ON [dbo].[ActivityRegistrations]([ActivityId])
CREATE NONCLUSTERED INDEX [IX_VR_UserId] ON [dbo].[ViolationRecords]([UserId])
CREATE NONCLUSTERED INDEX [IX_AL_OperatorId] ON [dbo].[AuditLogs]([OperatorId])
CREATE NONCLUSTERED INDEX [IX_AL_CreatedAt] ON [dbo].[AuditLogs]([CreatedAt])
CREATE NONCLUSTERED INDEX [IX_AL_ActionType] ON [dbo].[AuditLogs]([ActionType])
CREATE NONCLUSTERED INDEX [IX_Rep_Status] ON [dbo].[Reports]([Status])
CREATE NONCLUSTERED INDEX [IX_SS_WorkDate] ON [dbo].[StaffSchedules]([WorkDate])
GO

-- ============================================================
-- 触发器：预约状态变更自动记录日志
-- ============================================================
CREATE TRIGGER [dbo].[trg_Reservations_StatusChange]
ON [dbo].[Reservations]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON
    IF UPDATE([Status])
    BEGIN
        INSERT INTO [dbo].[ReservationStatusLog] ([ReservationId],[FromStatus],[ToStatus],[OperatorId],[Remark])
        SELECT i.[Id], d.[Status], i.[Status], i.[ReviewerId],
            CASE i.[Status]
                WHEN 'approved' THEN N'审核通过'
                WHEN 'rejected' THEN N'审核拒绝: ' + ISNULL(i.[ReviewRemark], N'')
                WHEN 'cancelled' THEN N'用户取消预约'
                WHEN 'checked_in' THEN N'已入校核验'
                WHEN 'checked_out' THEN N'已离校登记'
                WHEN 'no_show' THEN N'系统标记爽约'
                ELSE N'状态变更'
            END
        FROM INSERTED i INNER JOIN DELETED d ON i.[Id] = d.[Id]
    END
END
GO

-- ============================================================
-- 种子数据
-- ============================================================

-- 1. 用户 (10条)
INSERT INTO [dbo].[Users] ([Name],[Phone],[PasswordHash],[Role],[IsActive]) VALUES (N'系统管理员','13800138000','CHANGE_ME','admin',1)
INSERT INTO [dbo].[Users] ([Name],[Phone],[PasswordHash],[Role],[IsActive]) VALUES (N'张三','13800138001','CHANGE_ME','visitor',1)
INSERT INTO [dbo].[Users] ([Name],[Phone],[PasswordHash],[Role],[IsActive]) VALUES (N'李四','13800138002','CHANGE_ME','visitor',1)
INSERT INTO [dbo].[Users] ([Name],[Phone],[PasswordHash],[Role],[IsActive]) VALUES (N'王五','13800138003','CHANGE_ME','visitor',1)
INSERT INTO [dbo].[Users] ([Name],[Phone],[PasswordHash],[Role],[IsActive]) VALUES (N'赵六','13800138004','CHANGE_ME','visitor',1)
INSERT INTO [dbo].[Users] ([Name],[Phone],[PasswordHash],[Role],[IsActive]) VALUES (N'孙七','13800138005','CHANGE_ME','visitor',1)
INSERT INTO [dbo].[Users] ([Name],[Phone],[PasswordHash],[Role],[IsActive]) VALUES (N'安保张三','13900139001','CHANGE_ME','security',1)
INSERT INTO [dbo].[Users] ([Name],[Phone],[PasswordHash],[Role],[IsActive]) VALUES (N'安保李四','13900139002','CHANGE_ME','security',1)
INSERT INTO [dbo].[Users] ([Name],[Phone],[PasswordHash],[Role],[IsActive]) VALUES (N'讲解员小王','13700137001','CHANGE_ME','staff',1)
INSERT INTO [dbo].[Users] ([Name],[Phone],[PasswordHash],[Role],[IsActive]) VALUES (N'志愿者小刘','13700137002','CHANGE_ME','staff',1)
GO

-- 2. 访客 (5条)
INSERT INTO [dbo].[Visitors] ([UserId],[IdCard],[Gender],[VisitorType],[Affiliation],[Remarks]) VALUES (2,'110101199001011234','男','tourist',NULL,N'普通游客')
INSERT INTO [dbo].[Visitors] ([UserId],[IdCard],[Gender],[VisitorType],[Affiliation],[Remarks]) VALUES (3,'110101199002021235','男','alumni',N'计算机学院2012级',N'校友返校')
INSERT INTO [dbo].[Visitors] ([UserId],[IdCard],[Gender],[VisitorType],[Affiliation],[Remarks]) VALUES (4,'110101199103031236','女','parent',N'学生家长',N'参加家长会')
INSERT INTO [dbo].[Visitors] ([UserId],[IdCard],[Gender],[VisitorType],[Affiliation],[Remarks]) VALUES (5,'110101199204041237','男','study_group',N'光明中学',N'带队教师')
INSERT INTO [dbo].[Visitors] ([UserId],[IdCard],[Gender],[VisitorType],[Affiliation],[Remarks]) VALUES (6,'110101199305051238','女','partner',N'华为技术有限公司',N'合作洽谈')
GO

-- 3. 校门 (4条)
INSERT INTO [dbo].[Gates] ([Name],[Code],[IsActive]) VALUES (N'南门（正门）','SOUTH',1)
INSERT INTO [dbo].[Gates] ([Name],[Code],[IsActive]) VALUES (N'北门','NORTH',1)
INSERT INTO [dbo].[Gates] ([Name],[Code],[IsActive]) VALUES (N'东门','EAST',1)
INSERT INTO [dbo].[Gates] ([Name],[Code],[IsActive]) VALUES (N'西门','WEST',0)
GO

-- 4. 校园区域 (9条)
INSERT INTO [dbo].[CampusAreas] ([Name],[Code],[Type],[AccessLevel],[Description]) VALUES (N'校门广场','A01','public','public',N'校园入口区域')
INSERT INTO [dbo].[CampusAreas] ([Name],[Code],[Type],[AccessLevel],[Description]) VALUES (N'主干道','A02','public','public',N'校园主要道路')
INSERT INTO [dbo].[CampusAreas] ([Name],[Code],[Type],[AccessLevel],[Description]) VALUES (N'操场/体育馆','A03','public','public',N'体育运动场所')
INSERT INTO [dbo].[CampusAreas] ([Name],[Code],[Type],[AccessLevel],[Description]) VALUES (N'图书馆','B01','academic','public',N'图书馆公共区域')
INSERT INTO [dbo].[CampusAreas] ([Name],[Code],[Type],[AccessLevel],[Description]) VALUES (N'校史馆','B02','public','restricted',N'需预约或校友身份进入')
INSERT INTO [dbo].[CampusAreas] ([Name],[Code],[Type],[AccessLevel],[Description]) VALUES (N'理工楼实验室','C01','lab','restricted',N'实验室展示区')
INSERT INTO [dbo].[CampusAreas] ([Name],[Code],[Type],[AccessLevel],[Description]) VALUES (N'行政办公楼','D01','office','restricted',N'办公区域')
INSERT INTO [dbo].[CampusAreas] ([Name],[Code],[Type],[AccessLevel],[Description]) VALUES (N'学生宿舍区','E01','living','forbidden',N'住宿区域')
INSERT INTO [dbo].[CampusAreas] ([Name],[Code],[Type],[AccessLevel],[Description]) VALUES (N'科研实验楼','C02','lab','forbidden',N'核心科研区域')
GO

-- 5. 区域权限 (5条)
INSERT INTO [dbo].[AreaPermissions] ([AreaId],[VisitorType]) VALUES (5,'alumni'),(5,'study_group'),(6,'study_group'),(6,'partner'),(7,'partner')
GO

-- 6. 开放规则 (4条)
INSERT INTO [dbo].[OpenRules] ([DateType],[StartDate],[EndDate],[TimeSlot],[MaxCapacity],[IsActive],[Remark]) VALUES ('weekday','2026-06-01','2026-07-31','full_day',500,1,N'暑假工作日全天开放')
INSERT INTO [dbo].[OpenRules] ([DateType],[StartDate],[EndDate],[TimeSlot],[MaxCapacity],[IsActive],[Remark]) VALUES ('weekend','2026-06-01','2026-07-31','full_day',800,1,N'周末开放容量增加')
INSERT INTO [dbo].[OpenRules] ([DateType],[StartDate],[EndDate],[TimeSlot],[MaxCapacity],[IsActive],[Remark]) VALUES ('exam','2026-06-20','2026-06-28','afternoon',200,1,N'考试周限流')
INSERT INTO [dbo].[OpenRules] ([DateType],[StartDate],[EndDate],[TimeSlot],[MaxCapacity],[IsActive],[Remark]) VALUES ('holiday','2026-07-01','2026-07-02','full_day',300,0,N'暂未启用')
GO

-- 7. 预约 (8条)
INSERT INTO [dbo].[Reservations] ([UserId],[ReservationNo],[VisitorType],[VisitorName],[VisitorPhone],[VisitDate],[TimeSlot],[Companions],[StayDuration],[Purpose],[Status],[ReviewerId],[ReviewRemark],[ReviewedAt],[CreatedAt]) VALUES (2,'R202606190001','tourist',N'张三','13800138001','2026-06-22','morning',2,'4h',N'带孩子参观校园','approved',1,N'审核通过','2026-06-19 09:00','2026-06-18 14:30')
INSERT INTO [dbo].[Reservations] ([UserId],[ReservationNo],[VisitorType],[VisitorName],[VisitorPhone],[VisitDate],[TimeSlot],[Companions],[StayDuration],[Purpose],[Status],[ReviewerId],[ReviewRemark],[ReviewedAt],[CreatedAt]) VALUES (3,'R202606190002','alumni',N'李四','13800138002','2026-06-23','afternoon',0,'half_day',N'回母校参观校史馆','pending',NULL,NULL,NULL,'2026-06-19 10:00')
INSERT INTO [dbo].[Reservations] ([UserId],[ReservationNo],[VisitorType],[VisitorName],[VisitorPhone],[VisitDate],[TimeSlot],[Companions],[StayDuration],[Purpose],[Status],[ReviewerId],[ReviewRemark],[ReviewedAt],[CreatedAt]) VALUES (4,'R202606190003','parent',N'王五','13800138003','2026-06-22','morning',1,'2h',N'参加家长开放日','approved',1,N'审核通过','2026-06-19 09:30','2026-06-18 16:00')
INSERT INTO [dbo].[Reservations] ([UserId],[ReservationNo],[VisitorType],[VisitorName],[VisitorPhone],[VisitDate],[TimeSlot],[Companions],[StayDuration],[Purpose],[Status],[ReviewerId],[ReviewRemark],[ReviewedAt],[CreatedAt]) VALUES (5,'R202606190004','study_group',N'赵六','13800138004','2026-06-24','full_day',15,'full_day',N'带领研学团队参观实验室','pending',NULL,NULL,NULL,'2026-06-19 11:00')
INSERT INTO [dbo].[Reservations] ([UserId],[ReservationNo],[VisitorType],[VisitorName],[VisitorPhone],[VisitDate],[TimeSlot],[Companions],[StayDuration],[Purpose],[Status],[ReviewerId],[ReviewRemark],[ReviewedAt],[CreatedAt]) VALUES (6,'R202606190005','partner',N'孙七','13800138005','2026-06-22','morning',2,'4h',N'与合作单位洽谈合作','approved',1,N'审核通过','2026-06-19 08:00','2026-06-17 09:00')
INSERT INTO [dbo].[Reservations] ([UserId],[ReservationNo],[VisitorType],[VisitorName],[VisitorPhone],[VisitDate],[TimeSlot],[Companions],[StayDuration],[Purpose],[Status],[ReviewerId],[ReviewRemark],[ReviewedAt],[CreatedAt]) VALUES (2,'R202606190006','tourist',N'张三','13800138001','2026-06-20','afternoon',0,'2h',N'临时有事取消参观','cancelled',1,NULL,'2026-06-19 10:00','2026-06-19 08:00')
INSERT INTO [dbo].[Reservations] ([UserId],[ReservationNo],[VisitorType],[VisitorName],[VisitorPhone],[VisitDate],[TimeSlot],[Companions],[StayDuration],[Purpose],[Status],[ReviewerId],[ReviewRemark],[ReviewedAt],[CreatedAt]) VALUES (3,'R202606190007','alumni',N'李四','13800138002','2026-06-19','morning',0,'half_day',N'校友返校日','checked_in',1,N'审核通过','2026-06-18 09:00','2026-06-17 10:00')
INSERT INTO [dbo].[Reservations] ([UserId],[ReservationNo],[VisitorType],[VisitorName],[VisitorPhone],[VisitDate],[TimeSlot],[Companions],[StayDuration],[Purpose],[Status],[ReviewerId],[ReviewRemark],[ReviewedAt],[CreatedAt]) VALUES (4,'R202606190008','parent',N'王五','13800138003','2026-06-18','morning',1,'2h',N'正常参观离校','checked_out',1,N'审核通过','2026-06-17 09:00','2026-06-16 14:00')
GO

-- 8. 状态变更日志 (9条)
INSERT INTO [dbo].[ReservationStatusLog] ([ReservationId],[FromStatus],[ToStatus],[OperatorId],[Remark],[CreatedAt]) VALUES (1,'pending','approved',1,N'审核通过','2026-06-19 09:00')
INSERT INTO [dbo].[ReservationStatusLog] ([ReservationId],[FromStatus],[ToStatus],[OperatorId],[Remark],[CreatedAt]) VALUES (3,'pending','approved',1,N'审核通过','2026-06-19 09:30')
INSERT INTO [dbo].[ReservationStatusLog] ([ReservationId],[FromStatus],[ToStatus],[OperatorId],[Remark],[CreatedAt]) VALUES (5,'pending','approved',1,N'审核通过','2026-06-19 08:00')
INSERT INTO [dbo].[ReservationStatusLog] ([ReservationId],[FromStatus],[ToStatus],[OperatorId],[Remark],[CreatedAt]) VALUES (6,'pending','cancelled',2,N'用户取消预约','2026-06-19 10:00')
INSERT INTO [dbo].[ReservationStatusLog] ([ReservationId],[FromStatus],[ToStatus],[OperatorId],[Remark],[CreatedAt]) VALUES (7,'pending','approved',1,N'审核通过','2026-06-18 09:00')
INSERT INTO [dbo].[ReservationStatusLog] ([ReservationId],[FromStatus],[ToStatus],[OperatorId],[Remark],[CreatedAt]) VALUES (7,'approved','checked_in',7,N'南门入校核验','2026-06-19 08:30')
INSERT INTO [dbo].[ReservationStatusLog] ([ReservationId],[FromStatus],[ToStatus],[OperatorId],[Remark],[CreatedAt]) VALUES (8,'pending','approved',1,N'审核通过','2026-06-17 09:00')
INSERT INTO [dbo].[ReservationStatusLog] ([ReservationId],[FromStatus],[ToStatus],[OperatorId],[Remark],[CreatedAt]) VALUES (8,'approved','checked_in',7,N'南门入校核验','2026-06-18 08:15')
INSERT INTO [dbo].[ReservationStatusLog] ([ReservationId],[FromStatus],[ToStatus],[OperatorId],[Remark],[CreatedAt]) VALUES (8,'checked_in','checked_out',7,N'北门离校登记','2026-06-18 11:30')
GO

-- 9. 出入校记录 (2条)
INSERT INTO [dbo].[EntryExitRecords] ([ReservationId],[UserId],[EntryTime],[EntryGateId],[ExitTime],[ExitGateId],[OperatorId]) VALUES (7,3,'2026-06-19 08:30',1,NULL,NULL,7)
INSERT INTO [dbo].[EntryExitRecords] ([ReservationId],[UserId],[EntryTime],[EntryGateId],[ExitTime],[ExitGateId],[OperatorId]) VALUES (8,4,'2026-06-18 08:15',1,'2026-06-18 11:30',2,7)
GO

-- 10. 活动 (4条)
INSERT INTO [dbo].[Activities] ([Title],[Location],[Description],[StartTime],[EndTime],[MaxParticipants],[CurrentCount],[Status],[ContactPerson],[ContactPhone],[CreatedBy]) VALUES (N'校史馆公益讲解',N'校史馆',N'由专业讲解员带领参观校史馆。','2026-06-22 10:00','2026-06-22 11:30',50,0,'open',N'讲解员小王','13700137001',1)
INSERT INTO [dbo].[Activities] ([Title],[Location],[Description],[StartTime],[EndTime],[MaxParticipants],[CurrentCount],[Status],[ContactPerson],[ContactPhone],[CreatedBy]) VALUES (N'实验室开放参观',N'理工楼A座3楼',N'参观最新科研实验室。','2026-06-23 14:00','2026-06-23 16:00',50,0,'open',N'志愿者小刘','13700137002',1)
INSERT INTO [dbo].[Activities] ([Title],[Location],[Description],[StartTime],[EndTime],[MaxParticipants],[CurrentCount],[Status],[ContactPerson],[ContactPhone],[CreatedBy]) VALUES (N'招生宣讲会',N'学术报告厅',N'招生老师解答报考问题。','2026-06-25 09:00','2026-06-25 11:00',200,0,'open',N'系统管理员','13800138000',1)
INSERT INTO [dbo].[Activities] ([Title],[Location],[Description],[StartTime],[EndTime],[MaxParticipants],[CurrentCount],[Status],[ContactPerson],[ContactPhone],[CreatedBy]) VALUES (N'社团展示活动',N'操场',N'各大学生社团展示校园文化。','2026-06-28 09:00','2026-06-28 16:00',500,0,'open',N'系统管理员','13800138000',1)
GO

-- 11. 活动报名 (7条)
INSERT INTO [dbo].[ActivityRegistrations] ([ActivityId],[UserId],[VisitorName],[VisitorPhone],[Companions],[Status]) VALUES (1,2,N'张三','13800138001',2,'registered')
INSERT INTO [dbo].[ActivityRegistrations] ([ActivityId],[UserId],[VisitorName],[VisitorPhone],[Companions],[Status]) VALUES (1,3,N'李四','13800138002',0,'registered')
INSERT INTO [dbo].[ActivityRegistrations] ([ActivityId],[UserId],[VisitorName],[VisitorPhone],[Companions],[Status]) VALUES (2,2,N'张三','13800138001',1,'registered')
INSERT INTO [dbo].[ActivityRegistrations] ([ActivityId],[UserId],[VisitorName],[VisitorPhone],[Companions],[Status]) VALUES (2,4,N'王五','13800138003',0,'registered')
INSERT INTO [dbo].[ActivityRegistrations] ([ActivityId],[UserId],[VisitorName],[VisitorPhone],[Companions],[Status]) VALUES (3,2,N'张三','13800138001',0,'registered')
INSERT INTO [dbo].[ActivityRegistrations] ([ActivityId],[UserId],[VisitorName],[VisitorPhone],[Companions],[Status]) VALUES (3,3,N'李四','13800138002',0,'registered')
INSERT INTO [dbo].[ActivityRegistrations] ([ActivityId],[UserId],[VisitorName],[VisitorPhone],[Companions],[Status]) VALUES (3,5,N'赵六','13800138004',15,'registered')
GO

-- 根据实际报名记录更新 CurrentCount
UPDATE [dbo].[Activities]
SET [CurrentCount] = (
    SELECT COUNT(*) FROM [dbo].[ActivityRegistrations]
    WHERE [ActivityId] = [Activities].[Id] AND [Status] = 'registered'
)
GO

-- 12. 违规记录 (2条)
INSERT INTO [dbo].[ViolationRecords] ([UserId],[ViolationType],[Description],[OccurredAt],[Location],[Severity],[SourceType]) VALUES (2,'no_show',N'预约2026-06-15上午时段未到校','2026-06-15',NULL,'minor','system')
INSERT INTO [dbo].[ViolationRecords] ([UserId],[ViolationType],[Description],[OccurredAt],[Location],[Severity],[SourceType]) VALUES (5,'overstay',N'超时滞留2小时','2026-06-16',N'理工楼','major','manual')
INSERT INTO [dbo].[ViolationRecords] ([UserId],[ViolationType],[Description],[OccurredAt],[Location],[Severity],[SourceType]) VALUES (5,'no_show',N'预约2026-06-10上午时段未到校','2026-06-10',NULL,'minor','system')
INSERT INTO [dbo].[ViolationRecords] ([UserId],[ViolationType],[Description],[OccurredAt],[Location],[Severity],[SourceType]) VALUES (5,'no_show',N'预约2026-06-12下午时段未到校','2026-06-12',NULL,'minor','system')
GO

-- 13. 黑名单 (1条)
INSERT INTO [dbo].[Blacklist] ([UserId],[Reason],[ViolationCount],[BlacklistedAt],[ExpiresAt],[IsActive],[OperatorId]) VALUES (5,N'累计爽约2次+超时滞留1次',3,'2026-06-17 14:00','2026-09-17 14:00',1,1)
GO

-- 14. 举报 (1条)
INSERT INTO [dbo].[Reports] ([ReporterId],[TargetName],[ViolationType],[Location],[OccurredAt],[Description],[Status]) VALUES (1,N'外来人员','trespass',N'理工楼B座3楼','2026-06-18 14:20',N'非校园人员在实验室区域附近徘徊。','pending')
GO

-- 15. 排班 (6条)
INSERT INTO [dbo].[StaffSchedules] ([StaffId],[StaffRole],[WorkDate],[Shift],[StartTime],[EndTime],[Location],[Task],[CreatedBy]) VALUES (9,'guide','2026-06-22','morning','08:00','12:00',N'校史馆',N'校史馆公益讲解',1)
INSERT INTO [dbo].[StaffSchedules] ([StaffId],[StaffRole],[WorkDate],[Shift],[StartTime],[EndTime],[Location],[Task],[CreatedBy]) VALUES (10,'volunteer','2026-06-22','morning','08:00','12:00',N'校门广场',N'访客引导和接待',1)
INSERT INTO [dbo].[StaffSchedules] ([StaffId],[StaffRole],[WorkDate],[Shift],[StartTime],[EndTime],[Location],[Task],[CreatedBy]) VALUES (10,'volunteer','2026-06-22','afternoon','12:00','16:00',N'主干道',N'校园导览',1)
INSERT INTO [dbo].[StaffSchedules] ([StaffId],[StaffRole],[WorkDate],[Shift],[StartTime],[EndTime],[Location],[Task],[CreatedBy]) VALUES (7,'security','2026-06-22','full_day','08:00','17:00',N'南门',N'入校核验及安保',1)
INSERT INTO [dbo].[StaffSchedules] ([StaffId],[StaffRole],[WorkDate],[Shift],[StartTime],[EndTime],[Location],[Task],[CreatedBy]) VALUES (8,'security','2026-06-22','full_day','08:00','17:00',N'北门',N'入校核验及安保',1)
INSERT INTO [dbo].[StaffSchedules] ([StaffId],[StaffRole],[WorkDate],[Shift],[StartTime],[EndTime],[Location],[Task],[CreatedBy]) VALUES (9,'guide','2026-06-23','afternoon','12:00','16:00',N'理工楼',N'实验室讲解',1)
GO

-- 16. 审计日志 (5条)
INSERT INTO [dbo].[AuditLogs] ([OperatorId],[ActionType],[ActionDetail],[TargetType],[TargetId],[IpAddress],[Result],[CreatedAt]) VALUES (1,'review',N'审核通过预约R202606190001','Reservation',1,'192.168.1.100','success','2026-06-19 09:00')
INSERT INTO [dbo].[AuditLogs] ([OperatorId],[ActionType],[ActionDetail],[TargetType],[TargetId],[IpAddress],[Result],[CreatedAt]) VALUES (1,'review',N'审核通过预约R202606190003','Reservation',3,'192.168.1.100','success','2026-06-19 09:30')
INSERT INTO [dbo].[AuditLogs] ([OperatorId],[ActionType],[ActionDetail],[TargetType],[TargetId],[IpAddress],[Result],[CreatedAt]) VALUES (1,'config',N'修改开放规则-周末容量调整为800','OpenRule',2,'192.168.1.100','success','2026-06-18 15:00')
INSERT INTO [dbo].[AuditLogs] ([OperatorId],[ActionType],[ActionDetail],[TargetType],[TargetId],[IpAddress],[Result],[CreatedAt]) VALUES (1,'blacklist',N'将赵六加入黑名单','Blacklist',1,'192.168.1.100','success','2026-06-17 14:00')
INSERT INTO [dbo].[AuditLogs] ([OperatorId],[ActionType],[ActionDetail],[TargetType],[TargetId],[IpAddress],[Result],[CreatedAt]) VALUES (7,'other',N'南门入校核验-张三','EntryExit',7,'192.168.1.101','success','2026-06-19 08:30')
GO

PRINT N'========================================'
PRINT N'数据库 CampusVisitorDB 初始化完成！'
PRINT N'共创建 16 张表，插入种子数据'
PRINT N'========================================'
GO
