using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusVisitorApi.Models;

[Table("Users")]
public class User
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required, MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Role { get; set; } = "visitor";

    [MaxLength(100)]
    public string? Email { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    // Navigation
    public Visitor? Visitor { get; set; }
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<EntryExitRecord> EntryExitRecords { get; set; } = new List<EntryExitRecord>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<StaffSchedule> StaffSchedules { get; set; } = new List<StaffSchedule>();
    public ICollection<ViolationRecord> ViolationRecords { get; set; } = new List<ViolationRecord>();
    public ICollection<ActivityRegistration> ActivityRegistrations { get; set; } = new List<ActivityRegistration>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}
