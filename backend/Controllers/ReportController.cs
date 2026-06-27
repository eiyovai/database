using CampusVisitorApi.Data;
using CampusVisitorApi.DTOs;
using CampusVisitorApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusVisitorApi.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportController : ControllerBase
{
    private readonly CampusVisitorDbContext _db;

    public ReportController(CampusVisitorDbContext db) => _db = db;

    [HttpPost]
    [Authorize(Roles = "visitor,security,admin")]
    public async Task<ActionResult> Create([FromBody] CreateReportRequest request)
    {
        var report = new Report
        {
            ReporterId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value),
            TargetName = request.Target,
            ViolationType = request.ViolationType,
            Location = request.Location,
            OccurredAt = request.OccurredAt,
            Description = request.Description,
        };
        _db.Reports.Add(report);
        await _db.SaveChangesAsync();
        return Ok(new { message = "举报已提交" });
    }

    [HttpGet]
    [Authorize(Roles = "admin,security,visitor")]
    public async Task<ActionResult<List<Report>>> GetList([FromQuery] string? status, [FromQuery] bool? my)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var isAdminOrSecurity = User.IsInRole("admin") || User.IsInRole("security");

        var query = _db.Reports
            .Include(r => r.Reporter)
            .AsQueryable();

        // 访客只能看自己的举报
        if (!isAdminOrSecurity || my == true)
            query = query.Where(r => r.ReporterId == userId);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status == status);

        return Ok(await query.OrderByDescending(r => r.CreatedAt).ToListAsync());
    }

    [HttpPut("{id}/review")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult> Review(int id, [FromBody] ReviewReportRequest request)
    {
        var report = await _db.Reports.FindAsync(id);
        if (report == null) return NotFound(new { message = "举报不存在" });

        var reviewerId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        report.Status = request.Status;
        report.ReviewRemark = request.Remark;
        report.ReviewerId = reviewerId;
        report.ReviewedAt = DateTime.UtcNow;

        // 审核通过：自动创建违规记录 → 累计次数 → 自动拉黑
        if (request.Status == "approved")
        {
            // 查找被举报用户
            var targetUser = await _db.Users.FirstOrDefaultAsync(u => u.Name == report.TargetName);

            // 创建违规记录
            var violation = new ViolationRecord
            {
                UserId = targetUser?.Id ?? 0,
                ViolationType = report.ViolationType,
                Description = report.Description,
                OccurredAt = report.OccurredAt,
                Location = report.Location,
                Severity = "minor",
                SourceType = "report",
                SourceId = report.Id,
            };
            _db.ViolationRecords.Add(violation);

            if (targetUser != null)
            {
                // 统计该用户累计违规次数
                var totalViolations = await _db.ViolationRecords
                    .CountAsync(v => v.UserId == targetUser.Id);

                // 检查是否已在黑名单中
                var existingBlacklist = await _db.Blacklists
                    .FirstOrDefaultAsync(b => b.UserId == targetUser.Id && b.IsActive);

                if (existingBlacklist != null)
                {
                    // 已在黑名单，更新违规次数
                    existingBlacklist.ViolationCount = totalViolations;
                    existingBlacklist.Reason = $"累计违规{totalViolations}次（含：{report.ViolationType}）";
                }
                else if (totalViolations >= 3)
                {
                    // 累计违规≥3次，自动加入黑名单
                    _db.Blacklists.Add(new Blacklist
                    {
                        UserId = targetUser.Id,
                        Reason = $"累计违规{totalViolations}次（含：{report.ViolationType}）",
                        ViolationCount = totalViolations,
                        OperatorId = reviewerId,
                        BlacklistedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddMonths(3), // 3个月后自动解禁
                        IsActive = true,
                    });
                }
            }
        }

        var statusText = request.Status == "approved" ? "审核通过" : "审核驳回";
        _db.AuditLogs.Add(new AuditLog
        {
            OperatorId = reviewerId, ActionType = "review",
            ActionDetail = $"{statusText}举报：{report.TargetName}（{report.ViolationType}）",
            TargetType = "Report", TargetId = report.Id,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), Result = "success",
        });

        await _db.SaveChangesAsync();
        return Ok(new { message = "审核完成" });
    }
}
