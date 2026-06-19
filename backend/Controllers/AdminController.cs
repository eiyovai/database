using CampusVisitorApi.DTOs;
using CampusVisitorApi.Models;
using CampusVisitorApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusVisitorApi.Controllers;

[ApiController]
[Route("api")]
[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _service;

    public AdminController(IAdminService service) => _service = service;

    // === Dashboard ===
    [HttpGet("admin/dashboard")]
    public async Task<ActionResult<DashboardStatsResponse>> GetDashboard()
        => Ok(await _service.GetDashboardStatsAsync());

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
        await _service.RemoveBlacklistAsync(id);
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
        // Simplified: delete + recreate
        await _service.DeleteScheduleAsync(id);
        await _service.CreateScheduleAsync(request);
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
    public async Task<ActionResult<List<AuditLog>>> GetAuditLogs(
        [FromQuery] string? keyword, [FromQuery] string? actionType,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _service.GetAuditLogsAsync(keyword, actionType, page, pageSize));
}
