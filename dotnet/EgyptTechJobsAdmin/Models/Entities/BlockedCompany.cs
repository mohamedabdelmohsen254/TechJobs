using System.ComponentModel.DataAnnotations;

namespace EgyptTechJobsAdmin.Models.Entities;

/// <summary>
/// Companies whose jobs should be hidden from public job listings
/// </summary>
public class BlockedCompany
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}
