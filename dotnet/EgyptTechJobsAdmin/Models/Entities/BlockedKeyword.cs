using System.ComponentModel.DataAnnotations;

namespace EgyptTechJobsAdmin.Models.Entities;

/// <summary>
/// Keywords that, if found in job titles, will hide the job from public listings
/// </summary>
public class BlockedKeyword
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Keyword { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}
