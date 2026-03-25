using System.ComponentModel.DataAnnotations;

namespace EgyptTechJobsAdmin.Models.Entities;

public class AuditLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty; // Create, Update, Delete

    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    public int EntityId { get; set; }

    [MaxLength(100)]
    public string? PerformedBy { get; set; }

    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(2000)]
    public string? Details { get; set; }
}
