using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusVisitorApi.Models;

[Table("Reservations")]
public class Reservation
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required, MaxLength(30)]
    public string ReservationNo { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string VisitorType { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string VisitorName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string VisitorPhone { get; set; } = string.Empty;

    public DateTime VisitDate { get; set; }

    [Required, MaxLength(20)]
    public string TimeSlot { get; set; } = string.Empty;

    public int Companions { get; set; } = 0;

    [MaxLength(20)]
    public string? StayDuration { get; set; }

    [Required, MaxLength(500)]
    public string Purpose { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Status { get; set; } = "pending";

    public int? ReviewerId { get; set; }

    [MaxLength(500)]
    public string? ReviewRemark { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(ReviewerId))]
    public User? Reviewer { get; set; }

    public ICollection<ReservationStatusLog> StatusLogs { get; set; } = new List<ReservationStatusLog>();
    public ICollection<EntryExitRecord> EntryExitRecords { get; set; } = new List<EntryExitRecord>();
}
