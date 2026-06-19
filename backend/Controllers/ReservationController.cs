using System.Security.Claims;
using CampusVisitorApi.DTOs;
using CampusVisitorApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusVisitorApi.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _service;

    public ReservationController(IReservationService service) => _service = service;

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

    [HttpPut("{id}/review")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult> Review(int id, [FromBody] ReviewReservationRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.ReviewAsync(userId, id, request);
        return Ok(new { message = "操作成功" });
    }
}
