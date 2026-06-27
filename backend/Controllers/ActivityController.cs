using System.Security.Claims;
using CampusVisitorApi.Data;
using CampusVisitorApi.DTOs;
using CampusVisitorApi.Models;
using CampusVisitorApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusVisitorApi.Controllers;

[ApiController]
[Route("api/activities")]
public class ActivityController : ControllerBase
{
    private readonly IActivityService _service;
    private readonly CampusVisitorDbContext _db;

    public ActivityController(IActivityService service, CampusVisitorDbContext db)
    {
        _service = service;
        _db = db;
    }

    [HttpGet("public")]
    public async Task<ActionResult<List<ActivityListResponse>>> GetPublic(
        [FromQuery] string? keyword, [FromQuery] string? status)
    {
        return Ok(await _service.GetPublicAsync(keyword, status));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ActivityListResponse>> GetDetail(int id)
    {
        try
        {
            return Ok(await _service.GetDetailAsync(id));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterActivityRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _service.RegisterAsync(userId, request);
            _db.AuditLogs.Add(new AuditLog
            {
                OperatorId = userId, ActionType = "register",
                ActionDetail = $"报名活动：ActivityId={request.ActivityId}",
                TargetType = "ActivityRegistration", TargetId = request.ActivityId,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), Result = "success",
            });
            await _db.SaveChangesAsync();
            return Ok(new { message = "报名成功" });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("my-registrations")]
    public async Task<ActionResult<List<ActivityListResponse>>> GetMyRegistrations()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _service.GetMyRegistrationsAsync(userId));
    }

    [Authorize]
    [HttpPut("{id}/cancel-registration")]
    public async Task<ActionResult> CancelRegistration(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.CancelRegistrationAsync(userId, id);
        return Ok(new { message = "已取消报名" });
    }

    // === 管理员接口 ===
    [Authorize(Roles = "admin")]
    [HttpGet]
    public async Task<ActionResult<List<ActivityListResponse>>> GetList()
    {
        return Ok(await _service.GetListAsync());
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateActivityRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.CreateAsync(userId, request);
        _db.AuditLogs.Add(new AuditLog
        {
            OperatorId = userId, ActionType = "create",
            ActionDetail = $"发布活动：{request.Title}", TargetType = "Activity",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), Result = "success",
        });
        await _db.SaveChangesAsync();
        return Ok(new { message = "活动已发布" });
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] CreateActivityRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.UpdateAsync(id, request);
        _db.AuditLogs.Add(new AuditLog
        {
            OperatorId = userId, ActionType = "update",
            ActionDetail = $"更新活动：ID={id}", TargetType = "Activity", TargetId = id,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), Result = "success",
        });
        await _db.SaveChangesAsync();
        return Ok(new { message = "活动已更新" });
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.DeleteAsync(id);
        _db.AuditLogs.Add(new AuditLog
        {
            OperatorId = userId, ActionType = "delete",
            ActionDetail = $"删除活动：ID={id}", TargetType = "Activity", TargetId = id,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), Result = "success",
        });
        await _db.SaveChangesAsync();
        return Ok(new { message = "活动已删除" });
    }

    [Authorize(Roles = "admin")]
    [HttpPut("checkin/{registrationId}")]
    public async Task<ActionResult> CheckIn(int registrationId)
    {
        await _service.CheckInAsync(registrationId);
        return Ok(new { message = "签到成功" });
    }

    // === 报名审核 ===
    [Authorize(Roles = "admin")]
    [HttpGet("registrations/pending")]
    public async Task<ActionResult<List<ActivityRegistrationResponse>>> GetAllPendingRegistrations()
    {
        return Ok(await _service.GetAllPendingRegistrationsAsync());
    }

    [Authorize(Roles = "admin")]
    [HttpGet("{id}/registrations")]
    public async Task<ActionResult<List<ActivityRegistrationResponse>>> GetRegistrations(int id)
    {
        return Ok(await _service.GetPendingRegistrationsAsync(id));
    }

    [Authorize(Roles = "admin")]
    [HttpPut("registrations/{registrationId}/approve")]
    public async Task<ActionResult> ApproveRegistration(int registrationId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _service.ApproveRegistrationAsync(registrationId);
            _db.AuditLogs.Add(new AuditLog
            {
                OperatorId = userId, ActionType = "review",
                ActionDetail = $"审核通过活动报名：RegistrationId={registrationId}",
                TargetType = "ActivityRegistration", TargetId = registrationId,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), Result = "success",
            });
            await _db.SaveChangesAsync();
            return Ok(new { message = "报名已通过" });
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

    [Authorize(Roles = "admin")]
    [HttpPut("registrations/{registrationId}/reject")]
    public async Task<ActionResult> RejectRegistration(int registrationId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _service.RejectRegistrationAsync(registrationId);
            _db.AuditLogs.Add(new AuditLog
            {
                OperatorId = userId, ActionType = "review",
                ActionDetail = $"拒绝活动报名：RegistrationId={registrationId}",
                TargetType = "ActivityRegistration", TargetId = registrationId,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), Result = "success",
            });
            await _db.SaveChangesAsync();
            return Ok(new { message = "报名已拒绝" });
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

}
