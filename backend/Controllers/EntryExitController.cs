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
[Route("api/entry-exit")]
[Authorize(Roles = "security")]
public class EntryExitController : ControllerBase
{
    private readonly IEntryExitService _service;
    private readonly CampusVisitorDbContext _db;

    public EntryExitController(IEntryExitService service, CampusVisitorDbContext db)
    {
        _service = service;
        _db = db;
    }

    [HttpGet("stats")]
    public async Task<ActionResult> GetStats()
    {
        var today = DateTime.Today;
        return Ok(new
        {
            todayEntries = await _db.EntryExitRecords.CountAsync(e => e.EntryTime != null && e.EntryTime.Value.Date == today),
            todayExits = await _db.EntryExitRecords.CountAsync(e => e.ExitTime != null && e.ExitTime.Value.Date == today),
            currentVisitors = await _db.EntryExitRecords.CountAsync(e => e.EntryTime != null && e.ExitTime == null),
        });
    }

    [HttpPost("search")]
    public async Task<ActionResult<Reservation>> Search([FromBody] EntryCheckRequest request)
    {
        try
        {
            return Ok(await _service.SearchAsync(request.Query ?? ""));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("entry")]
    public async Task<ActionResult<EntryExitRecord>> Entry([FromBody] EntryCheckRequest request)
    {
        var operatorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var record = await _service.EntryAsync(operatorId, request);
            return Ok(record);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("exit")]
    public async Task<ActionResult<EntryExitRecord>> Exit([FromBody] ExitRecordRequest request)
    {
        var operatorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var record = await _service.ExitAsync(operatorId, request);
            return Ok(record);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("recent")]
    public async Task<ActionResult> GetRecent()
    {
        var records = await _db.EntryExitRecords
            .Include(e => e.Reservation)
            .Include(e => e.EntryGate)
            .Where(e => e.EntryTime != null && e.ExitTime == null)
            .OrderByDescending(e => e.EntryTime)
            .Take(5)
            .Select(e => new
            {
                name = e.Reservation.VisitorName,
                gate = e.EntryGate!.Name,
                entryTime = e.EntryTime,
            })
            .ToListAsync();
        return Ok(records);
    }

    [HttpGet("current")]
    public async Task<ActionResult<List<EntryExitRecord>>> GetCurrent()
    {
        await _service.AutoDetectViolationsAsync(); // 自动检测爽约和超时
        return Ok(await _service.GetCurrentVisitorsAsync());
    }

    [HttpGet("records")]
    public async Task<ActionResult<List<EntryExitRecord>>> GetRecords(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _service.GetRecordsAsync(page, pageSize));
    }
}
