using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusVisitorApi.Models;

[Table("CampusAreas")]
public class CampusArea
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Type { get; set; } = "public";

    [Required, MaxLength(20)]
    public string AccessLevel { get; set; } = "public";

    /// <summary>默认上午开放时间（可被 OpenRule 覆盖）</summary>
    public TimeSpan? MorningStart { get; set; }

    /// <summary>默认上午关闭时间</summary>
    public TimeSpan? MorningEnd { get; set; }

    /// <summary>默认下午开放时间</summary>
    public TimeSpan? AfternoonStart { get; set; }

    /// <summary>默认下午关闭时间（用于违规检测）</summary>
    public TimeSpan? AfternoonEnd { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<AreaPermission> AreaPermissions { get; set; } = new List<AreaPermission>();
    public ICollection<OpenRule> OpenRules { get; set; } = new List<OpenRule>();
}
