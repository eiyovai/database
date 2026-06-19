using CampusVisitorApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusVisitorApi.Controllers;

[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly CampusVisitorDbContext _db;

    public PublicController(CampusVisitorDbContext db) => _db = db;

    [HttpGet("campus-status")]
    public async Task<ActionResult> GetCampusStatus()
    {
        var today = DateTime.Today;

        var openRule = await _db.OpenRules
            .Where(r => r.IsActive && r.StartDate <= today && r.EndDate >= today)
            .OrderByDescending(r => r.MaxCapacity)
            .FirstOrDefaultAsync();

        var todayReservations = await _db.Reservations
            .CountAsync(r => r.VisitDate == today);

        var currentVisitors = await _db.EntryExitRecords
            .CountAsync(r => r.EntryTime != null && r.ExitTime == null);

        var maxCapacity = openRule?.MaxCapacity ?? 500;

        return Ok(new
        {
            isOpen = openRule != null,
            todayReservations,
            currentVisitors,
            maxCapacity,
        });
    }
}
