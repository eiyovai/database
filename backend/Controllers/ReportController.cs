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

        report.Status = request.Status;
        report.ReviewRemark = request.Remark;
        report.ReviewerId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        report.ReviewedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { message = "审核完成" });
    }
}
