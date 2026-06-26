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
[Route("api/reservations")]
[Authorize]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _service;
    private readonly CampusVisitorDbContext _db;

    public ReservationController(IReservationService service, CampusVisitorDbContext db)
    {
        _service = service;
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateReservationRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.CreateAsync(userId, request);
        return Ok(new { message = "预约申请已提交" });
    }

    [HttpGet("my")]
    public async Task<ActionResult<PagedResult<ReservationListResponse>>> GetMy(
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _service.GetMyReservationsAsync(userId, status, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReservationListResponse>> GetDetail(int id)
    {
        try
        {
            var result = await _service.GetDetailAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/cancel")]
    public async Task<ActionResult> Cancel(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.CancelAsync(userId, id);

        _db.AuditLogs.Add(new AuditLog
        {
            OperatorId = userId,
            ActionType = "other",
            ActionDetail = $"取消预约：ID={id}",
            TargetType = "Reservation",
            TargetId = id,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Result = "success",
        });
        await _db.SaveChangesAsync();

        return Ok(new { message = "预约已取消" });
    }

    // === 管理员接口 ===

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<PagedResult<ReservationListResponse>>> GetList(
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 15)
    {
        var result = await _service.GetListAsync(status, page, pageSize);
        return Ok(result);
    }

    [HttpGet("stats")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult> GetStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var query = _db.Reservations.AsQueryable();
        if (startDate.HasValue) query = query.Where(r => r.VisitDate >= startDate.Value);
        if (endDate.HasValue) query = query.Where(r => r.VisitDate <= endDate.Value);

        var total = await query.CountAsync();
        var approved = await query.CountAsync(r => r.Status == "approved");
        var pending = await query.CountAsync(r => r.Status == "pending");
        var checkedIn = await query.CountAsync(r => r.Status == "checked_in");
        var cancelled = await query.CountAsync(r => r.Status == "cancelled" || r.Status == "rejected");

        return Ok(new { total, approved, pending, checkedIn, cancelled });
    }

    [HttpPut("{id}/review")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult> Review(int id, [FromBody] ReviewReservationRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.ReviewAsync(userId, id, request);

        // 记录审计日志
        var statusText = request.Status == "approved" ? "审核通过" : "审核拒绝";
        _db.AuditLogs.Add(new AuditLog
        {
            OperatorId = userId,
            ActionType = "review",
            ActionDetail = $"{statusText}预约：ID={id}",
            TargetType = "Reservation",
            TargetId = id,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Result = "success",
        });
        await _db.SaveChangesAsync();

        return Ok(new { message = "操作成功" });
    }
}
