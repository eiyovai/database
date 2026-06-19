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
    private readonly CampusVisitorDbContext _db;

    public AdminController(IAdminService service, CampusVisitorDbContext db)
    {
        _service = service;
        _db = db;
    }

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

    [HttpPost("blacklist")]
    public async Task<ActionResult> AddBlacklist([FromBody] AddBlacklistRequest request)
    {
        var operatorId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        try
        {
            var item = await _service.AddBlacklistAsync(operatorId, request);

            // 记录审计日志
            _db.AuditLogs.Add(new AuditLog
            {
                OperatorId = operatorId,
                ActionType = "blacklist",
                ActionDetail = $"手动将 {request.UserName} 加入黑名单",
                TargetType = "Blacklist",
                TargetId = item.Id,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Result = "success",
            });
            await _db.SaveChangesAsync();

            return Ok(new { message = "已加入黑名单" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

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

    // === Users ===
    [HttpGet("users")]
    public async Task<ActionResult<List<User>>> GetUsers()
    {
        var users = await _db.Users.Where(u => u.Role == "visitor").Select(u => new { u.Id, u.Name, u.Phone }).ToListAsync();
        return Ok(users);
    }

    // === Audit Logs ===
    [HttpGet("audit-logs")]
    public async Task<ActionResult<List<AuditLog>>> GetAuditLogs(
        [FromQuery] string? keyword, [FromQuery] string? actionType,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _service.GetAuditLogsAsync(keyword, actionType, page, pageSize));
}
