using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusVisitorApi.Models;

[Table("EntryExitRecords")]
public class EntryExitRecord
{
    [Key]
    public int Id { get; set; }

    public int ReservationId { get; set; }
    public int UserId { get; set; }

    public DateTime? EntryTime { get; set; }
    public int? EntryGateId { get; set; }

    public DateTime? ExitTime { get; set; }
    public int? ExitGateId { get; set; }

    public int? OperatorId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ReservationId))]
    public Reservation Reservation { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(EntryGateId))]
    public Gate? EntryGate { get; set; }

    [ForeignKey(nameof(ExitGateId))]
    public Gate? ExitGate { get; set; }

    [ForeignKey(nameof(OperatorId))]
    public User? Operator { get; set; }
}
