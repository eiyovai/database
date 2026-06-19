using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusVisitorApi.Models;

[Table("Reports")]
public class Report
{
    [Key]
    public int Id { get; set; }

    public int ReporterId { get; set; }

    [Required, MaxLength(100)]
    public string TargetName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string ViolationType { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    public DateTime OccurredAt { get; set; }

    [Required, MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public string? Evidence { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "pending";

    public int? ReviewerId { get; set; }

    [MaxLength(500)]
    public string? ReviewRemark { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ReporterId))]
    public User Reporter { get; set; } = null!;

    [ForeignKey(nameof(ReviewerId))]
    public User? Reviewer { get; set; }
}
