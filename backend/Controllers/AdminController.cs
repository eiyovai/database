using System.Security.Claims;
using CampusVisitorApi.Data;
using CampusVisitorApi.DTOs;
using CampusVisitorApi.Models;
using CampusVisitorApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusVisitorApi.Controllers;

[ApiController]
[Route("api")]
[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _service;
    private readonly IEntryExitService _entryExitService;
    private readonly CampusVisitorDbContext _db;

    public AdminController(IAdminService service, IEntryExitService entryExitService, CampusVisitorDbContext db)
    {
        _service = service;
        _entryExitService = entryExitService;
        _db = db;
    }

    // === Dashboard ===
    [HttpGet("admin/dashboard")]
    public async Task<ActionResult<DashboardStatsResponse>> GetDashboard()
    {
        await _entryExitService.AutoDetectViolationsAsync(); // 自动检测违规
        return Ok(await _service.GetDashboardStatsAsync());
    }

    // === Open Rules ===
    [HttpGet("open-rules")]
    public async Task<ActionResult<List<OpenRule>>> GetOpenRules()
        => Ok(await _service.GetOpenRulesAsync());

    [HttpPost("open-rules")]
    public async Task<ActionResult> SaveOpenRule([FromBody] SaveOpenRuleRequest request)
    {
        await _service.SaveOpenRuleAsync(request);
        return Ok(new { message = "保存成功" });
    }

    [HttpDelete("open-rules/{id}")]
    public async Task<ActionResult> DeleteOpenRule(int id)
    {
        await _service.DeleteOpenRuleAsync(id);
        return Ok(new { message = "已删除" });
    }

    // === Areas ===
    [HttpGet("areas")]
    public async Task<ActionResult<List<CampusArea>>> GetAreas()
        => Ok(await _service.GetAreasAsync());

    [HttpPost("areas")]
    public async Task<ActionResult> SaveArea([FromBody] SaveAreaRequest request)
    {
        await _service.SaveAreaAsync(request);
        return Ok(new { message = "保存成功" });
    }

    [HttpDelete("areas/{id}")]
    public async Task<ActionResult> DeleteArea(int id)
    {
        await _service.DeleteAreaAsync(id);
        return Ok(new { message = "已删除" });
    }

    // === Blacklist ===
    [HttpGet("blacklist")]
    public async Task<ActionResult<List<Blacklist>>> GetBlacklist()
        => Ok(await _service.GetBlacklistAsync());

    [HttpDelete("blacklist/{id}")]
    public async Task<ActionResult> RemoveBlacklist(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.RemoveBlacklistAsync(id);
        _db.AuditLogs.Add(new AuditLog
        {
            OperatorId = userId, ActionType = "update",
            ActionDetail = $"移出黑名单：BlacklistId={id}",
            TargetType = "Blacklist", TargetId = id,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), Result = "success",
        });
        await _db.SaveChangesAsync();
        return Ok(new { message = "已移出黑名单" });
    }

    // === Schedules ===
    [HttpGet("schedules")]
    public async Task<ActionResult<List<StaffSchedule>>> GetSchedules()
        => Ok(await _service.GetSchedulesAsync());

    [HttpPost("schedules")]
    public async Task<ActionResult> CreateSchedule([FromBody] CreateScheduleRequest request)
    {
        await _service.CreateScheduleAsync(request);
        return Ok(new { message = "排班已创建" });
    }

    [HttpPut("schedules/{id}")]
    public async Task<ActionResult> UpdateSchedule(int id, [FromBody] CreateScheduleRequest request)
    {
        await _service.UpdateScheduleAsync(id, request);
        return Ok(new { message = "排班已更新" });
    }

    [HttpDelete("schedules/{id}")]
    public async Task<ActionResult> DeleteSchedule(int id)
    {
        await _service.DeleteScheduleAsync(id);
        return Ok(new { message = "已删除" });
    }

    // === Audit Logs ===
    [HttpGet("audit-logs")]
    public async Task<ActionResult<PagedResult<AuditLog>>> GetAuditLogs(
        [FromQuery] string? keyword, [FromQuery] string? actionType,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _service.GetAuditLogsAsync(keyword, actionType, page, pageSize));
}
