using System.Security.Claims;
using CampusVisitorApi.DTOs;
using CampusVisitorApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusVisitorApi.Controllers;

[ApiController]
[Route("api/activities")]
public class ActivityController : ControllerBase
{
    private readonly IActivityService _service;

    public ActivityController(IActivityService service) => _service = service;

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
        await _service.RegisterAsync(userId, request);
        return Ok(new { message = "报名成功" });
    }

    [Authorize]
    [HttpGet("my-registrations")]
    public async Task<ActionResult<List<ActivityListResponse>>> GetMyRegistrations()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _service.GetMyRegistrationsAsync(userId));
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
        return Ok(new { message = "活动已发布" });
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] CreateActivityRequest request)
    {
        await _service.UpdateAsync(id, request);
        return Ok(new { message = "活动已更新" });
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(new { message = "活动已删除" });
    }

    [Authorize(Roles = "admin")]
    [HttpPut("checkin/{registrationId}")]
    public async Task<ActionResult> CheckIn(int registrationId)
    {
        await _service.CheckInAsync(registrationId);
        return Ok(new { message = "签到成功" });
    }
}
