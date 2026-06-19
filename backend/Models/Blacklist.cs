using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusVisitorApi.Models;

[Table("Blacklist")]
public class Blacklist
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    public int ViolationCount { get; set; } = 0;

    public DateTime BlacklistedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;

    public int OperatorId { get; set; }

    public DateTime? UnblacklistedAt { get; set; }

    [MaxLength(500)]
    public string? UnblacklistReason { get; set; }

    // Navigation
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(OperatorId))]
    public User Operator { get; set; } = null!;
}
