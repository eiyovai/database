using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusVisitorApi.Models;

[Table("OpenRules")]
public class OpenRule
{
    [Key]
    public int Id { get; set; }

    /// <summary>关联区域（null=全院通用规则）</summary>
    public int? AreaId { get; set; }

    [Required, MaxLength(20)]
    public string DateType { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    [Required, MaxLength(20)]
    public string TimeSlot { get; set; } = string.Empty;

    public TimeSpan? MorningStart { get; set; }
    public TimeSpan? MorningEnd { get; set; }
    public TimeSpan? AfternoonStart { get; set; }
    public TimeSpan? AfternoonEnd { get; set; }

    public int MaxCapacity { get; set; } = 500;
    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Remark { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(AreaId))]
    public CampusArea? Area { get; set; }
}
