using AutoMapper;
using CampusVisitorApi.Data;
using CampusVisitorApi.DTOs;
using CampusVisitorApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusVisitorApi.Services;

public interface IActivityService
{
    Task<List<ActivityListResponse>> GetPublicAsync(string? keyword, string? status);
    Task<ActivityListResponse> GetDetailAsync(int id);
    Task RegisterAsync(int userId, RegisterActivityRequest request);
    Task CancelRegistrationAsync(int userId, int registrationId);
    Task<List<ActivityListResponse>> GetMyRegistrationsAsync(int userId);

    // Admin
    Task<List<ActivityListResponse>> GetListAsync();
    Task CreateAsync(int userId, CreateActivityRequest request);
    Task UpdateAsync(int id, CreateActivityRequest request);
    Task DeleteAsync(int id);
    Task CheckInAsync(int registrationId);
}

public class ActivityService : IActivityService
{
    private readonly CampusVisitorDbContext _db;
    private readonly IMapper _mapper;

    public ActivityService(CampusVisitorDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<ActivityListResponse>> GetPublicAsync(string? keyword, string? status)
    {
        await AutoCloseExpiredAsync();
        var query = _db.Activities.Where(a => a.Status == "open").AsQueryable();

        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(a => a.Title.Contains(keyword));

        var items = await query.OrderBy(a => a.StartTime).ToListAsync();
        return _mapper.Map<List<ActivityListResponse>>(items);
    }

    public async Task<ActivityListResponse> GetDetailAsync(int id)
    {
        await AutoCloseExpiredAsync();
        var activity = await _db.Activities.FindAsync(id)
            ?? throw new KeyNotFoundException("活动不存在");
        return _mapper.Map<ActivityListResponse>(activity);
    }

    public async Task RegisterAsync(int userId, RegisterActivityRequest request)
    {
        var activity = await _db.Activities.FindAsync(request.ActivityId)
            ?? throw new KeyNotFoundException("活动不存在");

        if (activity.CurrentCount >= activity.MaxParticipants)
            throw new InvalidOperationException("报名人数已满");

        if (await _db.ActivityRegistrations.AnyAsync(r =>
                r.ActivityId == request.ActivityId && r.UserId == userId))
            throw new InvalidOperationException("您已报名该活动");

        var registration = new ActivityRegistration
        {
            ActivityId = request.ActivityId,
            UserId = userId,
            VisitorName = (await _db.Users.FindAsync(userId))?.Name ?? "",
            VisitorPhone = (await _db.Users.FindAsync(userId))?.Phone ?? "",
        };

        activity.CurrentCount++;
        _db.ActivityRegistrations.Add(registration);
        await _db.SaveChangesAsync();
    }

    public async Task CancelRegistrationAsync(int userId, int registrationId)
    {
        var reg = await _db.ActivityRegistrations
            .Include(r => r.Activity)
            .FirstOrDefaultAsync(r => r.Id == registrationId && r.UserId == userId)
            ?? throw new KeyNotFoundException("报名记录不存在");

        reg.Status = "cancelled";
        reg.Activity.CurrentCount--;
        await _db.SaveChangesAsync();
    }

    public async Task<List<ActivityListResponse>> GetMyRegistrationsAsync(int userId)
    {
        await AutoCloseExpiredAsync();
        var regs = await _db.ActivityRegistrations
            .Include(r => r.Activity)
            .Where(r => r.UserId == userId && r.Status == "registered")
            .Select(r => r.Activity)
            .ToListAsync();

        return _mapper.Map<List<ActivityListResponse>>(regs);
    }

    public async Task<List<ActivityListResponse>> GetListAsync()
    {
        await AutoCloseExpiredAsync();
        var items = await _db.Activities.OrderByDescending(a => a.CreatedAt).ToListAsync();
        return _mapper.Map<List<ActivityListResponse>>(items);
    }

    public async Task CreateAsync(int userId, CreateActivityRequest request)
    {
        var activity = new Activity
        {
            Title = request.Title,
            Location = request.Location,
            Description = request.Description,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            MaxParticipants = request.MaxParticipants,
            Status = "open",
            CreatedBy = userId,
        };
        _db.Activities.Add(activity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(int id, CreateActivityRequest request)
    {
        var activity = await _db.Activities.FindAsync(id)
            ?? throw new KeyNotFoundException("活动不存在");
        activity.Title = request.Title;
        activity.Location = request.Location;
        activity.Description = request.Description;
        activity.StartTime = request.StartTime;
        activity.EndTime = request.EndTime;
        activity.MaxParticipants = request.MaxParticipants;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var activity = await _db.Activities.FindAsync(id)
            ?? throw new KeyNotFoundException("活动不存在");
        _db.Activities.Remove(activity);
        await _db.SaveChangesAsync();
    }

    public async Task CheckInAsync(int registrationId)
    {
        var reg = await _db.ActivityRegistrations.FindAsync(registrationId)
            ?? throw new KeyNotFoundException("报名记录不存在");
        reg.Status = "checked_in";
        reg.CheckedInAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// 自动关闭已过期的活动
    /// </summary>
    private async Task AutoCloseExpiredAsync()
    {
        var expired = await _db.Activities
            .Where(a => a.Status == "open" && a.EndTime < DateTime.UtcNow)
            .ToListAsync();

        if (expired.Count > 0)
        {
            foreach (var a in expired) a.Status = "closed";
            await _db.SaveChangesAsync();
        }
    }
}
