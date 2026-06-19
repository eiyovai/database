using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusVisitorApi.Models;

[Table("ReservationStatusLog")]
public class ReservationStatusLog
{
    [Key]
    public int Id { get; set; }

    public int ReservationId { get; set; }

    [MaxLength(20)]
    public string? FromStatus { get; set; }

    [Required, MaxLength(20)]
    public string ToStatus { get; set; } = string.Empty;

    public int? OperatorId { get; set; }

    [MaxLength(500)]
    public string? Remark { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ReservationId))]
    public Reservation Reservation { get; set; } = null!;

    [ForeignKey(nameof(OperatorId))]
    public User? Operator { get; set; }
}
