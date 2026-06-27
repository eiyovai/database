using Microsoft.EntityFrameworkCore;
using CampusVisitorApi.Models;

namespace CampusVisitorApi.Data;

public class CampusVisitorDbContext : DbContext
{
    public CampusVisitorDbContext(DbContextOptions<CampusVisitorDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Visitor> Visitors => Set<Visitor>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ReservationStatusLog> ReservationStatusLogs => Set<ReservationStatusLog>();
    public DbSet<CampusArea> CampusAreas => Set<CampusArea>();
    public DbSet<AreaPermission> AreaPermissions => Set<AreaPermission>();
    public DbSet<Gate> Gates => Set<Gate>();
    public DbSet<OpenRule> OpenRules => Set<OpenRule>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ActivityRegistration> ActivityRegistrations => Set<ActivityRegistration>();
    public DbSet<EntryExitRecord> EntryExitRecords => Set<EntryExitRecord>();
    public DbSet<ViolationRecord> ViolationRecords => Set<ViolationRecord>();
    public DbSet<Blacklist> Blacklists => Set<Blacklist>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<StaffSchedule> StaffSchedules => Set<StaffSchedule>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Users
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Phone).IsUnique();
            e.HasOne(u => u.Visitor).WithOne(v => v.User).HasForeignKey<Visitor>(v => v.UserId);
        });

        // Visitors
        modelBuilder.Entity<Visitor>(e =>
        {
            e.HasOne(v => v.User).WithOne(u => u.Visitor).HasForeignKey<Visitor>(v => v.UserId);
        });

        // Reservations
        modelBuilder.Entity<Reservation>(e =>
        {
            e.ToTable(tb => tb.HasTrigger("trg_Reservations_StatusChange"));
            e.HasIndex(r => r.ReservationNo).IsUnique();
            e.HasIndex(r => r.UserId);
            e.HasIndex(r => r.Status);
            e.HasIndex(r => r.VisitDate);
            e.HasOne(r => r.User).WithMany(u => u.Reservations).HasForeignKey(r => r.UserId);
            e.HasOne(r => r.Reviewer).WithMany().HasForeignKey(r => r.ReviewerId).OnDelete(DeleteBehavior.NoAction);
        });

        // ReservationStatusLog
        modelBuilder.Entity<ReservationStatusLog>(e =>
        {
            e.HasIndex(l => l.ReservationId);
            e.HasOne(l => l.Reservation).WithMany(r => r.StatusLogs).HasForeignKey(l => l.ReservationId);
            e.HasOne(l => l.Operator).WithMany().HasForeignKey(l => l.OperatorId).OnDelete(DeleteBehavior.NoAction);
        });

        // CampusAreas
        modelBuilder.Entity<CampusArea>(e =>
        {
            e.HasIndex(a => a.Code).IsUnique();
            e.HasMany(a => a.OpenRules).WithOne(r => r.Area).HasForeignKey(r => r.AreaId).OnDelete(DeleteBehavior.SetNull);
        });

        // AreaPermissions
        modelBuilder.Entity<AreaPermission>(e =>
        {
            e.HasIndex(p => new { p.AreaId, p.VisitorType }).IsUnique();
            e.HasOne(p => p.Area).WithMany(a => a.AreaPermissions).HasForeignKey(p => p.AreaId);
        });

        // Gates
        modelBuilder.Entity<Gate>(e =>
        {
            e.HasIndex(g => g.Code).IsUnique();
        });

        // Activities
        modelBuilder.Entity<Activity>(e =>
        {
            e.HasOne(a => a.Creator).WithMany().HasForeignKey(a => a.CreatedBy).OnDelete(DeleteBehavior.NoAction);
        });

        // ActivityRegistrations
        modelBuilder.Entity<ActivityRegistration>(e =>
        {
            e.HasIndex(r => new { r.ActivityId, r.UserId }).IsUnique();
            e.HasOne(r => r.Activity).WithMany(a => a.Registrations).HasForeignKey(r => r.ActivityId);
            e.HasOne(r => r.User).WithMany(u => u.ActivityRegistrations).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.NoAction);
        });

        // EntryExitRecords
        modelBuilder.Entity<EntryExitRecord>(e =>
        {
            e.HasIndex(r => r.UserId);
            e.HasIndex(r => r.EntryTime);
            e.HasOne(r => r.Reservation).WithMany(r => r.EntryExitRecords).HasForeignKey(r => r.ReservationId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(r => r.User).WithMany(u => u.EntryExitRecords).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(r => r.EntryGate).WithMany().HasForeignKey(r => r.EntryGateId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(r => r.ExitGate).WithMany().HasForeignKey(r => r.ExitGateId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(r => r.Operator).WithMany().HasForeignKey(r => r.OperatorId).OnDelete(DeleteBehavior.NoAction);
        });

        // ViolationRecords
        modelBuilder.Entity<ViolationRecord>(e =>
        {
            e.HasIndex(v => v.UserId);
            e.HasOne(v => v.User).WithMany(u => u.ViolationRecords).HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.NoAction);
        });

        // Blacklist
        modelBuilder.Entity<Blacklist>(e =>
        {
            e.HasIndex(b => b.UserId).IsUnique();
            e.HasOne(b => b.User).WithMany().HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(b => b.Operator).WithMany().HasForeignKey(b => b.OperatorId).OnDelete(DeleteBehavior.NoAction);
        });

        // Reports
        modelBuilder.Entity<Report>(e =>
        {
            e.HasIndex(r => r.Status);
            e.HasOne(r => r.Reporter).WithMany(u => u.Reports).HasForeignKey(r => r.ReporterId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(r => r.Reviewer).WithMany().HasForeignKey(r => r.ReviewerId).OnDelete(DeleteBehavior.NoAction);
        });

        // StaffSchedules
        modelBuilder.Entity<StaffSchedule>(e =>
        {
            e.HasIndex(s => new { s.StaffId, s.WorkDate, s.Shift }).IsUnique();
            e.HasOne(s => s.Staff).WithMany(u => u.StaffSchedules).HasForeignKey(s => s.StaffId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(s => s.Creator).WithMany().HasForeignKey(s => s.CreatedBy).OnDelete(DeleteBehavior.NoAction);
        });

        // AuditLogs
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasIndex(l => l.OperatorId);
            e.HasIndex(l => l.CreatedAt);
            e.HasIndex(l => l.ActionType);
            e.HasOne(l => l.Operator).WithMany(u => u.AuditLogs).HasForeignKey(l => l.OperatorId).OnDelete(DeleteBehavior.NoAction);
        });
    }
}
