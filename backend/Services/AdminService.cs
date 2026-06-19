using CampusVisitorApi.Data;
using CampusVisitorApi.DTOs;
using CampusVisitorApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusVisitorApi.Services;

public interface IAdminService
{
    // Open Rules
    Task<List<OpenRule>> GetOpenRulesAsync();
    Task<OpenRule> SaveOpenRuleAsync(SaveOpenRuleRequest request);
    Task DeleteOpenRuleAsync(int id);

    // Areas
    Task<List<CampusArea>> GetAreasAsync();
    Task<CampusArea> SaveAreaAsync(SaveAreaRequest request);
    Task DeleteAreaAsync(int id);

    // Blacklist
    Task<List<Blacklist>> GetBlacklistAsync();
    Task RemoveBlacklistAsync(int id);

    // Schedules
    Task<List<StaffSchedule>> GetSchedulesAsync();
    Task<StaffSchedule> CreateScheduleAsync(CreateScheduleRequest request);
    Task DeleteScheduleAsync(int id);

    // Reports
    Task<List<Report>> GetReportsAsync(string? status);
    Task ReviewReportAsync(int reviewerId, int id, ReviewReportRequest request);

    // Audit Logs
    Task<List<AuditLog>> GetAuditLogsAsync(string? keyword, string? actionType, int page, int pageSize);

    // Dashboard
    Task<DashboardStatsResponse> GetDashboardStatsAsync();
}

public class AdminService : IAdminService
{
    private readonly CampusVisitorDbContext _db;

    public AdminService(CampusVisitorDbContext db) => _db = db;

    public async Task<List<OpenRule>> GetOpenRulesAsync()
        => await _db.OpenRules.OrderByDescending(r => r.CreatedAt).ToListAsync();

    public async Task<OpenRule> SaveOpenRuleAsync(SaveOpenRuleRequest request)
    {
        var rule = new OpenRule
        {
            DateType = request.DateType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TimeSlot = request.TimeSlot,
            MaxCapacity = request.MaxCapacity,
            IsActive = request.IsActive,
            Remark = request.Remark,
        };
        _db.OpenRules.Add(rule);
        await _db.SaveChangesAsync();
        return rule;
    }

    public async Task DeleteOpenRuleAsync(int id)
    {
        var rule = await _db.OpenRules.FindAsync(id) ?? throw new KeyNotFoundException();
        _db.OpenRules.Remove(rule);
        await _db.SaveChangesAsync();
    }

    public async Task<List<CampusArea>> GetAreasAsync()
        => await _db.CampusAreas.Include(a => a.AreaPermissions).ToListAsync();

    public async Task<CampusArea> SaveAreaAsync(SaveAreaRequest request)
    {
        var area = new CampusArea
        {
            Name = request.Name,
            Code = request.Code,
            Type = request.Type,
            AccessLevel = request.AccessLevel,
            Description = request.Description,
        };
        _db.CampusAreas.Add(area);
        await _db.SaveChangesAsync();

        foreach (var t in request.AllowedTypes)
            _db.AreaPermissions.Add(new AreaPermission { AreaId = area.Id, VisitorType = t });
        await _db.SaveChangesAsync();

        return area;
    }

    public async Task DeleteAreaAsync(int id)
    {
        var area = await _db.CampusAreas.FindAsync(id) ?? throw new KeyNotFoundException();
        _db.CampusAreas.Remove(area);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Blacklist>> GetBlacklistAsync()
        => await _db.Blacklists.Include(b => b.User).Where(b => b.IsActive).ToListAsync();

    public async Task RemoveBlacklistAsync(int id)
    {
        var item = await _db.Blacklists.FindAsync(id) ?? throw new KeyNotFoundException();
        item.IsActive = false;
        item.UnblacklistedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<List<StaffSchedule>> GetSchedulesAsync()
        => await _db.StaffSchedules.Include(s => s.Staff).OrderBy(s => s.WorkDate).ToListAsync();

    public async Task<StaffSchedule> CreateScheduleAsync(CreateScheduleRequest request)
    {
        var staff = await _db.Users.FirstOrDefaultAsync(u => u.Name == request.StaffName && u.Role == "staff")
            ?? throw new KeyNotFoundException("未找到该工作人员");

        var schedule = new StaffSchedule
        {
            StaffId = staff.Id,
            StaffRole = request.StaffRole,
            WorkDate = request.WorkDate,
            Shift = request.Shift,
            Location = request.Location,
            Task = request.Task,
            CreatedBy = staff.Id,
        };
        _db.StaffSchedules.Add(schedule);
        await _db.SaveChangesAsync();
        return schedule;
    }

    public async Task DeleteScheduleAsync(int id)
    {
        var s = await _db.StaffSchedules.FindAsync(id) ?? throw new KeyNotFoundException();
        _db.StaffSchedules.Remove(s);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Report>> GetReportsAsync(string? status)
    {
        var query = _db.Reports.AsQueryable();
        if (!string.IsNullOrEmpty(status)) query = query.Where(r => r.Status == status);
        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task ReviewReportAsync(int reviewerId, int id, ReviewReportRequest request)
    {
        var report = await _db.Reports.FindAsync(id) ?? throw new KeyNotFoundException();
        report.Status = request.Status;
        report.ReviewerId = reviewerId;
        report.ReviewRemark = request.Remark;
        report.ReviewedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetAuditLogsAsync(string? keyword, string? actionType, int page, int pageSize)
    {
        var query = _db.AuditLogs.AsQueryable();
        if (!string.IsNullOrEmpty(actionType)) query = query.Where(l => l.ActionType == actionType);
        if (!string.IsNullOrEmpty(keyword)) query = query.Where(l => l.ActionDetail.Contains(keyword));

        return await query.OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();
    }

    public async Task<DashboardStatsResponse> GetDashboardStatsAsync()
    {
        var today = DateTime.Today;
        return new DashboardStatsResponse
        {
            TodayReservations = await _db.Reservations.CountAsync(r => r.VisitDate == today),
            CurrentVisitors = await _db.EntryExitRecords.CountAsync(r =>
                r.EntryTime != null && r.ExitTime == null),
            PendingReviews = await _db.Reservations.CountAsync(r => r.Status == "pending"),
            BlacklistCount = await _db.Blacklists.CountAsync(b => b.IsActive),
        };
    }
}
