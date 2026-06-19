using AutoMapper;
using CampusVisitorApi.Data;
using CampusVisitorApi.DTOs;
using CampusVisitorApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusVisitorApi.Services;

public class ReservationService : IReservationService
{
    private readonly CampusVisitorDbContext _db;
    private readonly IMapper _mapper;

    public ReservationService(CampusVisitorDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task CreateAsync(int userId, CreateReservationRequest request)
    {
        var last = await _db.Reservations
            .OrderByDescending(r => r.Id)
            .Select(r => r.ReservationNo)
            .FirstOrDefaultAsync();

        var seq = 1;
        if (last != null) seq = int.Parse(last[^4..]) + 1;

        var reservation = new Reservation
        {
            UserId = userId,
            ReservationNo = $"R{DateTime.Now:yyyyMMdd}{seq:D4}",
            VisitorType = request.VisitorType,
            VisitorName = request.Name,
            VisitorPhone = request.Phone,
            VisitDate = request.VisitDate,
            TimeSlot = request.TimeSlot,
            Companions = request.Companions,
            StayDuration = request.StayDuration,
            Purpose = request.Purpose,
            Status = "pending",
        };

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();
    }

    public async Task<PagedResult<ReservationListResponse>> GetMyReservationsAsync(
        int userId, string? status, int page, int pageSize)
    {
        var query = _db.Reservations
            .Include(r => r.EntryExitRecords).ThenInclude(e => e.EntryGate)
            .Where(r => r.UserId == userId).AsQueryable();

        if (!string.IsNullOrEmpty(status) && status != "all")
        {
            if (status == "closed")
                query = query.Where(r => r.Status == "rejected" || r.Status == "cancelled");
            else
                query = query.Where(r => r.Status == status);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        return new PagedResult<ReservationListResponse>
        {
            Items = _mapper.Map<List<ReservationListResponse>>(items),
            Total = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<ReservationListResponse> GetDetailAsync(int id)
    {
        var reservation = await _db.Reservations
            .Include(r => r.EntryExitRecords).ThenInclude(e => e.EntryGate)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException("预约不存在");

        return _mapper.Map<ReservationListResponse>(reservation);
    }

    public async Task CancelAsync(int userId, int id)
    {
        var reservation = await _db.Reservations.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId)
            ?? throw new KeyNotFoundException("预约不存在");

        if (reservation.Status != "pending")
            throw new InvalidOperationException("只能取消待审核的预约");

        reservation.Status = "cancelled";
        await _db.SaveChangesAsync();
    }

    public async Task<PagedResult<ReservationListResponse>> GetListAsync(
        string? status, int page, int pageSize)
    {
        var query = _db.Reservations
            .Include(r => r.EntryExitRecords).ThenInclude(e => e.EntryGate)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status == status);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        return new PagedResult<ReservationListResponse>
        {
            Items = _mapper.Map<List<ReservationListResponse>>(items),
            Total = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task ReviewAsync(int reviewerId, int id, ReviewReservationRequest request)
    {
        var reservation = await _db.Reservations.FindAsync(id)
            ?? throw new KeyNotFoundException("预约不存在");

        if (reservation.Status != "pending")
            throw new InvalidOperationException("该预约已被审核");

        reservation.Status = request.Status;
        reservation.ReviewerId = reviewerId;
        reservation.ReviewRemark = request.Remark;
        reservation.ReviewedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
