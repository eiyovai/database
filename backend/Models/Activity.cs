using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusVisitorApi.Models;

[Table("Activities")]
public class Activity
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public int MaxParticipants { get; set; } = 100;
    public int CurrentCount { get; set; } = 0;

    [Required, MaxLength(20)]
    public string Status { get; set; } = "draft";

    [MaxLength(500)]
    public string? CoverImage { get; set; }

    [MaxLength(50)]
    public string? ContactPerson { get; set; }

    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(CreatedBy))]
    public User Creator { get; set; } = null!;

    public ICollection<ActivityRegistration> Registrations { get; set; } = new List<ActivityRegistration>();
}
