using System.ComponentModel.DataAnnotations;

namespace EgyptTechJobsAdmin.Models.Entities;

public class Job
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Company { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(50)]
    public string? WorkType { get; set; } // Remote, Hybrid, Onsite

    [Required]
    [MaxLength(1000)]
    public string ApplyUrl { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Source { get; set; }

    [MaxLength(200)]
    public string? JobId { get; set; } // External job ID

    public DateTime? PostedDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public bool IsManualEntry { get; set; } = false;

    public bool IsVisibleToUsers { get; set; } = true; // Controls visibility on public site

    [MaxLength(5000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? SalaryRange { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; } // Comma-separated tags
}
