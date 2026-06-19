using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusVisitorApi.Models;

[Table("StaffSchedules")]
public class StaffSchedule
{
    [Key]
    public int Id { get; set; }

    public int StaffId { get; set; }

    [Required, MaxLength(20)]
    public string StaffRole { get; set; } = string.Empty;

    public DateTime WorkDate { get; set; }

    [Required, MaxLength(20)]
    public string Shift { get; set; } = string.Empty;

    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    [Required, MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Task { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(StaffId))]
    public User Staff { get; set; } = null!;

    [ForeignKey(nameof(CreatedBy))]
    public User Creator { get; set; } = null!;
}
