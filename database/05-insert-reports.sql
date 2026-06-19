INSERT INTO CampusVisitorDB.dbo.Reports
    (ReporterId, TargetName, ViolationType, Location, OccurredAt, Description, Status, CreatedAt)
VALUES
    (1, '张三', 'trespass', N'理工楼B座3楼', '2026-06-18 14:20', N'有非授权人员试图进入封闭实验室区域', 'pending', '2026-06-18T14:25:00'),
    (1, '李四', 'noise', N'图书馆2楼自习区', '2026-06-19 09:15', N'在安静自习区大声喧哗，经提醒仍不改正', 'pending', '2026-06-19T09:20:00'),
    (1, '王五', 'damage', N'体育馆更衣室', '2026-06-17 16:30', N'故意损坏更衣柜门锁', 'pending', '2026-06-17T16:45:00'),
    (1, '赵六', 'harassment', N'食堂一楼', '2026-06-19 11:00', N'多次骚扰其他就餐同学，有言语冲突', 'pending', '2026-06-19T11:10:00');

SELECT 'Inserted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows' AS Result;
