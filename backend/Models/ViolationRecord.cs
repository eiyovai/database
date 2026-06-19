using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusVisitorApi.Models;

[Table("ViolationRecords")]
public class ViolationRecord
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required, MaxLength(20)]
    public string ViolationType { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public DateTime OccurredAt { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    [Required, MaxLength(10)]
    public string Severity { get; set; } = "minor";

    [Required, MaxLength(20)]
    public string SourceType { get; set; } = "system";

    public int? SourceId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
