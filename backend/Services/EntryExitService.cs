using CampusVisitorApi.Data;
using CampusVisitorApi.DTOs;
using CampusVisitorApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusVisitorApi.Services;

public interface IEntryExitService
{
    Task<Reservation> SearchAsync(string query);
    Task<EntryExitRecord> EntryAsync(int operatorId, EntryCheckRequest request);
    Task<EntryExitRecord> ExitAsync(int operatorId, ExitRecordRequest request);
    Task<List<EntryExitRecord>> GetCurrentVisitorsAsync();
}

public class EntryExitService : IEntryExitService
{
    private readonly CampusVisitorDbContext _db;

    public EntryExitService(CampusVisitorDbContext db) => _db = db;

    public async Task<Reservation> SearchAsync(string query)
    {
        var reservation = await _db.Reservations
            .Include(r => r.EntryExitRecords)
            .FirstOrDefaultAsync(r =>
                r.ReservationNo == query ||
                r.VisitorPhone == query ||
                r.VisitorName.Contains(query))
            ?? throw new KeyNotFoundException("未找到匹配的预约记录");

        return reservation;
    }

    public async Task<EntryExitRecord> EntryAsync(int operatorId, EntryCheckRequest request)
    {
        var reservation = await _db.Reservations.FindAsync(request.Id)
            ?? throw new KeyNotFoundException("预约不存在");

        if (reservation.Status != "approved")
            throw new InvalidOperationException("该预约未通过审核，不可入校");

        var gate = await _db.Gates.FirstOrDefaultAsync(g => g.Name == request.Gate)
            ?? await _db.Gates.FirstAsync();

        reservation.Status = "checked_in";

        var record = new EntryExitRecord
        {
            ReservationId = reservation.Id,
            UserId = reservation.UserId,
            EntryTime = DateTime.UtcNow,
            EntryGateId = gate.Id,
            OperatorId = operatorId,
        };

        _db.EntryExitRecords.Add(record);
        await _db.SaveChangesAsync();
        return record;
    }

    public async Task<EntryExitRecord> ExitAsync(int operatorId, ExitRecordRequest request)
    {
        var record = await _db.EntryExitRecords
            .Include(r => r.Reservation)
            .FirstOrDefaultAsync(r =>
                r.Reservation.VisitorName == request.Name &&
                r.ExitTime == null)
            ?? throw new KeyNotFoundException("未找到该访客的入校记录");

        var gate = await _db.Gates.FirstOrDefaultAsync(g => g.Name == request.Gate)
            ?? await _db.Gates.FirstAsync();

        record.ExitTime = DateTime.UtcNow;
        record.ExitGateId = gate.Id;
        record.Reservation.Status = "checked_out";

        await _db.SaveChangesAsync();
        return record;
    }

    public async Task<List<EntryExitRecord>> GetCurrentVisitorsAsync()
    {
        return await _db.EntryExitRecords
            .Include(r => r.Reservation)
            .Include(r => r.EntryGate)
            .Where(r => r.EntryTime != null && r.ExitTime == null)
            .OrderByDescending(r => r.EntryTime)
            .ToListAsync();
    }
}
