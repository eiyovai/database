using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusVisitorApi.Models;

[Table("ActivityRegistrations")]
public class ActivityRegistration
{
    [Key]
    public int Id { get; set; }

    public int ActivityId { get; set; }
    public int UserId { get; set; }

    [Required, MaxLength(50)]
    public string VisitorName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string VisitorPhone { get; set; } = string.Empty;

    public int Companions { get; set; } = 0;

    [Required, MaxLength(20)]
    public string Status { get; set; } = "registered";

    public DateTime? CheckedInAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ActivityId))]
    public Activity Activity { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
