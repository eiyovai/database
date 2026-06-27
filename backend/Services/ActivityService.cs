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
    Task<List<ActivityRegistrationResponse>> GetAllPendingRegistrationsAsync();
    Task<List<ActivityRegistrationResponse>> GetPendingRegistrationsAsync(int activityId);
    Task ApproveRegistrationAsync(int registrationId);
    Task RejectRegistrationAsync(int registrationId);
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

        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("用户不存在");

        var registration = new ActivityRegistration
        {
            ActivityId = request.ActivityId,
            UserId = userId,
            VisitorName = user.Name,
            VisitorPhone = user.Phone,
            Status = "pending",   // 待审核
        };

        _db.ActivityRegistrations.Add(registration);
        await _db.SaveChangesAsync();
    }

    public async Task<List<ActivityRegistrationResponse>> GetAllPendingRegistrationsAsync()
    {
        return await _db.ActivityRegistrations
            .Include(r => r.User)
            .Include(r => r.Activity)
            .Where(r => r.Status == "pending")
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ActivityRegistrationResponse
            {
                Id = r.Id,
                ActivityId = r.ActivityId,
                ActivityTitle = r.Activity.Title,
                UserId = r.UserId,
                VisitorName = r.VisitorName,
                VisitorPhone = r.VisitorPhone,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
            })
            .ToListAsync();
    }

    public async Task<List<ActivityRegistrationResponse>> GetPendingRegistrationsAsync(int activityId)
    {
        return await _db.ActivityRegistrations
            .Include(r => r.User)
            .Where(r => r.ActivityId == activityId && r.Status == "pending")
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ActivityRegistrationResponse
            {
                Id = r.Id,
                ActivityId = r.ActivityId,
                UserId = r.UserId,
                VisitorName = r.VisitorName,
                VisitorPhone = r.VisitorPhone,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
            })
            .ToListAsync();
    }

    public async Task ApproveRegistrationAsync(int registrationId)
    {
        var reg = await _db.ActivityRegistrations
            .Include(r => r.Activity)
            .FirstOrDefaultAsync(r => r.Id == registrationId)
            ?? throw new KeyNotFoundException("报名记录不存在");

        if (reg.Status != "pending")
            throw new InvalidOperationException("该报名已处理");

        var activity = reg.Activity;

        // 审核通过：递增人数
        activity.CurrentCount++;
        reg.Status = "registered";

        // 自动创建关联的入校预约
        var alreadyHasReservation = await _db.Reservations
            .AnyAsync(r => r.UserId == reg.UserId && r.VisitDate == activity.StartTime.Date
                && r.Status != "cancelled" && r.Status != "rejected");
        if (!alreadyHasReservation)
        {
            var last = await _db.Reservations
                .OrderByDescending(r => r.Id)
                .Select(r => r.ReservationNo)
                .FirstOrDefaultAsync();

            var seq = 1;
            if (last != null)
            {
                var todayPrefix = $"R{DateTime.Now:yyyyMMdd}";
                if (last.StartsWith(todayPrefix))
                    seq = int.Parse(last[^4..]) + 1;
            }

            var hour = activity.StartTime.Hour;
            var timeSlot = hour < 12 ? "morning" : hour < 17 ? "afternoon" : "full_day";

            var user = await _db.Users.FindAsync(reg.UserId);
            var visitor = await _db.Visitors.FirstOrDefaultAsync(v => v.UserId == reg.UserId);
            var visitorType = visitor?.VisitorType ?? "tourist";

            var reservation = new Reservation
            {
                UserId = reg.UserId,
                ReservationNo = $"R{DateTime.Now:yyyyMMdd}{seq:D4}",
                VisitorType = visitorType,
                VisitorName = reg.VisitorName,
                VisitorPhone = reg.VisitorPhone,
                VisitDate = activity.StartTime.Date,
                TimeSlot = timeSlot,
                Companions = 0,
                StayDuration = "activity",
                Purpose = $"参加活动：{activity.Title}",
                Status = "approved",
                ReviewerId = 1,
                ReviewedAt = DateTime.UtcNow,
            };

            _db.Reservations.Add(reservation);
        }

        await _db.SaveChangesAsync();
    }

    public async Task RejectRegistrationAsync(int registrationId)
    {
        var reg = await _db.ActivityRegistrations.FindAsync(registrationId)
            ?? throw new KeyNotFoundException("报名记录不存在");

        if (reg.Status != "pending")
            throw new InvalidOperationException("该报名已处理");

        reg.Status = "rejected";
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
