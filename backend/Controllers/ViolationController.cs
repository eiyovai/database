using CampusVisitorApi.Data;
using CampusVisitorApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusVisitorApi.Controllers;

[ApiController]
[Route("api/violations")]
[Authorize(Roles = "admin,security")]
public class ViolationController : ControllerBase
{
    private readonly CampusVisitorDbContext _db;

    public ViolationController(CampusVisitorDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<ViolationRecord>>> GetList([FromQuery] string? source)
    {
        var query = _db.ViolationRecords
            .Include(v => v.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(source))
            query = query.Where(v => v.SourceType == source);

        return Ok(await query
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync());
    }
}
