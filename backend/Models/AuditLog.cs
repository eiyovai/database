using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusVisitorApi.Models;

[Table("AuditLogs")]
public class AuditLog
{
    [Key]
    public long Id { get; set; }

    public int? OperatorId { get; set; }

    [Required, MaxLength(30)]
    public string ActionType { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string ActionDetail { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? TargetType { get; set; }

    public int? TargetId { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [Required, MaxLength(10)]
    public string Result { get; set; } = "success";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(OperatorId))]
    public User? Operator { get; set; }
}
