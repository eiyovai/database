using CampusVisitorApi.Data;
using CampusVisitorApi.DTOs;
using CampusVisitorApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusVisitorApi.Services;

public interface IEntryExitService
{
    Task<Reservation> SearchAsync(string query);
    Task<EntryExitRecord> EntryAsync(int operatorId, EntryCheckRequest request);
    Task<EntryExitRecord> ExitAsync(int operatorId, ExitRecordRequest request);
    Task<List<EntryExitRecord>> GetCurrentVisitorsAsync();
    Task AutoDetectViolationsAsync();
    Task<List<EntryExitRecord>> GetRecordsAsync(int page, int pageSize);
}

public class EntryExitService : IEntryExitService
{
    private readonly CampusVisitorDbContext _db;

    public EntryExitService(CampusVisitorDbContext db) => _db = db;

    /// <summary>
    /// 自动违规检测：爽约 + 超时滞留（按区域）+ 访问权限违规
    /// </summary>
    public async Task AutoDetectViolationsAsync()
    {
        // 使用本地时间进行比较（开放规则中的时间为本地时间）
        var now = DateTime.Now;
        var today = DateTime.Today;

        // === 1. 爽约检测：预约日期已过，status=approved 但从未来过 ===
        var noShows = await _db.Reservations
            .Where(r => r.Status == "approved" && r.VisitDate < today)
            .ToListAsync();

        foreach (var r in noShows)
        {
            r.Status = "no_show";

            var exists = await _db.ViolationRecords.AnyAsync(v =>
                v.UserId == r.UserId && v.ViolationType == "no_show" && v.SourceId == r.Id);
            if (!exists)
            {
                _db.ViolationRecords.Add(new ViolationRecord
                {
                    UserId = r.UserId,
                    ViolationType = "no_show",
                    Description = $"预约{r.ReservationNo}（{r.VisitDate:yyyy-MM-dd}）未到校",
                    OccurredAt = r.VisitDate,
                    Severity = "minor",
                    SourceType = "system",
                    SourceId = r.Id,
                });
            }
        }

        // === 2. 超时滞留检测：按区域关闭时间逐区检查 ===
        var activeRules = await _db.OpenRules
            .Include(r => r.Area)
            .Where(r => r.IsActive && r.StartDate <= today && r.EndDate >= today)
            .ToListAsync();

        // 按区域去重，取最早的关闭时间；未分配区域的规则按「全校」处理
        var closedAreas = activeRules
            .Select(r => new {
                AreaId = r.AreaId,
                AreaName = r.Area?.Name ?? "全校",
                CloseTime = r.AfternoonEnd ?? new TimeSpan(17, 0, 0),
            })
            .Where(x => now.TimeOfDay > x.CloseTime)
            .GroupBy(x => x.AreaId)
            .Select(g => g.OrderBy(x => x.CloseTime).First())
            .ToList();

        // 如果没有规则，使用校园默认关闭时间 17:00
        if (closedAreas.Count == 0 && now.TimeOfDay > new TimeSpan(17, 0, 0))
        {
            closedAreas.Add(new { AreaId = (int?)null, AreaName = "全校", CloseTime = new TimeSpan(17, 0, 0) });
        }

        foreach (var area in closedAreas)
        {
            var cutoffTime = today.Add(area.CloseTime);
            var overstays = await _db.EntryExitRecords
                .Include(e => e.Reservation)
                .Where(e => e.EntryTime != null && e.ExitTime == null
                    && e.EntryTime.Value < cutoffTime)
                .ToListAsync();

            foreach (var e in overstays)
            {
                // 同时检查数据库和内存中是否已有记录
                var existsDb = await _db.ViolationRecords.AnyAsync(v =>
                    v.UserId == e.UserId && v.ViolationType == "overstay"
                    && v.SourceId == e.Id && v.Location == area.AreaName);
                var existsLocal = _db.ViolationRecords.Local.Any(v =>
                    v.UserId == e.UserId && v.ViolationType == "overstay"
                    && v.SourceId == e.Id && v.Location == area.AreaName);

                if (!existsDb && !existsLocal)
                {
                    var duration = now - (e.EntryTime ?? now);
                    _db.ViolationRecords.Add(new ViolationRecord
                    {
                        UserId = e.UserId,
                        ViolationType = "overstay",
                        Description = $"访客{e.Reservation.VisitorName}自{e.EntryTime:yyyy-MM-dd HH:mm}入校后，" +
                            $"{area.AreaName}已于{area.CloseTime:hh\\:mm}关闭，已超时滞留{(int)duration.TotalHours}小时",
                        OccurredAt = e.EntryTime ?? now,
                        Location = area.AreaName,
                        Severity = "major",
                        SourceType = "system",
                        SourceId = e.Id,
                    });
                }
            }
        }

        // === 3. 访问权限违规检测：游客类型不匹配区域权限 ===
        var currentVisitors = await _db.EntryExitRecords
            .Include(e => e.Reservation)
            .Where(e => e.EntryTime != null && e.ExitTime == null)
            .ToListAsync();

        var allAreaPermissions = await _db.AreaPermissions
            .Include(p => p.Area)
            .ToListAsync();

        foreach (var visitor in currentVisitors)
        {
            var visitorType = visitor.Reservation.VisitorType;

            // 找出该访客类型无权进入的区域
            var forbiddenAreas = allAreaPermissions
                .GroupBy(p => p.AreaId)
                .Where(g => !g.Any(p => p.VisitorType == visitorType))
                .Select(g => g.First().Area)
                .Where(a => a != null && a.AccessLevel == "forbidden")
                .ToList();

            foreach (var area in forbiddenAreas)
            {
                var exists = await _db.ViolationRecords.AnyAsync(v =>
                    v.UserId == visitor.UserId && v.ViolationType == "trespass"
                    && v.Location == area.Name && v.OccurredAt.Date == today);
                if (!exists)
                {
                    _db.ViolationRecords.Add(new ViolationRecord
                    {
                        UserId = visitor.UserId,
                        ViolationType = "trespass",
                        Description = $"访客{visitor.Reservation.VisitorName}（{visitorType}）" +
                            $"可能进入了禁止区域「{area.Name}」（该区域不对此类型访客开放）",
                        OccurredAt = now,
                        Location = area.Name,
                        Severity = "minor",
                        SourceType = "system",
                        SourceId = visitor.Id,
                    });
                }
            }
        }

        await _db.SaveChangesAsync();
        await AutoBlacklistAsync();
    }

    /// <summary>
    /// 自动拉黑：累计违规≥3次
    /// </summary>
    private async Task AutoBlacklistAsync()
    {
        var violators = await _db.ViolationRecords
            .GroupBy(v => v.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .Where(x => x.Count >= 3)
            .ToListAsync();

        foreach (var v in violators)
        {
            var alreadyBlacklisted = await _db.Blacklists
                .AnyAsync(b => b.UserId == v.UserId && b.IsActive);
            if (!alreadyBlacklisted)
            {
                var user = await _db.Users.FindAsync(v.UserId);
                _db.Blacklists.Add(new Blacklist
                {
                    UserId = v.UserId,
                    Reason = $"系统自动检测：累计违规{v.Count}次",
                    ViolationCount = v.Count,
                    OperatorId = 1, // 系统自动操作
                    BlacklistedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMonths(3),
                    IsActive = true,
                });
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task<Reservation> SearchAsync(string query)
    {
        var reservation = await _db.Reservations
            .Include(r => r.EntryExitRecords)
            .FirstOrDefaultAsync(r =>
                r.ReservationNo == query ||
                r.VisitorPhone == query ||
                r.VisitorName.Contains(query))
            ?? throw new KeyNotFoundException("未找到匹配的预约记录");

        return reservation;
    }

    public async Task<EntryExitRecord> EntryAsync(int operatorId, EntryCheckRequest request)
    {
        var reservation = await _db.Reservations.FindAsync(request.Id)
            ?? throw new KeyNotFoundException("预约不存在");

        if (reservation.Status != "approved")
            throw new InvalidOperationException("该预约未通过审核，不可入校");

        var gate = await _db.Gates.FirstOrDefaultAsync(g => g.Name == request.Gate)
            ?? await _db.Gates.FirstAsync();

        reservation.Status = "checked_in";

        var record = new EntryExitRecord
        {
            ReservationId = reservation.Id,
            UserId = reservation.UserId,
            EntryTime = DateTime.UtcNow,
            EntryGateId = gate.Id,
            OperatorId = operatorId,
        };

        _db.EntryExitRecords.Add(record);
        await _db.SaveChangesAsync();
        return record;
    }

    public async Task<EntryExitRecord> ExitAsync(int operatorId, ExitRecordRequest request)
    {
        var record = await _db.EntryExitRecords
            .Include(r => r.Reservation)
            .FirstOrDefaultAsync(r =>
                r.Reservation.VisitorName == request.Name &&
                r.ExitTime == null)
            ?? throw new KeyNotFoundException("未找到该访客的入校记录");

        var gate = await _db.Gates.FirstOrDefaultAsync(g => g.Name == request.Gate)
            ?? await _db.Gates.FirstAsync();

        record.ExitTime = DateTime.UtcNow;
        record.ExitGateId = gate.Id;
        record.Reservation.Status = "checked_out";

        await _db.SaveChangesAsync();
        return record;
    }

    public async Task<List<EntryExitRecord>> GetCurrentVisitorsAsync()
    {
        return await _db.EntryExitRecords
            .Include(r => r.Reservation)
            .Include(r => r.EntryGate)
            .Where(r => r.EntryTime != null && r.ExitTime == null)
            .OrderByDescending(r => r.EntryTime)
            .ToListAsync();
    }

    public async Task<List<EntryExitRecord>> GetRecordsAsync(int page, int pageSize)
    {
        return await _db.EntryExitRecords
            .Include(r => r.Reservation)
            .Include(r => r.EntryGate)
            .Include(r => r.ExitGate)
            .OrderByDescending(r => r.EntryTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
