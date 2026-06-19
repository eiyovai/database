-- ============================================================
-- Docker 环境下数据库初始化脚本
-- 在 SQL Server 容器首次启动时自动执行
-- ============================================================

-- 创建数据库（如果不存在）
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'CampusVisitorDB')
BEGIN
    CREATE DATABASE [CampusVisitorDB]
END
GO

USE [CampusVisitorDB]
GO

-- ============================================================
-- 建表（16张表）
-- ============================================================

-- 1. Users
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]'))
BEGIN
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
END
GO

-- 2. Visitors
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Visitors]'))
BEGIN
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
END
GO

-- 3. CampusAreas
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CampusAreas]'))
BEGIN
CREATE TABLE [dbo].[CampusAreas] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL,
    [Code] VARCHAR(20) NOT NULL,
    [Type] VARCHAR(20) NOT NULL DEFAULT 'public' CHECK ([Type] IN ('public','academic','office','living','lab','restricted')),
    [AccessLevel] VARCHAR(20) NOT NULL DEFAULT 'public' CHECK ([AccessLevel] IN ('public','restricted','forbidden')),
    [Description] NVARCHAR(500) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [UQ_CampusAreas_Code] UNIQUE ([Code])
)
END
GO

-- 4. AreaPermissions
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AreaPermissions]'))
BEGIN
CREATE TABLE [dbo].[AreaPermissions] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [AreaId] INT NOT NULL,
    [VisitorType] VARCHAR(20) NOT NULL CHECK ([VisitorType] IN ('parent','alumni','tourist','study_group','partner')),
    CONSTRAINT [FK_AP_Areas] FOREIGN KEY ([AreaId]) REFERENCES [dbo].[CampusAreas]([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_AP_AreaType] UNIQUE ([AreaId],[VisitorType])
)
END
GO

-- 5. Gates
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Gates]'))
BEGIN
CREATE TABLE [dbo].[Gates] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Name] NVARCHAR(50) NOT NULL,
    [Code] VARCHAR(20) NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [UQ_Gates_Code] UNIQUE ([Code])
)
END
GO

-- 6. OpenRules
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OpenRules]'))
BEGIN
CREATE TABLE [dbo].[OpenRules] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
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
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETDATE()
)
END
GO

-- 7. Reservations
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reservations]'))
BEGIN
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
END
GO

-- 8. ReservationStatusLog
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReservationStatusLog]'))
BEGIN
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
END
GO

-- 9. Activities
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Activities]'))
BEGIN
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
END
GO

-- 10. ActivityRegistrations
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActivityRegistrations]'))
BEGIN
CREATE TABLE [dbo].[ActivityRegistrations] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [ActivityId] INT NOT NULL,
    [UserId] INT NOT NULL,
    [VisitorName] NVARCHAR(50) NOT NULL,
    [VisitorPhone] VARCHAR(20) NOT NULL,
    [Companions] INT NOT NULL DEFAULT 0,
    [Status] VARCHAR(20) NOT NULL DEFAULT 'registered' CHECK ([Status] IN ('registered','cancelled','checked_in','absent')),
    [CheckedInAt] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_AR_Activities] FOREIGN KEY ([ActivityId]) REFERENCES [dbo].[Activities]([Id]),
    CONSTRAINT [FK_AR_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [UQ_AR_ActivityUser] UNIQUE ([ActivityId],[UserId])
)
END
GO

-- 11. EntryExitRecords
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EntryExitRecords]'))
BEGIN
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
END
GO

-- 12. ViolationRecords
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ViolationRecords]'))
BEGIN
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
END
GO

-- 13. Blacklist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Blacklist]'))
BEGIN
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
END
GO

-- 14. Reports
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reports]'))
BEGIN
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
END
GO

-- 15. StaffSchedules
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StaffSchedules]'))
BEGIN
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
END
GO

-- 16. AuditLogs
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]'))
BEGIN
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
END
GO

-- ============================================================
-- 插入测试数据
-- ============================================================

-- 1. 用户
IF NOT EXISTS (SELECT 1 FROM [dbo].[Users])
BEGIN
INSERT INTO [dbo].[Users] ([Name],[Phone],[PasswordHash],[Role],[IsActive]) VALUES (N'系统管理员','13800138000','hash123','admin',1)
INSERT INTO [dbo].[Users] ([Name],[Phone],[PasswordHash],[Role],[IsActive]) VALUES (N'张三','13800138001','hash123','visitor',1)
INSERT INTO [dbo].[Users] ([Name],[Phone],[PasswordHash],[Role],[IsActive]) VALUES (N'李四','13800138002','hash123','visitor',1)
INSERT INTO [dbo].[Users] ([Name],[Phone],[PasswordHash],[Role],[IsActive]) VALUES (N'王五','13800138003','hash123','visitor',1)
INSERT INTO [dbo].[Users] ([Name],[Phone],[PasswordHash],[Role],[IsActive]) VALUES (N'赵六','13800138004','hash123','visitor',1)
INSERT INTO [dbo].[Users] ([Name],[Phone],[PasswordHash],[Role],[IsActive]) VALUES (N'孙七','13800138005','hash123','visitor',1)
INSERT INTO [dbo].[Users] ([Name],[Phone],[PasswordHash],[Role],[IsActive]) VALUES (N'安保张三','13900139001','hash123','security',1)
INSERT INTO [dbo].[Users] ([Name],[Phone],[PasswordHash],[Role],[IsActive]) VALUES (N'安保李四','13900139002','hash123','security',1)
INSERT INTO [dbo].[Users] ([Name],[Phone],[PasswordHash],[Role],[IsActive]) VALUES (N'讲解员小王','13700137001','hash123','staff',1)
INSERT INTO [dbo].[Users] ([Name],[Phone],[PasswordHash],[Role],[IsActive]) VALUES (N'志愿者小刘','13700137002','hash123','staff',1)
END
GO

-- 2. 访客
IF NOT EXISTS (SELECT 1 FROM [dbo].[Visitors])
BEGIN
INSERT INTO [dbo].[Visitors] ([UserId],[IdCard],[Gender],[VisitorType],[Affiliation],[Remarks]) VALUES (2,'110101199001011234','男','tourist',NULL,N'普通游客')
INSERT INTO [dbo].[Visitors] ([UserId],[IdCard],[Gender],[VisitorType],[Affiliation],[Remarks]) VALUES (3,'110101199002021235','男','alumni',N'计算机学院2012级',N'校友返校')
INSERT INTO [dbo].[Visitors] ([UserId],[IdCard],[Gender],[VisitorType],[Affiliation],[Remarks]) VALUES (4,'110101199103031236','女','parent',N'学生家长',N'参加家长会')
INSERT INTO [dbo].[Visitors] ([UserId],[IdCard],[Gender],[VisitorType],[Affiliation],[Remarks]) VALUES (5,'110101199204041237','男','study_group',N'光明中学',N'带队教师')
INSERT INTO [dbo].[Visitors] ([UserId],[IdCard],[Gender],[VisitorType],[Affiliation],[Remarks]) VALUES (6,'110101199305051238','女','partner',N'华为技术有限公司',N'合作洽谈')
END
GO

-- 3. 校门
IF NOT EXISTS (SELECT 1 FROM [dbo].[Gates])
BEGIN
INSERT INTO [dbo].[Gates] ([Name],[Code],[IsActive]) VALUES (N'南门（正门）','SOUTH',1)
INSERT INTO [dbo].[Gates] ([Name],[Code],[IsActive]) VALUES (N'北门','NORTH',1)
INSERT INTO [dbo].[Gates] ([Name],[Code],[IsActive]) VALUES (N'东门','EAST',1)
INSERT INTO [dbo].[Gates] ([Name],[Code],[IsActive]) VALUES (N'西门','WEST',0)
END
GO

-- 其余表的建表和数据参见 database/00-full-backup.sql
-- Docker 构建时实际使用的是 00-full-backup.sql（完整的建库+建表+数据脚本）
-- 此文件保留作为建表部分的参考
GO

PRINT 'Docker 环境数据库初始化完成！'
GO
