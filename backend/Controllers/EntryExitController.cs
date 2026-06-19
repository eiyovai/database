using System.Security.Claims;
using CampusVisitorApi.DTOs;
using CampusVisitorApi.Models;
using CampusVisitorApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusVisitorApi.Controllers;

[ApiController]
[Route("api/entry-exit")]
[Authorize(Roles = "security")]
public class EntryExitController : ControllerBase
{
    private readonly IEntryExitService _service;

    public EntryExitController(IEntryExitService service) => _service = service;

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

    [HttpGet("current")]
    public async Task<ActionResult<List<EntryExitRecord>>> GetCurrent()
    {
        return Ok(await _service.GetCurrentVisitorsAsync());
    }
}
