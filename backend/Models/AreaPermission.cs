using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusVisitorApi.Models;

[Table("AreaPermissions")]
public class AreaPermission
{
    [Key]
    public int Id { get; set; }

    public int AreaId { get; set; }

    [Required, MaxLength(20)]
    public string VisitorType { get; set; } = string.Empty;

    // Navigation
    [ForeignKey(nameof(AreaId))]
    public CampusArea Area { get; set; } = null!;
}
