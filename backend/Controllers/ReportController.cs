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
    [Authorize]
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

        // 记录审计日志
        var reporterId = report.ReporterId;
        _db.AuditLogs.Add(new AuditLog
        {
            OperatorId = reporterId,
            ActionType = "report",
            ActionDetail = $"提交举报：{request.Target} - {request.ViolationType}",
            TargetType = "Report",
            TargetId = report.Id,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Result = "success",
        });
        await _db.SaveChangesAsync();

        return Ok(new { message = "举报已提交" });
    }

    [HttpGet]
    [Authorize(Roles = "admin,security")]
    public async Task<ActionResult<List<Report>>> GetList([FromQuery] string? status)
    {
        var query = _db.Reports.AsQueryable();
        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status == status);
        return Ok(await query.ToListAsync());
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

        // 审核通过时创建违规记录
        var autoBlacklisted = false;
        if (request.Status == "approved")
        {
            var targetUser = await _db.Users.FirstOrDefaultAsync(u => u.Name == report.TargetName);
            if (targetUser != null)
            {
                var violation = new ViolationRecord
                {
                    UserId = targetUser.Id,
                    ViolationType = report.ViolationType,
                    Description = report.Description,
                    OccurredAt = report.OccurredAt,
                    Location = report.Location,
                    Severity = "major",
                    SourceType = "report",
                    SourceId = report.Id,
                };
                _db.ViolationRecords.Add(violation);

                // 累计违规次数达3次则自动拉黑
                var violationCount = await _db.ViolationRecords.CountAsync(v => v.UserId == targetUser.Id) + 1;
                if (violationCount >= 3)
                {
                    var existing = await _db.Blacklists.FirstOrDefaultAsync(b => b.UserId == targetUser.Id && b.IsActive);
                    if (existing == null)
                    {
                        _db.Blacklists.Add(new Blacklist
                        {
                            UserId = targetUser.Id,
                            Reason = $"累计违规{violationCount}次，自动拉黑",
                            ViolationCount = violationCount,
                            BlacklistedAt = DateTime.UtcNow,
                            IsActive = true,
                            OperatorId = reviewerId,
                        });
                        autoBlacklisted = true;
                    }
                }
            }
        }

        await _db.SaveChangesAsync();

        // 记录审计日志
        var actionText = request.Status == "approved" ? "审核通过举报" : "审核驳回举报";
        if (autoBlacklisted) actionText += "（自动拉黑）";
        _db.AuditLogs.Add(new AuditLog
        {
            OperatorId = reviewerId,
            ActionType = "review",
            ActionDetail = $"{actionText}：{report.TargetName} - {report.ViolationType}",
            TargetType = "Report",
            TargetId = report.Id,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Result = "success",
        });
        await _db.SaveChangesAsync();

        return Ok(new { message = "审核完成" });
    }
}
