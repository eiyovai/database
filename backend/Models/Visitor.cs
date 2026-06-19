using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusVisitorApi.Models;

[Table("Visitors")]
public class Visitor
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [MaxLength(20)]
    public string? IdCard { get; set; }

    [MaxLength(4)]
    public string? Gender { get; set; }

    public DateTime? BirthDate { get; set; }

    [MaxLength(100)]
    public string? Affiliation { get; set; }

    [Required, MaxLength(20)]
    public string VisitorType { get; set; } = "tourist";

    [MaxLength(50)]
    public string? EmergencyContact { get; set; }

    [MaxLength(20)]
    public string? EmergencyPhone { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }

    // Navigation
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
